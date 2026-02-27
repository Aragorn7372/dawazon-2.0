using CSharpFunctionalExtensions;
using dawazon2._0.Models;
using dawazon2._0.MvcControllers;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Errors;
using dawazonBackend.Cart.Models;
using dawazonBackend.Cart.Service;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace dawazonTest.dawazon2._0.MvcControllers;

[TestFixture]
public class ManagerMvcControllerTests
{
    private Mock<ICartService> _cartServiceMock;
    private ManagerMvcController _controller;

    [SetUp]
    public void Setup()
    {
        _cartServiceMock = new Mock<ICartService>();

        _controller = new ManagerMvcController(_cartServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, UserRoles.MANAGER)
        }, "mock"));

        var httpContext = new DefaultHttpContext() { User = user };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    private void SetupManagerUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, UserRoles.MANAGER)
        }, "mock"));

        var httpContext = new DefaultHttpContext() { User = user };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Test]
    public async Task Sales_ShouldReturnViewWithSales()
    {
        SetupManagerUser();
        var mockSales = new List<SaleLineDto>
        {
            new SaleLineDto { SaleId = "cart1", ProductId = "product1", ProductName = "Product 1", Quantity = 1, ProductPrice = 10, TotalPrice = 10, Status = Status.Preparado, ManagerId = 1, ManagerName = "Manager 1", UserId = 2, CreateAt = DateTime.UtcNow, UpdateAt = DateTime.UtcNow }
        };
        var mockPage = new PageResponseDto<SaleLineDto>(mockSales, 1, 10, 1, 0, 10, "createAt", "desc");

        _cartServiceMock.Setup(s => s.FindAllSalesAsLinesAsync(1, false, It.IsAny<FilterDto>()))
            .ReturnsAsync(mockPage);
        _cartServiceMock.Setup(s => s.CalculateTotalEarningsAsync(1, false))
            .ReturnsAsync(100.0);

        var result = await _controller.Sales() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<AdminSaleListViewModel>());
    }

    [Test]
    public async Task Sales_WhenNoSales_ShouldReturnViewWithEmptyList()
    {
        SetupManagerUser();
        var mockPage = new PageResponseDto<SaleLineDto>(new List<SaleLineDto>(), 1, 10, 0, 0, 10, "createAt", "desc");

        _cartServiceMock.Setup(s => s.FindAllSalesAsLinesAsync(1, false, It.IsAny<FilterDto>()))
            .ReturnsAsync(mockPage);
        _cartServiceMock.Setup(s => s.CalculateTotalEarningsAsync(1, false))
            .ReturnsAsync(0.0);

        var result = await _controller.Sales() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<AdminSaleListViewModel>());
    }

    [Test]
    public async Task SaleEdit_Get_WhenCartAndLineExist_ShouldReturnViewWithModel()
    {
        SetupManagerUser();
        var mockCart = new CartResponseDto("cart1", 2, true, new ClientDto { Name = "Client", Email = "client@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, 
            new List<SaleLineDto> { new SaleLineDto { SaleId = "cart1", ProductId = "product1", ProductName = "Product 1", Quantity = 2, ProductPrice = 10, TotalPrice = 20, Status = Status.Preparado, ManagerId = 1, ManagerName = "Manager 1", UserId = 2, CreateAt = DateTime.UtcNow, UpdateAt = DateTime.UtcNow } }, 2, 20.0);

        _cartServiceMock.Setup(s => s.GetByIdAsync("cart1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.SaleEdit("cart1", "product1") as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<AdminSaleEditViewModel>());
        var vm = result.Model as AdminSaleEditViewModel;
        Assert.That(vm.ProductName, Is.EqualTo("Product 1"));
        Assert.That(vm.Quantity, Is.EqualTo(2));
    }

    [Test]
    public async Task SaleEdit_Get_WhenCartNotFound_ShouldReturnNotFound()
    {
        SetupManagerUser();
        _cartServiceMock.Setup(s => s.GetByIdAsync("cart1"))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.SaleEdit("cart1", "product1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task SaleEdit_Get_WhenLineNotFound_ShouldReturnNotFound()
    {
        SetupManagerUser();
        var mockCart = new CartResponseDto("cart1", 2, true, new ClientDto { Name = "Client", Email = "client@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, 
            new List<SaleLineDto>(), 0, 0.0);

        _cartServiceMock.Setup(s => s.GetByIdAsync("cart1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.SaleEdit("cart1", "product1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task SaleEdit_Post_WhenValid_ShouldUpdateStatusAndRedirect()
    {
        SetupManagerUser();
        var vm = new AdminSaleEditViewModel { SaleId = "cart1", ProductId = "product1", NewStatus = Status.Enviado };

        _cartServiceMock.Setup(s => s.UpdateSaleStatusAsync("cart1", "product1", Status.Enviado, 1, false))
            .ReturnsAsync((DomainError?)null);

        var result = await _controller.SaleEdit("cart1", "product1", vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Sales"));
        Assert.That(_controller.TempData["Success"], Is.EqualTo("Estado modificado correctamente."));
    }

    [Test]
    public async Task SaleEdit_Post_WhenUpdateFails_ShouldReturnViewWithError()
    {
        SetupManagerUser();
        var vm = new AdminSaleEditViewModel { SaleId = "cart1", ProductId = "product1", NewStatus = Status.Enviado };

        _cartServiceMock.Setup(s => s.UpdateSaleStatusAsync("cart1", "product1", Status.Enviado, 1, false))
            .ReturnsAsync(new CartError("Update failed"));

        var result = await _controller.SaleEdit("cart1", "product1", vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task SaleEdit_Post_WhenModelInvalid_ShouldReturnView()
    {
        SetupManagerUser();
        _controller.ModelState.AddModelError("NewStatus", "Required");

        var vm = new AdminSaleEditViewModel { SaleId = "cart1", ProductId = "product1", NewStatus = Status.Enviado };
        var result = await _controller.SaleEdit("cart1", "product1", vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<AdminSaleEditViewModel>());
    }

    [Test]
    public async Task SaleCancel_WhenCancelSucceeds_ShouldRedirectWithSuccessMessage()
    {
        SetupManagerUser();
        _cartServiceMock.Setup(s => s.CancelSaleAsync("cart1", "product1", 1, false))
            .ReturnsAsync((DomainError?)null);

        var result = await _controller.SaleCancel("cart1", "product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Sales"));
        Assert.That(_controller.TempData["Success"], Is.EqualTo("Cancelaste la venta de tu producto con éxito."));
    }

    [Test]
    public async Task SaleCancel_WhenCancelFails_ShouldRedirectWithErrorMessage()
    {
        SetupManagerUser();
        _cartServiceMock.Setup(s => s.CancelSaleAsync("cart1", "product1", 1, false))
            .ReturnsAsync(new CartError("Cancel failed"));

        var result = await _controller.SaleCancel("cart1", "product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Sales"));
        Assert.That(_controller.TempData["Error"], Is.EqualTo("Cancel failed"));
    }

    [Test]
    public async Task Sales_WhenUserIdClaimMissing_ShouldReturnForbid()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Role, UserRoles.MANAGER)
        }, "mock"));

        var httpContext = new DefaultHttpContext() { User = user };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await _controller.Sales();

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }
}
