using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Errors;
using dawazonBackend.Cart.Models;
using dawazonBackend.Cart.Repository;
using dawazonBackend.Cart.Service;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Common.Mail;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Repository.Productos;
using dawazonBackend.Stripe;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.Cart.Service;

[TestFixture]
[Description("CartService Unit Tests")]
public class CartServiceTest
{
    private Mock<IProductRepository> _productRepositoryMock;
    private Mock<ICartRepository> _cartRepositoryMock;
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<IStripeService> _stripeServiceMock;
    private Mock<IEmailService> _mailServiceMock;
    private Mock<ILogger<CartService>> _loggerMock;

    private CartService _cartService;

    private const string CartId    = "cart1";
    private const string ProductId = "prod1";
    private const long   UserId    = 1L;

    [SetUp]
    public void SetUp()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _cartRepositoryMock    = new Mock<ICartRepository>();

        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _stripeServiceMock = new Mock<IStripeService>();
        _mailServiceMock   = new Mock<IEmailService>();
        _loggerMock        = new Mock<ILogger<CartService>>();

        _cartService = new CartService(
            _productRepositoryMock.Object,
            _cartRepositoryMock.Object,
            _userManagerMock.Object,
            _stripeServiceMock.Object,
            _mailServiceMock.Object,
            _loggerMock.Object
        );
    }

    private static dawazonBackend.Cart.Models.Cart BuildCart(
        string? cartId = CartId,
        long userId = UserId,
        List<CartLine>? lines = null) => new()
    {
        Id        = cartId ?? CartId,
        UserId    = userId,
        CartLines = lines ?? []
    };

    private static User BuildUser(long userId = UserId) => new()
    {
        Id = userId,
        Client = new Client
        {
            Name  = "Test User",
            Email = "test@example.com",
            Phone = "600000000",
            Address = new Address { Street = "Calle Test", Number = 1, City = "Madrid", Province = "Madrid", Country = "España", PostalCode = 28000 }
        }
    };

    private static Product BuildProduct(string? id = ProductId, int stock = 10, long version = 1, long creatorId = 99) => new()
    {
        Id        = id ?? ProductId,
        Stock     = stock,
        Version   = version,
        CreatorId = creatorId,
        Price     = 50.0
    };

    [Test]
    [Description("AddProductAsync: Debería fallar si el producto no existe")]
    public async Task AddProductAsync_WhenProductDoesNotExist_ShouldReturnProductNotFoundError()
    {
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync((Product?)null);

        var result = await _cartService.AddProductAsync(CartId, ProductId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("AddProductAsync: Debería añadir línea al carrito y recalcular si el producto existe")]
    public async Task AddProductAsync_WhenProductExists_ShouldAddLineAndRecalculate()
    {
        var product = BuildProduct();
        var cart    = BuildCart(lines: [new CartLine { CartId = CartId, ProductId = ProductId, Quantity = 1, ProductPrice = 50.0 }]);

        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _cartRepositoryMock.Setup(r => r.AddCartLineAsync(CartId, It.IsAny<CartLine>())).ReturnsAsync(true);
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _cartRepositoryMock.Setup(r => r.UpdateCartAsync(CartId, cart)).ReturnsAsync(cart);

        var result = await _cartService.AddProductAsync(CartId, ProductId);

        Assert.That(result.IsSuccess, Is.True);
        _cartRepositoryMock.Verify(r => r.AddCartLineAsync(CartId, It.Is<CartLine>(cl => cl.ProductId == ProductId && cl.Quantity == 1)), Times.Once);
    }

    [Test]
    [Description("GetByIdAsync: Debería devolver un error si el carrito no existe")]
    public async Task GetByIdAsync_WhenCartDoesNotExist_ShouldReturnCartNotFoundError()
    {
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync((dawazonBackend.Cart.Models.Cart?)null);

        var result = await _cartService.GetByIdAsync(CartId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<CartNotFoundError>());
    }

    [Test]
    [Description("GetByIdAsync: Debería devolver el carrito mapeado a DTO si existe")]
    public async Task GetByIdAsync_WhenCartExists_ShouldReturnCartDto()
    {
        var cart = BuildCart();
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);

        var result = await _cartService.GetByIdAsync(CartId);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(CartId));
    }

    [Test]
    [Description("CheckoutAsync: Carrito no encontrado debe retornar CartNotFoundError")]
    public async Task CheckoutAsync_WhenCartNotFound_ShouldReturnCartNotFoundError()
    {
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync((dawazonBackend.Cart.Models.Cart?)null);

        var result = await _cartService.CheckoutAsync(CartId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<CartNotFoundError>());
    }

    [Test]
    [Description("CheckoutAsync: Usuario no encontrado debe retornar UserNotFoundError")]
    public async Task CheckoutAsync_WhenUserNotFound_ShouldReturnUserNotFoundError()
    {
        var cart = BuildCart();
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _userManagerMock.Setup(um => um.FindByIdAsync(UserId.ToString())).ReturnsAsync((User?)null);

        var result = await _cartService.CheckoutAsync(CartId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserNotFoundError>());
    }

    [Test]
    [Description("CheckoutAsync: Producto del carrito no encontrado debe retornar ProductNotFoundError")]
    public async Task CheckoutAsync_WhenProductNotFound_ShouldReturnProductNotFoundError()
    {
        var cart = BuildCart(lines: [new CartLine { ProductId = ProductId, Quantity = 1 }]);
        var user = BuildUser();

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _userManagerMock.Setup(um => um.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);
        _cartRepositoryMock.Setup(r => r.UpdateCartAsync(CartId, cart)).ReturnsAsync(cart);

        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync((Product?)null);

        var result = await _cartService.CheckoutAsync(CartId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("CheckoutAsync: Stock insuficiente debe retornar CartProductQuantityExceededError")]
    public async Task CheckoutAsync_WhenStockInsufficient_ShouldReturnQuantityExceededError()
    {
        var cart    = BuildCart(lines: [new CartLine { ProductId = ProductId, Quantity = 10 }]);
        var user    = BuildUser();
        var product = BuildProduct(stock: 2); // solo 2 en stock

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _userManagerMock.Setup(um => um.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);
        _cartRepositoryMock.Setup(r => r.UpdateCartAsync(CartId, cart)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var result = await _cartService.CheckoutAsync(CartId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<CartProductQuantityExceededError>());
    }

    [Test]
    [Description("CheckoutAsync: 3 intentos fallidos de concurrencia deben retornar CartAttemptAmountExceededError")]
    public async Task CheckoutAsync_WhenConcurrencyFails3Times_ShouldReturnAttemptExceededError()
    {
        var cart    = BuildCart(lines: [new CartLine { ProductId = ProductId, Quantity = 1 }]);
        var user    = BuildUser();
        var product = BuildProduct(stock: 10);

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _userManagerMock.Setup(um => um.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);
        _cartRepositoryMock.Setup(r => r.UpdateCartAsync(CartId, cart)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.SubstractStockAsync(ProductId, 1, 1)).ReturnsAsync(0);

        var result = await _cartService.CheckoutAsync(CartId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<CartAttemptAmountExceededError>());
    }

    [Test]
    [Description("CheckoutAsync: Stripe falla debe propagar el error de dominio")]
    public async Task CheckoutAsync_WhenStripeFails_ShouldReturnStripeError()
    {
        var cart    = BuildCart(lines: [new CartLine { ProductId = ProductId, Quantity = 1 }]);
        var user    = BuildUser();
        var product = BuildProduct();

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _userManagerMock.Setup(um => um.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);
        _cartRepositoryMock.Setup(r => r.UpdateCartAsync(CartId, cart)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.SubstractStockAsync(ProductId, 1, 1)).ReturnsAsync(1);

        _stripeServiceMock.Setup(s => s.CreateCheckoutSessionAsync(cart))
            .ReturnsAsync(Result.Failure<string, DomainError>(new CartNotFoundError("Stripe error")));

        var result = await _cartService.CheckoutAsync(CartId);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    [Description("CheckoutAsync: Flujo completo válido debe retornar URL de Stripe y reducir stock")]
    public async Task CheckoutAsync_WhenAllValid_ShouldCallStripeAndReturnUrl()
    {
        var cart    = BuildCart(lines: [new CartLine { ProductId = ProductId, Quantity = 2 }]);
        var user    = BuildUser();
        var product = BuildProduct(stock: 10);

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _userManagerMock.Setup(um => um.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);
        _cartRepositoryMock.Setup(r => r.UpdateCartAsync(CartId, cart)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.SubstractStockAsync(ProductId, 2, 1)).ReturnsAsync(1);
        _stripeServiceMock.Setup(s => s.CreateCheckoutSessionAsync(cart))
            .ReturnsAsync(Result.Success<string, DomainError>("https://stripe.com/checkout"));

        var result = await _cartService.CheckoutAsync(CartId);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("https://stripe.com/checkout"));
        Assert.That(cart.CheckoutInProgress, Is.True);
        _productRepositoryMock.Verify(r => r.SubstractStockAsync(ProductId, 2, 1), Times.Once);
    }

    [Test]
    [Description("RestoreStockAsync: Carrito no encontrado debe completar sin hacer nada")]
    public async Task RestoreStockAsync_WhenCartNotFound_ShouldDoNothing()
    {
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync((dawazonBackend.Cart.Models.Cart?)null);

        Assert.DoesNotThrowAsync(() => _cartService.RestoreStockAsync(CartId));

        _productRepositoryMock.Verify(r => r.UpdateProductAsync(It.IsAny<Product>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("RestoreStockAsync: Carrito ya comprado no debe restaurar stock")]
    public async Task RestoreStockAsync_WhenCartIsPurchased_ShouldNotRestoreStock()
    {
        var cart = BuildCart(lines: [new CartLine { ProductId = ProductId, Quantity = 2 }]);
        cart.Purchased = true;

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);

        await _cartService.RestoreStockAsync(CartId);

        _productRepositoryMock.Verify(r => r.UpdateProductAsync(It.IsAny<Product>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("RestoreStockAsync: Carrito no comprado debe sumar stock de cada línea y resetear checkout")]
    public async Task RestoreStockAsync_WhenCartNotPurchased_ShouldRestoreStockAndResetCheckout()
    {
        var product = BuildProduct(stock: 5);
        var cart    = BuildCart(lines: [new CartLine { ProductId = ProductId, Quantity = 3 }]);
        cart.Purchased          = false;
        cart.CheckoutInProgress = true;

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId)).ReturnsAsync(product);
        _cartRepositoryMock.Setup(r => r.UpdateCartAsync(CartId, cart)).ReturnsAsync(cart);

        await _cartService.RestoreStockAsync(CartId);

        Assert.That(product.Stock, Is.EqualTo(8)); // 5 + 3
        Assert.That(cart.CheckoutInProgress, Is.False);
        Assert.That(cart.CheckoutStartedAt, Is.Null);
        _cartRepositoryMock.Verify(r => r.UpdateCartAsync(CartId, cart), Times.Once);
    }

    [Test]
    [Description("RestoreStockAsync: Si el producto de la línea no existe no debe lanzar excepción")]
    public async Task RestoreStockAsync_WhenProductNotFound_ShouldSkipAndNotThrow()
    {
        var cart = BuildCart(lines: [new CartLine { ProductId = ProductId, Quantity = 1 }]);
        cart.Purchased = false;

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync((Product?)null);
        _cartRepositoryMock.Setup(r => r.UpdateCartAsync(CartId, cart)).ReturnsAsync(cart);

        Assert.DoesNotThrowAsync(() => _cartService.RestoreStockAsync(CartId));
    }

    [Test]
    [Description("DeleteByIdAsync: Debe delegar la eliminación al repositorio")]
    public async Task DeleteByIdAsync_ShouldCallRepositoryDeleteCart()
    {
        _cartRepositoryMock.Setup(r => r.DeleteCartAsync(CartId)).Returns(Task.CompletedTask);

        await _cartService.DeleteByIdAsync(CartId);

        _cartRepositoryMock.Verify(r => r.DeleteCartAsync(CartId), Times.Once);
    }

    [Test]
    [Description("GetCartByUserIdAsync: Carrito existente debe retornar el DTO")]
    public async Task GetCartByUserIdAsync_WhenCartExists_ShouldReturnCartDto()
    {
        var cart = BuildCart();
        _cartRepositoryMock.Setup(r => r.FindByUserIdAndPurchasedAsync(UserId, false)).ReturnsAsync(cart);

        var result = await _cartService.GetCartByUserIdAsync(UserId);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(CartId));
    }

    [Test]
    [Description("GetCartByUserIdAsync: Sin carrito activo debe retornar CartNotFoundError")]
    public async Task GetCartByUserIdAsync_WhenNoCart_ShouldReturnCartNotFoundError()
    {
        _cartRepositoryMock.Setup(r => r.FindByUserIdAndPurchasedAsync(UserId, false)).ReturnsAsync((dawazonBackend.Cart.Models.Cart?)null);

        var result = await _cartService.GetCartByUserIdAsync(UserId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<CartNotFoundError>());
    }

    [Test]
    [Description("CancelSaleAsync: Carrito no encontrado debe retornar CartNotFoundError")]
    public async Task CancelSaleAsync_WhenCartNotFound_ShouldReturnCartNotFoundError()
    {
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync((dawazonBackend.Cart.Models.Cart?)null);

        var error = await _cartService.CancelSaleAsync(CartId, ProductId, null, true);

        Assert.That(error, Is.InstanceOf<CartNotFoundError>());
    }

    [Test]
    [Description("CancelSaleAsync: Línea de producto no encontrada debe retornar CartNotFoundError")]
    public async Task CancelSaleAsync_WhenLineNotFound_ShouldReturnCartNotFoundError()
    {
        var cart = BuildCart(lines: []); // sin líneas
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);

        var error = await _cartService.CancelSaleAsync(CartId, ProductId, null, true);

        Assert.That(error, Is.InstanceOf<CartNotFoundError>());
    }

    [Test]
    [Description("CancelSaleAsync: Producto no encontrado en BD debe retornar ProductNotFoundError")]
    public async Task CancelSaleAsync_WhenProductNotFound_ShouldReturnProductNotFoundError()
    {
        var line = new CartLine { ProductId = ProductId, Quantity = 1, Status = Status.Preparado };
        var cart = BuildCart(lines: [line]);
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync((Product?)null);

        var error = await _cartService.CancelSaleAsync(CartId, ProductId, null, true);

        Assert.That(error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("CancelSaleAsync: Manager sin permiso (no admin, producto de otro) debe retornar CartUnauthorizedError")]
    public async Task CancelSaleAsync_WhenManagerNotOwner_ShouldReturnUnauthorizedError()
    {
        var line    = new CartLine { ProductId = ProductId, Quantity = 1, Status = Status.Preparado };
        var cart    = BuildCart(lines: [line]);
        var product = BuildProduct(creatorId: 999); // creador distinto

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var error = await _cartService.CancelSaleAsync(CartId, ProductId, managerId: 42, isAdmin: false);

        Assert.That(error, Is.InstanceOf<CartUnauthorizedError>());
    }

    [Test]
    [Description("CancelSaleAsync: Admin cancela línea no cancelada → actualiza estado, restaura stock y retorna null")]
    public async Task CancelSaleAsync_WhenAdminAndLineNotCancelled_ShouldCancelAndRestoreStock()
    {
        var line    = new CartLine { ProductId = ProductId, Quantity = 2, Status = Status.Preparado };
        var cart    = BuildCart(lines: [line]);
        var product = BuildProduct(stock: 5);

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _cartRepositoryMock.Setup(r => r.UpdateCartLineStatusAsync(CartId, ProductId, Status.Cancelado)).ReturnsAsync(true);
        _productRepositoryMock.Setup(r => r.UpdateProductAsync(product, ProductId!)).ReturnsAsync(product);

        var error = await _cartService.CancelSaleAsync(CartId, ProductId, null, isAdmin: true);

        Assert.That(error, Is.Null);
        Assert.That(product.Stock, Is.EqualTo(7)); // 5 + 2
        _cartRepositoryMock.Verify(r => r.UpdateCartLineStatusAsync(CartId, ProductId, Status.Cancelado), Times.Once);
    }

    [Test]
    [Description("CancelSaleAsync: Línea ya cancelada no debe volver a cancelar ni cambiar stock")]
    public async Task CancelSaleAsync_WhenLineAlreadyCancelled_ShouldDoNothingAndReturnNull()
    {
        var line    = new CartLine { ProductId = ProductId, Quantity = 2, Status = Status.Cancelado };
        var cart    = BuildCart(lines: [line]);
        var product = BuildProduct(stock: 5);

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var error = await _cartService.CancelSaleAsync(CartId, ProductId, null, isAdmin: true);

        Assert.That(error, Is.Null);
        Assert.That(product.Stock, Is.EqualTo(5)); // sin cambios
        _cartRepositoryMock.Verify(r => r.UpdateCartLineStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Status>()), Times.Never);
    }

    [Test]
    [Description("UpdateSaleStatusAsync: Status.Cancelado debe delegar a CancelSaleAsync")]
    public async Task UpdateSaleStatusAsync_WhenStatusCancelled_ShouldDelegateToCancelSale()
    {
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync((dawazonBackend.Cart.Models.Cart?)null);

        var error = await _cartService.UpdateSaleStatusAsync(CartId, ProductId, Status.Cancelado, null, true);

        Assert.That(error, Is.InstanceOf<CartNotFoundError>());
    }

    [Test]
    [Description("UpdateSaleStatusAsync: Carrito no encontrado debe retornar CartNotFoundError")]
    public async Task UpdateSaleStatusAsync_WhenCartNotFound_ShouldReturnCartNotFoundError()
    {
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync((dawazonBackend.Cart.Models.Cart?)null);

        var error = await _cartService.UpdateSaleStatusAsync(CartId, ProductId, Status.Enviado, null, true);

        Assert.That(error, Is.InstanceOf<CartNotFoundError>());
    }

    [Test]
    [Description("UpdateSaleStatusAsync: Manager sin permiso debe retornar CartUnauthorizedError")]
    public async Task UpdateSaleStatusAsync_WhenManagerNotOwner_ShouldReturnUnauthorizedError()
    {
        var line    = new CartLine { ProductId = ProductId, Quantity = 1, Status = Status.Preparado };
        var cart    = BuildCart(lines: [line]);
        var product = BuildProduct(creatorId: 999);

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var error = await _cartService.UpdateSaleStatusAsync(CartId, ProductId, Status.Enviado, managerId: 42, isAdmin: false);

        Assert.That(error, Is.InstanceOf<CartUnauthorizedError>());
    }

    [Test]
    [Description("UpdateSaleStatusAsync: Pasar de Cancelado a Preparado con stock suficiente debe restar stock y actualizar estado")]
    public async Task UpdateSaleStatusAsync_WhenReactivatingCancelledLine_ShouldDeductStockAndUpdate()
    {
        var line    = new CartLine { ProductId = ProductId, Quantity = 2, Status = Status.Cancelado };
        var cart    = BuildCart(lines: [line]);
        var product = BuildProduct(stock: 10, creatorId: 42);

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.UpdateProductAsync(product, ProductId!)).ReturnsAsync(product);
        _cartRepositoryMock.Setup(r => r.UpdateCartLineStatusAsync(CartId, ProductId, Status.Preparado)).ReturnsAsync(true);

        var error = await _cartService.UpdateSaleStatusAsync(CartId, ProductId, Status.Preparado, managerId: 42, isAdmin: false);

        Assert.That(error, Is.Null);
        Assert.That(product.Stock, Is.EqualTo(8)); // 10 - 2
        _cartRepositoryMock.Verify(r => r.UpdateCartLineStatusAsync(CartId, ProductId, Status.Preparado), Times.Once);
    }

    [Test]
    [Description("UpdateSaleStatusAsync: Reactivar línea cancelada sin stock debe retornar CartProductQuantityExceededError")]
    public async Task UpdateSaleStatusAsync_WhenReactivatingButInsufficientStock_ShouldReturnQuantityExceededError()
    {
        var line    = new CartLine { ProductId = ProductId, Quantity = 10, Status = Status.Cancelado };
        var cart    = BuildCart(lines: [line]);
        var product = BuildProduct(stock: 2, creatorId: 42); // solo 2 en stock

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var error = await _cartService.UpdateSaleStatusAsync(CartId, ProductId, Status.Preparado, managerId: 42, isAdmin: false);

        Assert.That(error, Is.InstanceOf<CartProductQuantityExceededError>());
    }

    [Test]
    [Description("UpdateSaleStatusAsync: Estado Enviado en línea activa sin ser cancelada solo actualiza estado")]
    public async Task UpdateSaleStatusAsync_WhenNormalTransition_ShouldOnlyUpdateStatus()
    {
        var line    = new CartLine { ProductId = ProductId, Quantity = 1, Status = Status.Preparado };
        var cart    = BuildCart(lines: [line]);
        var product = BuildProduct(stock: 5, creatorId: 42);

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _cartRepositoryMock.Setup(r => r.UpdateCartLineStatusAsync(CartId, ProductId, Status.Enviado)).ReturnsAsync(true);

        var error = await _cartService.UpdateSaleStatusAsync(CartId, ProductId, Status.Enviado, managerId: 42, isAdmin: false);

        Assert.That(error, Is.Null);
        Assert.That(product.Stock, Is.EqualTo(5));
        _cartRepositoryMock.Verify(r => r.UpdateCartLineStatusAsync(CartId, ProductId, Status.Enviado), Times.Once);
        _productRepositoryMock.Verify(r => r.UpdateProductAsync(It.IsAny<Product>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("UpdateStockWithValidationAsync: Si la cantidad es menor a 1, debe retornar CartMinQuantityError")]
    public async Task UpdateStockWithValidationAsync_WhenQuantityLessThanOne_ShouldReturnMinQuantityError()
    {
        var result = await _cartService.UpdateStockWithValidationAsync(CartId, ProductId, 0);

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(BuildCart());
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(BuildProduct());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<CartMinQuantityError>());
    }

    [Test]
    [Description("UpdateStockWithValidationAsync: Si el stock es insuficiente, debe retornar CartProductQuantityExceededError")]
    public async Task UpdateStockWithValidationAsync_WhenStockIsInsufficient_ShouldReturnQuantityExceededError()
    {
        var cart    = BuildCart(lines: []);
        var product = BuildProduct(stock: 5);

        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);        
        
        var result = await _cartService.UpdateStockWithValidationAsync(CartId, ProductId, 10);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<CartProductQuantityExceededError>());
    }

    [Test]
    [Description("FindAllSalesAsLinesAsync: Debe retornar página de ventas con filtros y roles")]
    public async Task FindAllSalesAsLinesAsync_ShouldReturnPagedSales()
    {
        var filter = new FilterDto(null, null, 0, 10, "Date", "asc");
        var lines = new List<SaleLineDto> { 
            new SaleLineDto {
                SaleId = CartId, 
                ProductId = ProductId, 
                Quantity = 1, 
                ProductPrice = 10.0, 
                TotalPrice = 10.0, 
                Status = Status.Preparado, 
                ProductName = "Prod1", 
                CreateAt = DateTime.Now, 
                UpdateAt = DateTime.Now
            } 
        };
        _cartRepositoryMock.Setup(r => r.GetSalesAsLinesAsync(1L, true, filter)).ReturnsAsync((lines, 1));

        var result = await _cartService.FindAllSalesAsLinesAsync(1L, true, filter);

        Assert.That(result.TotalElements, Is.EqualTo(1));
        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(result.TotalPages, Is.EqualTo(1));
    }

    [Test]
    [Description("CalculateTotalEarningsAsync: Debe retornar total calculado por repositorio")]
    public async Task CalculateTotalEarningsAsync_ShouldReturnCalculatedEarnings()
    {
        _cartRepositoryMock.Setup(r => r.CalculateTotalEarningsAsync(1L, true)).ReturnsAsync(150.5);

        var result = await _cartService.CalculateTotalEarningsAsync(1L, true);

        Assert.That(result, Is.EqualTo(150.5));
    }

    [Test]
    [Description("FindAllAsync: Debe filtrar carritos por UserId si se provee, y mapear a DTO")]
    public async Task FindAllAsync_WithUserId_ShouldReturnFilteredCarts()
    {
        var filter = new FilterCartDto(null, null, null, 0, 10, "id", "asc");
        var cart1 = BuildCart("c1", 1L); cart1.Purchased = true;
        var cart2 = BuildCart("c2", 2L); cart2.Purchased = true;
        var carts = new List<dawazonBackend.Cart.Models.Cart> { cart1, cart2 };

        _cartRepositoryMock.Setup(r => r.GetAllAsync(filter)).ReturnsAsync((carts, 2));

        var result = await _cartService.FindAllAsync(1L, true, filter);

        Assert.That(result.Content.Count, Is.EqualTo(1));
        Assert.That(result.Content.First().UserId, Is.EqualTo(1L));
        Assert.That(result.TotalPages, Is.EqualTo(1));
    }

    [Test]
    [Description("FindAllAsync: Debe retornar todos los comprados si UserId es null")]
    public async Task FindAllAsync_WithoutUserId_ShouldReturnAllPurchased()
    {
        var filter = new FilterCartDto(null, null, null, 0, 10, "id", "asc");
        var cart1 = BuildCart("c1", 1L); cart1.Purchased = true;
        var cart2 = BuildCart("c2", 2L); cart2.Purchased = true;
        var carts = new List<dawazonBackend.Cart.Models.Cart> { cart1, cart2 };

        _cartRepositoryMock.Setup(r => r.GetAllAsync(filter)).ReturnsAsync((carts, 2));

        var result = await _cartService.FindAllAsync(null, true, filter);

        Assert.That(result.Content.Count, Is.EqualTo(2));
        Assert.That(result.TotalPages, Is.EqualTo(1));
    }

    [Test]
    [Description("RemoveProductAsync: Debe eliminar línea y recalcular totales")]
    public async Task RemoveProductAsync_ShouldRemoveLineAndRecalculate()
    {
        var cart = BuildCart(CartId, 1L);
        _cartRepositoryMock.Setup(r => r.RemoveCartLineAsync(CartId, It.IsAny<CartLine>())).ReturnsAsync(true);
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);

        var result = await _cartService.RemoveProductAsync(CartId, ProductId);

        _cartRepositoryMock.Verify(r => r.RemoveCartLineAsync(CartId, It.Is<CartLine>(c => c.ProductId == ProductId)), Times.Once);
        Assert.That(result.Id, Is.EqualTo(CartId));
    }

    [Test]
    [Description("GetCartModelByIdAsync: Debe retornar el modelo original desde el repositorio")]
    public async Task GetCartModelByIdAsync_ShouldReturnCartModel()
    {
        var cart = BuildCart();
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync(CartId)).ReturnsAsync(cart);

        var result = await _cartService.GetCartModelByIdAsync(CartId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(CartId));
    }

    [Test]
    [Description("SaveAsync: Debe actualizar carrito a comprado, crear uno nuevo y retornarlo")]
    public async Task SaveAsync_ShouldMarkAsPurchasedAndCreateNewCart()
    {
        var oldCart = BuildCart(lines: [new CartLine { Status = Status.EnCarrito }]);
        var user = BuildUser();
        var newCart = BuildCart("c2", UserId);

        _cartRepositoryMock.Setup(r => r.UpdateCartAsync(CartId, oldCart)).ReturnsAsync(oldCart);
        _userManagerMock.Setup(um => um.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);
        _cartRepositoryMock.Setup(r => r.CreateCartAsync(It.IsAny<dawazonBackend.Cart.Models.Cart>())).ReturnsAsync(newCart);

        var result = await _cartService.SaveAsync(oldCart);

        Assert.That(oldCart.Purchased, Is.True);
        Assert.That(oldCart.CheckoutInProgress, Is.False);
        Assert.That(oldCart.CartLines.All(l => l.Status == Status.Preparado), Is.True);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo("c2"));
    }

    [Test]
    [Description("SendConfirmationEmailAsync: Debe encolar el email si todo es correcto")]
    public async Task SendConfirmationEmailAsync_ShouldEnqueueEmail()
    {
        var cart = BuildCart();
        cart.Client = new Client { Email = "test@example.com" };

        _mailServiceMock.Setup(m => m.EnqueueEmailAsync(It.IsAny<EmailMessage>())).Returns(Task.CompletedTask);

        await _cartService.SendConfirmationEmailAsync(cart);

        _mailServiceMock.Verify(m => m.EnqueueEmailAsync(It.Is<EmailMessage>(e => e.To == "test@example.com" && e.Subject.Contains(CartId))), Times.Once);
    }

    [Test]
    [Description("GetNewSalesCountAsync: Debe retornar el conteo desde el repositorio")]
    public async Task GetNewSalesCountAsync_ShouldReturnCountFromRepository()
    {
        var since = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _cartRepositoryMock.Setup(r => r.CountNewSalesAsync(999, since)).ReturnsAsync(5);

        var result = await _cartService.GetNewSalesCountAsync(999, since);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(5));
    }

    [Test]
    [Description("GetNewSalesCountAsync: Si el repositorio lanza excepcion debe retornar error")]
    public async Task GetNewSalesCountAsync_WhenRepositoryThrows_ShouldReturnError()
    {
        var since = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _cartRepositoryMock.Setup(r => r.CountNewSalesAsync(It.IsAny<long>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _cartService.GetNewSalesCountAsync(999, since);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    [Description("GetTotalSalesCountAsync: Debe retornar el total desde el repositorio")]
    public async Task GetTotalSalesCountAsync_ShouldReturnTotalFromRepository()
    {
        _cartRepositoryMock.Setup(r => r.GetTotalSalesCountAsync()).ReturnsAsync(42);

        var result = await _cartService.GetTotalSalesCountAsync();

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    [Description("CleanupExpiredCheckoutsAsync: Si no hay carritos en BD, debe terminar sin hacer nada")]
    public async Task CleanupExpiredCheckoutsAsync_WhenNoCarts_ShouldDoNothing()
    {
        _cartRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<FilterCartDto>()))
            .ReturnsAsync((new List<dawazonBackend.Cart.Models.Cart>(), 0));

        await _cartService.CleanupExpiredCheckoutsAsync(5);

        _cartRepositoryMock.Verify(r => r.FindCartByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("CleanupExpiredCheckoutsAsync: Si los carritos no están expirados o no tienen checkout activo, no debe restaurar stock")]
    public async Task CleanupExpiredCheckoutsAsync_WhenNoExpiredCarts_ShouldDoNothing()
    {
        var carts = new List<dawazonBackend.Cart.Models.Cart>
        {
            BuildCart("c1"), 
            new dawazonBackend.Cart.Models.Cart { 
                Id = "c2", 
                CheckoutInProgress = true, 
                CheckoutStartedAt = DateTime.UtcNow.AddMinutes(-2)  
            }
        };

        _cartRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<FilterCartDto>())).ReturnsAsync((carts, 2));

        await _cartService.CleanupExpiredCheckoutsAsync(5);

        _cartRepositoryMock.Verify(r => r.FindCartByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("CleanupExpiredCheckoutsAsync: Si hay carritos expirados, debe restaurar stock y resetear estado de checkout")]
    public async Task CleanupExpiredCheckoutsAsync_WhenExpiredCarts_ShouldRestoreStock()
    {
        var expiredCart = new dawazonBackend.Cart.Models.Cart 
        { 
            Id = "c_expired", 
            CheckoutInProgress = true, 
            CheckoutStartedAt = DateTime.UtcNow.AddMinutes(-10),
            Purchased = false,
            CartLines = [new CartLine { ProductId = ProductId, Quantity = 2 }]
        };
        var activeCart = new dawazonBackend.Cart.Models.Cart 
        { 
            Id = "c_active", 
            CheckoutInProgress = true, 
            CheckoutStartedAt = DateTime.UtcNow.AddMinutes(-2), 
            Purchased = false
        };

        var carts = new List<dawazonBackend.Cart.Models.Cart> { expiredCart, activeCart };
        var product = BuildProduct(stock: 10);

        _cartRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<FilterCartDto>())).ReturnsAsync((carts, 2));
        
        _cartRepositoryMock.Setup(r => r.FindCartByIdAsync("c_expired")).ReturnsAsync(expiredCart);
        _productRepositoryMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId)).ReturnsAsync(product);
        _cartRepositoryMock.Setup(r => r.UpdateCartAsync("c_expired", expiredCart)).ReturnsAsync(expiredCart);

        await _cartService.CleanupExpiredCheckoutsAsync(5);

        Assert.That(product.Stock, Is.EqualTo(12)); 
        Assert.That(expiredCart.CheckoutInProgress, Is.False);
        Assert.That(expiredCart.CheckoutStartedAt, Is.Null);

        _cartRepositoryMock.Verify(r => r.UpdateCartAsync("c_expired", expiredCart), Times.Once);
        _cartRepositoryMock.Verify(r => r.FindCartByIdAsync("c_active"), Times.Never);
    }
}