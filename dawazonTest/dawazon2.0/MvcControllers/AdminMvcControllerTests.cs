using CSharpFunctionalExtensions;
using dawazon2._0.Models;
using dawazon2._0.MvcControllers;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Errors;
using dawazonBackend.Cart.Service;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using dawazonBackend.Cart.Models;

namespace dawazonTest.dawazon2._0.MvcControllers;

[TestFixture]
public class AdminMvcControllerTests
{
    private Mock<IUserService> _userServiceMock;
    private Mock<ICartService> _cartServiceMock;
    private Mock<UserManager<User>> _userManagerMock;
    private AdminMvcController _controller;

    [SetUp]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();
        _cartServiceMock = new Mock<ICartService>();
        
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

        _controller = new AdminMvcController(_userServiceMock.Object, _cartServiceMock.Object, _userManagerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, UserRoles.ADMIN)
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

    [Test]
    public async Task Users_ShouldReturnViewWithModel()
    {
        var mockPage = new PageResponseDto<UserDto>(new List<UserDto>(), 1, 10, 10, 0, 10, "id", "asc");
        _userServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>()))
            .ReturnsAsync(mockPage);

        var result = await _controller.Users() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<AdminUserListViewModel>());
        var model = (AdminUserListViewModel)result.Model;
        Assert.That(model.TotalElements, Is.EqualTo(10));
    }

    [Test]
    public async Task UserDetail_WhenExists_ShouldReturnViewWithModel()
    {
        var mockDto = new UserDto { Id = 1, Nombre = "John" };
        _userServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Success<UserDto, UserError>(mockDto));

        var result = await _controller.UserDetail("1") as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<AdminUserDetailViewModel>());
    }

    [Test]
    public async Task UserDetail_WhenNotExists_ShouldReturnNotFound()
    {
        _userServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Failure<UserDto, UserError>(new UserNotFoundError("Not found")));

        var result = await _controller.UserDetail("1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task UserEdit_Get_WhenExists_ShouldReturnViewWithModel()
    {
        var mockDto = new UserDto { Id = 1, Nombre = "John", Email = "j@e.com", Roles = new HashSet<string> { UserRoles.USER } };
        _userServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Success<UserDto, UserError>(mockDto));
        _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("1");

        var result = await _controller.UserEdit("1") as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<UserEditViewModel>());
        var model = (UserEditViewModel)result.Model;
        Assert.That(model.Nombre, Is.EqualTo("John"));
        Assert.That(model.Rol, Is.EqualTo(UserRoles.USER));
    }

    [Test]
    public async Task UserEdit_Post_WhenUpdateSucceeds_ShouldRedirect()
    {
        var vm = new UserEditViewModel { Nombre = "John", Rol = UserRoles.MANAGER };
        _userManagerMock.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("1");
        
        var mockDto = new UserDto { Id = 2 };
        _userServiceMock.Setup(s => s.UpdateByIdAsync(2, It.IsAny<UserRequestDto>(), null))
            .ReturnsAsync(Result.Success<UserDto, UserError>(mockDto));

        var mockUser = new User { Id = 2 };
        _userManagerMock.Setup(m => m.FindByIdAsync("2")).ReturnsAsync(mockUser);
        _userManagerMock.Setup(m => m.GetRolesAsync(mockUser)).ReturnsAsync(new List<string> { UserRoles.USER });
        _userManagerMock.Setup(m => m.RemoveFromRolesAsync(mockUser, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(mockUser, UserRoles.MANAGER)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.UserEdit("2", vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Users"));
    }

    [Test]
    public async Task UserDelete_ShouldBanUserAndRedirect()
    {
        _userServiceMock.Setup(s => s.BanUserById("2")).Returns(Task.CompletedTask);

        var result = await _controller.UserDelete("2") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Users"));
    }

    [Test]
    public async Task Sales_ShouldReturnViewWithModel()
    {
        var mockPage = new PageResponseDto<SaleLineDto>(new List<SaleLineDto>(), 1, 5, 10, 0, 5, "createAt", "desc");
        _cartServiceMock.Setup(s => s.FindAllSalesAsLinesAsync(null, true, It.IsAny<FilterDto>()))
            .ReturnsAsync(mockPage);
        _cartServiceMock.Setup(s => s.CalculateTotalEarningsAsync(null, true)).ReturnsAsync(100.0);

        var result = await _controller.Sales() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<AdminSaleListViewModel>());
        var model = (AdminSaleListViewModel)result.Model;
        Assert.That(model.TotalEarnings, Is.EqualTo(100.0));
    }

    [Test]
    public async Task SaleEdit_Get_WhenCartNotFound_ShouldReturnNotFound()
    {
        _cartServiceMock.Setup(s => s.GetByIdAsync("CART-1"))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Cart not found")));

        var result = await _controller.SaleEdit("CART-1", "PROD-1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task SaleEdit_Get_WhenLineNotFound_ShouldReturnNotFound()
    {
        var client = new ClientDto { Name = "John" };
        var cart = new CartResponseDto("CART-1", 1, true, client, new List<SaleLineDto>(), 0, 0);
        
        _cartServiceMock.Setup(s => s.GetByIdAsync("CART-1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(cart));

        var result = await _controller.SaleEdit("CART-1", "PROD-1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task SaleEdit_Get_WhenSuccess_ShouldReturnView()
    {
        var client = new ClientDto { Name = "John" };
        var line = new SaleLineDto { ProductId = "PROD-1", ProductName = "Product 1", Status = Status.EnCarrito };
        var cart = new CartResponseDto("CART-1", 1, true, client, new List<SaleLineDto> { line }, 1, 10.0);
        
        _cartServiceMock.Setup(s => s.GetByIdAsync("CART-1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(cart));

        var result = await _controller.SaleEdit("CART-1", "PROD-1") as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<AdminSaleEditViewModel>());
    }

    [Test]
    public async Task SaleEdit_Post_WhenSuccess_ShouldRedirect()
    {
        var vm = new AdminSaleEditViewModel { NewStatus = Status.Enviado };
        _cartServiceMock.Setup(s => s.UpdateSaleStatusAsync("CART-1", "PROD-1", Status.Enviado, null, true))
            .ReturnsAsync((dawazonBackend.Common.Error.DomainError)null);

        var result = await _controller.SaleEdit("CART-1", "PROD-1", vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Sales"));
    }

    [Test]
    public async Task SaleCancel_WhenSuccess_ShouldRedirect()
    {
        _cartServiceMock.Setup(s => s.CancelSaleAsync("CART-1", "PROD-1", null, true))
            .ReturnsAsync((dawazonBackend.Common.Error.DomainError)null);

        var result = await _controller.SaleCancel("CART-1", "PROD-1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Sales"));
    }

    [Test]
    public void Stats_ShouldReturnView()
    {
        var result = _controller.Stats() as ViewResult;

        Assert.That(result, Is.Not.Null);
    }
}