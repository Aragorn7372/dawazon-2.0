using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using dawazon2._0.RestControllers;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Errors;
using dawazonBackend.Cart.Service;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Products.Errors;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.RestController;

[TestFixture]
public class CartControllerTests
{
    private Mock<ICartService> _cartServiceMock;
    private Mock<ILogger<CartController>> _loggerMock;
    private CartController _controller;

    [SetUp]
    public void SetUp()
    {
        _cartServiceMock = new Mock<ICartService>();
        _loggerMock = new Mock<ILogger<CartController>>();
        _controller = new CartController(_cartServiceMock.Object, _loggerMock.Object);
    }

    private void SetUpUserContext(string userId = "1", string role = UserRoles.USER)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Test]
    public async Task GetSaleLinesAsync_ReturnsOk_WithSaleLines()
    {
        var expectedLines = new List<SaleLineDto>();
        var pageResponse = new PageResponseDto<SaleLineDto>(expectedLines, 0, 0, 10, 0, 0, "id", "asc");
        _cartServiceMock
            .Setup(s => s.FindAllSalesAsLinesAsync(null, true, It.IsAny<FilterDto>()))
            .ReturnsAsync(pageResponse);

        var result = await _controller.GetSaleLinesAsync();

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(pageResponse));
    }

    [Test]
    public async Task GetPurchasedCartsAsync_ReturnsOk_WithCarts()
    {
        SetUpUserContext("123");
        var expectedCarts = new List<CartResponseDto>();
        var pageResponse = new PageResponseDto<CartResponseDto>(expectedCarts, 0, 0, 10, 0, 0, "id", "asc");
        _cartServiceMock
            .Setup(s => s.FindAllAsync(123, true, It.IsAny<FilterCartDto>()))
            .ReturnsAsync(pageResponse);

        var result = await _controller.GetPurchasedCartsAsync();

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(pageResponse));
    }

    [Test]
    public async Task GetNotPurchasedCartsAsync_ReturnsOk_WithCarts()
    {
        SetUpUserContext("123");
        var expectedCarts = new List<CartResponseDto>();
        var pageResponse = new PageResponseDto<CartResponseDto>(expectedCarts, 0, 0, 10, 0, 0, "id", "asc");
        _cartServiceMock
            .Setup(s => s.FindAllAsync(123, false, It.IsAny<FilterCartDto>()))
            .ReturnsAsync(pageResponse);

        var result = await _controller.GetNotPurchasedCartsAsync();

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(pageResponse));
    }

    [Test]
    public async Task AddProcuctAsync_ReturnsOk_WhenSuccess()
    {
        var cartDto = new CartResponseDto("cart1", 123, false, null!, new List<SaleLineDto>(), 0, 0.0);
        _cartServiceMock
            .Setup(s => s.AddProductAsync("cart1", "prod1"))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(cartDto));

        var result = await _controller.AddProcuctAsync("cart1", "prod1");

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(cartDto));
    }

    [Test]
    public async Task AddProcuctAsync_ReturnsNotFound_WhenCartNotFound()
    {
        _cartServiceMock
            .Setup(s => s.AddProductAsync("cart1", "prod1"))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.AddProcuctAsync("cart1", "prod1");

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task AddProcuctAsync_ReturnsConflict_WhenQuantityExceeded()
    {
        _cartServiceMock
            .Setup(s => s.AddProductAsync("cart1", "prod1"))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartProductQuantityExceededError("Exceeded")));

        var result = await _controller.AddProcuctAsync("cart1", "prod1");

        var conflictResult = result as ConflictObjectResult;
        Assert.That(conflictResult, Is.Not.Null);
    }

    [Test]
    public async Task AddProcuctAsync_ReturnsServerError_WhenOtherError()
    {
        _cartServiceMock
            .Setup(s => s.AddProductAsync("cart1", "prod1"))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartError("Generic Server Error")));

        var result = await _controller.AddProcuctAsync("cart1", "prod1");

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task RemoveProcuctAsync_ReturnsOk_WithUpdatedCart()
    {
        var cartDto = new CartResponseDto("cart1", 123, false, null!, new List<SaleLineDto>(), 0, 0.0);
        _cartServiceMock
            .Setup(s => s.RemoveProductAsync("cart1", "prod1"))
            .ReturnsAsync(cartDto);

        var result = await _controller.RemoveProcuctAsync("cart1", "prod1");

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(cartDto));
    }

    [Test]
    public async Task RemoveCartAsync_ReturnsNoContent()
    {
        _cartServiceMock
            .Setup(s => s.DeleteByIdAsync("cart1"))
            .Returns(Task.CompletedTask);

        var result = await _controller.RemoveCartAsync("cart1");

        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        _cartServiceMock.Verify(s => s.DeleteByIdAsync("cart1"), Times.Once);
    }

    [Test]
    public async Task GetCartByUserIdAsync_ReturnsOk_WhenCartFound()
    {
        SetUpUserContext("123");
        var cartDto = new CartResponseDto("cart1", 123, false, null!, new List<SaleLineDto>(), 0, 0.0);
        _cartServiceMock
            .Setup(s => s.GetCartByUserIdAsync(123))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(cartDto));

        var result = await _controller.GetCartByUserIdAsync();

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(cartDto));
    }

    [Test]
    public async Task GetCartByUserIdAsync_ReturnsNotFound_WhenCartNotFound()
    {
        SetUpUserContext("123");
        _cartServiceMock
            .Setup(s => s.GetCartByUserIdAsync(123))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartNotFoundError("Not found")));

        var result = await _controller.GetCartByUserIdAsync();

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task GetCartByUserIdAsync_ReturnsServerError_WhenOtherError()
    {
        SetUpUserContext("123");
        _cartServiceMock
            .Setup(s => s.GetCartByUserIdAsync(123))
            .ReturnsAsync(Result.Failure<CartResponseDto, DomainError>(new CartError("Generic error")));

        var result = await _controller.GetCartByUserIdAsync();

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task CancelSaleLineAsync_ReturnsNoContent_WhenSuccess()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        _cartServiceMock
            .Setup(s => s.CancelSaleAsync("cart1", "prod1", 123, false))
            .ReturnsAsync((DomainError?)null);

        var result = await _controller.CancelSaleLineAsync("cart1", "prod1");

        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
    }

    [Test]
    public async Task CancelSaleLineAsync_ReturnsNotFound_WhenCartNotFound()
    {
        SetUpUserContext("123", UserRoles.ADMIN);
        _cartServiceMock
            .Setup(s => s.CancelSaleAsync("cart1", "prod1", null, true))
            .ReturnsAsync(new CartNotFoundError("Not found"));

        var result = await _controller.CancelSaleLineAsync("cart1", "prod1");

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task CancelSaleLineAsync_ReturnsUnauthorized_WhenUnauthorized()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        _cartServiceMock
            .Setup(s => s.CancelSaleAsync("cart1", "prod1", 123, false))
            .ReturnsAsync(new CartUnauthorizedError("Unauthorized"));

        var result = await _controller.CancelSaleLineAsync("cart1", "prod1");

        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null);
    }

    [Test]
    public async Task CancelSaleLineAsync_ReturnsServerError_WhenOtherError()
    {
        SetUpUserContext();
        _cartServiceMock
            .Setup(s => s.CancelSaleAsync("cart1", "prod1", null, false))
            .ReturnsAsync(new CartError("Server error"));

        var result = await _controller.CancelSaleLineAsync("cart1", "prod1");

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }
}
