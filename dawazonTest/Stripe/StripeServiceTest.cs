using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Stripe;
using Stripe.Checkout;
using dawazonBackend.Stripe;
using dawazonBackend.Cart.Models;
using dawazonBackend.Stripe.Errors;
using System.Collections.Generic;
using System;

namespace dawazonTest.Stripe;

[TestFixture]
[Description("Tests for StripeService")]
public class StripeServiceTest
{
    private Mock<IConfiguration> _configurationMock;
    private Mock<ILogger<StripeService>> _loggerMock;
    private Mock<IStripeClient> _stripeClientMock;
    private StripeService _stripeService;

    [SetUp]
    public void SetUp()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Stripe:Key"]).Returns("sk_test_123");
        _configurationMock.Setup(c => c["Server:Url"]).Returns("https://localhost:5001");

        _loggerMock = new Mock<ILogger<StripeService>>();
        
        _stripeClientMock = new Mock<IStripeClient>();

        _stripeService = new StripeService(_configurationMock.Object, _loggerMock.Object);
        
        StripeConfiguration.StripeClient = _stripeClientMock.Object;
    }

    private dawazonBackend.Cart.Models.Cart GetTestCart()
    {
        return new dawazonBackend.Cart.Models.Cart
        {
            Id = "cart-123",
            Client = new Client { Email = "test@example.com" },
            CartLines = new List<CartLine>
            {
                new CartLine { ProductId = "prod-1", Quantity = 2, ProductPrice = 10.50 },
                new CartLine { ProductId = "prod-2", Quantity = 1, ProductPrice = 5.00 }
            }
        };
    }

    [Test]
    public async Task CreateCheckoutSessionAsync_WithValidCart_ReturnsSuccessWithSessionUrl()
    {
        var cart = GetTestCart();
        var expectedUrl = "https://checkout.stripe.com/pay/cs_test_123";

        _stripeClientMock.Setup(c => c.RequestAsync<Session>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<BaseOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Session { Url = expectedUrl });

        var result = await _stripeService.CreateCheckoutSessionAsync(cart);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedUrl));
    }

    [Test]
    public async Task CreateCheckoutSessionAsync_WithNoServerUrl_UsesDefaultUrl()
    {
        var cart = GetTestCart();
        var expectedUrl = "https://checkout.stripe.com/pay/cs_test_default";

        _configurationMock.Setup(c => c["Server:Url"]).Returns((string?)null);
        _stripeService = new StripeService(_configurationMock.Object, _loggerMock.Object);
        StripeConfiguration.StripeClient = _stripeClientMock.Object;

        _stripeClientMock.Setup(c => c.RequestAsync<Session>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<BaseOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Session { Url = expectedUrl });

        var result = await _stripeService.CreateCheckoutSessionAsync(cart);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedUrl));
    }

    [Test]
    public async Task CreateCheckoutSessionAsync_WhenStripeExceptionIsThrown_ReturnsFailureAndLogsWarning()
    {
        var cart = GetTestCart();
        
        var stripeException = new StripeException("Card declined")
        {
            StripeError = new global::Stripe.StripeError { Message = "Your card was declined." }
        };

        _stripeClientMock.Setup(c => c.RequestAsync<Session>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<BaseOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(stripeException);

        var result = await _stripeService.CreateCheckoutSessionAsync(cart);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<StripePaymentError>());
        Assert.That(result.Error.Message, Contains.Substring("Your card was declined"));
    }

    [Test]
    public async Task CreateCheckoutSessionAsync_WhenStripeExceptionHasNoStripeError_ReturnsFailureWithMessage()
    {
        var cart = GetTestCart();
        
        var stripeException = new StripeException("Network failure");

        _stripeClientMock.Setup(c => c.RequestAsync<Session>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<BaseOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(stripeException);

        var result = await _stripeService.CreateCheckoutSessionAsync(cart);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<StripePaymentError>());
        Assert.That(result.Error.Message, Contains.Substring("Network failure"));
    }

    [Test]
    public async Task CreateCheckoutSessionAsync_WhenGenericExceptionIsThrown_ReturnsFailureAndLogsError()
    {
        var cart = GetTestCart();
        
        _stripeClientMock.Setup(c => c.RequestAsync<Session>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<BaseOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database timeout scenario"));

        var result = await _stripeService.CreateCheckoutSessionAsync(cart);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<StripePaymentError>());
        Assert.That(result.Error.Message, Contains.Substring("Ocurrió un error inesperado"));
    }
}
