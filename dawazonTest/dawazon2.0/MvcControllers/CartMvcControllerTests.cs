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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using dawazon2._0.Pdf;

namespace dawazonTest.dawazon2._0.MvcControllers;

[TestFixture]
public class CartMvcControllerTests
{
    private Mock<ICartService> _cartServiceMock;
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<IOrderPdfService> _pdfServiceMock;
    private CartMvcController _controller;

    [SetUp]
    public void Setup()
    {
        _cartServiceMock = new Mock<ICartService>();

        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

        _pdfServiceMock = new Mock<IOrderPdfService>();

        _controller = new CartMvcController(_cartServiceMock.Object, _userManagerMock.Object, _pdfServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, UserRoles.USER)
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

    private void SetupUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, UserRoles.USER)
        }, "mock"));

        var httpContext = new DefaultHttpContext() { User = user };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Test]
    public async Task Index_WhenCartExists_ShouldReturnViewWithModel()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.Index() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<CartOrderDetailViewModel>());
    }

    [Test]
    public async Task Index_WhenCartNotFound_ShouldReturnViewWithEmptyModel()
    {
        SetupUser();
        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.Index() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<CartOrderDetailViewModel>());
        Assert.That(_controller.ViewBag.CartEmpty, Is.True);
    }

    [Test]
    public async Task MyOrders_ShouldReturnViewWithOrders()
    {
        SetupUser();
        var mockCarts = new List<CartResponseDto>
        {
            new CartResponseDto("cart1", 1, true, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0)
        };
        var mockPage = new PageResponseDto<CartResponseDto>(mockCarts, 1, 10, 1, 0, 10, "createAt", "desc");

        _cartServiceMock.Setup(s => s.FindAllAsync(1, true, It.IsAny<FilterCartDto>()))
            .ReturnsAsync(mockPage);

        var result = await _controller.MyOrders() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<CartOrderListViewModel>());
    }

    [Test]
    public async Task Detail_WhenCartExistsAndBelongsToUser_ShouldReturnViewWithModel()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, true, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetByIdAsync("cart1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.Detail("cart1") as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<CartOrderDetailViewModel>());
    }

    [Test]
    public async Task Detail_WhenCartNotFound_ShouldReturnNotFound()
    {
        SetupUser();
        _cartServiceMock.Setup(s => s.GetByIdAsync("cart1"))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.Detail("cart1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Detail_WhenCartBelongsToAnotherUser_ShouldReturnForbid()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 2, true, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetByIdAsync("cart1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.Detail("cart1");

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    [Test]
    public async Task Detail_WhenCartNotPurchased_ShouldReturnNotFound()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetByIdAsync("cart1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.Detail("cart1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task AddToCart_WhenCartExists_ShouldAddProductAndRedirect()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _cartServiceMock.Setup(s => s.AddProductAsync("cart1", "product1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.AddToCart("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(result.RouteValues["id"], Is.EqualTo("product1"));
        Assert.That(_controller.TempData["Success"], Is.EqualTo("Producto añadido al carrito."));
    }

    [Test]
    public async Task AddToCart_WhenCartNotFound_ShouldRedirectWithError()
    {
        SetupUser();
        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.AddToCart("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }

    [Test]
    public async Task AddToCart_WhenAddFails_ShouldRedirectWithError()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _cartServiceMock.Setup(s => s.AddProductAsync("cart1", "product1"))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Product not found")));

        var result = await _controller.AddToCart("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }

    [Test]
    public async Task RemoveFromCart_WhenCartExists_ShouldRemoveProductAndRedirect()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _cartServiceMock.Setup(s => s.RemoveProductAsync("cart1", "product1"))
            .ReturnsAsync(mockCart);

        var result = await _controller.RemoveFromCart("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(result.RouteValues["id"], Is.EqualTo("product1"));
        Assert.That(_controller.TempData["Success"], Is.EqualTo("Producto eliminado del carrito."));
    }

    [Test]
    public async Task RemoveFromCart_WhenCartNotFound_ShouldRedirectWithError()
    {
        SetupUser();
        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.RemoveFromCart("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }

    [Test]
    public async Task Checkout_Get_WhenCartExists_ShouldReturnViewWithModel()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);
        var mockUser = new User { Id = 1, Client = new dawazonBackend.Cart.Models.Client { Name = "Test", Email = "test@test.com", Phone = "123456789", Address = new dawazonBackend.Cart.Models.Address { Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 } } };

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(mockUser);

        var result = await _controller.Checkout() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<CheckoutViewModel>());
    }

    [Test]
    public async Task Checkout_Get_WhenCartNotFound_ShouldRedirectToIndex()
    {
        SetupUser();
        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.Checkout() as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public async Task Checkout_Post_WhenModelInvalid_ShouldReturnViewWithErrors()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        _controller.ModelState.AddModelError("Name", "Required");

        var vm = new CheckoutViewModel { CartId = "cart1", Name = "", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 };
        var result = await _controller.Checkout(vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<CheckoutViewModel>());
    }

    [Test]
    public async Task Checkout_Post_WhenValid_ShouldUpdateUserAndRedirectToStripe()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);
        var mockUser = new User { Id = 1 };

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(mockUser);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        _cartServiceMock.Setup(s => s.CheckoutAsync("cart1"))
            .ReturnsAsync(Result.Success<string, DomainError>("https://stripe.com/checkout"));

        var vm = new CheckoutViewModel { CartId = "cart1", Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 };
        var result = await _controller.Checkout(vm) as RedirectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Url, Is.EqualTo("https://stripe.com/checkout"));
    }

    [Test]
    public async Task Checkout_Post_WhenCheckoutFails_ShouldReturnViewWithError()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);
        var mockUser = new User { Id = 1 };

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(mockUser);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        _cartServiceMock.Setup(s => s.CheckoutAsync("cart1"))
            .ReturnsAsync(Result.Failure<string, DomainError>(new CartNotFoundError("Checkout failed")));

        var vm = new CheckoutViewModel { CartId = "cart1", Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 };
        var result = await _controller.Checkout(vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task Success_WhenCartIdEmpty_ShouldRedirectToIndex()
    {
        var result = await _controller.Success("");

        Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        var redirect = result as RedirectToActionResult;
        Assert.That(redirect.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public async Task Success_WhenCartNotFound_ShouldReturnViewWithError()
    {
        _cartServiceMock.Setup(s => s.GetByIdAsync("cart1"))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.Success("cart1") as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }

    [Test]
    public async Task Success_WhenCartFound_ShouldReturnViewWithOrderId()
    {
        var mockCart = new dawazonBackend.Cart.Models.Cart { Id = "cart1", UserId = 1, Purchased = true };
        var mockCartDto = new CartResponseDto("cart1", 1, true, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetByIdAsync("cart1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCartDto));
        _cartServiceMock.Setup(s => s.GetCartModelByIdAsync("cart1"))
            .ReturnsAsync(mockCart);
        _cartServiceMock.Setup(s => s.SaveAsync(It.IsAny<dawazonBackend.Cart.Models.Cart>()))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCartDto));
        _cartServiceMock.Setup(s => s.SendConfirmationEmailAsync(It.IsAny<dawazonBackend.Cart.Models.Cart>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Success("cart1") as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.TempData["OrderId"], Is.EqualTo("cart1"));
    }

    [Test]
    public void Cancel_ShouldReturnView()
    {
        var result = _controller.Cancel() as ViewResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task IncreaseQty_WhenCartExists_ShouldIncreaseQty()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, 
            new List<SaleLineDto> { new SaleLineDto { ProductId = "product1", Quantity = 1 } }, 1, 10.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _cartServiceMock.Setup(s => s.UpdateStockWithValidationAsync("cart1", "product1", 2))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.IncreaseQty("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public async Task IncreaseQty_WhenCartNotFound_ShouldRedirectWithError()
    {
        SetupUser();
        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.IncreaseQty("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }

    [Test]
    public async Task IncreaseQty_WhenUpdateFails_ShouldRedirectWithError()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, 
            new List<SaleLineDto> { new SaleLineDto { ProductId = "product1", Quantity = 1 } }, 1, 10.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _cartServiceMock.Setup(s => s.UpdateStockWithValidationAsync("cart1", "product1", 2))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Stock error")));

        var result = await _controller.IncreaseQty("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }

    [Test]
    public async Task DecreaseQty_WhenQtyGreaterThan1_ShouldDecreaseQty()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, 
            new List<SaleLineDto> { new SaleLineDto { ProductId = "product1", Quantity = 2 } }, 2, 20.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _cartServiceMock.Setup(s => s.UpdateStockWithValidationAsync("cart1", "product1", 1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.DecreaseQty("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public async Task DecreaseQty_WhenQtyEquals1_ShouldRemoveProduct()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, 
            new List<SaleLineDto> { new SaleLineDto { ProductId = "product1", Quantity = 1 } }, 1, 10.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _cartServiceMock.Setup(s => s.RemoveProductAsync("cart1", "product1"))
            .ReturnsAsync(mockCart);

        var result = await _controller.DecreaseQty("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public async Task DecreaseQty_WhenCartNotFound_ShouldRedirectWithError()
    {
        SetupUser();
        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.DecreaseQty("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }

    [Test]
    public async Task DeleteLine_WhenCartExists_ShouldDeleteLine()
    {
        SetupUser();
        var mockCart = new CartResponseDto("cart1", 1, false, new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123456789", Street = "Calle", Number = 1, City = "Ciudad", Province = "Provincia", Country = "Pais", PostalCode = 12345 }, new List<SaleLineDto>(), 2, 100.0);

        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));
        _cartServiceMock.Setup(s => s.RemoveProductAsync("cart1", "product1"))
            .ReturnsAsync(mockCart);

        var result = await _controller.DeleteLine("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        Assert.That(_controller.TempData["Success"], Is.EqualTo("Producto eliminado del carrito."));
    }

    [Test]
    public async Task DeleteLine_WhenCartNotFound_ShouldRedirectWithError()
    {
        SetupUser();
        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.DeleteLine("product1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }
}
