using dawazonBackend.Cart.Models;
using NUnit.Framework;

namespace dawazonTest.Cart.Models;

[TestFixture]
[Description("Cart Models Unit Tests")]
public class CartModelsTest
{
    [Test]
    [Description("Cart new() debe inicializar CartLines vacío, CheckoutInProgress en false, CheckoutStartedAt en null y timestamps en UTC de hoy")]
    public void Cart_New_ShouldHaveSafeDefaults()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var cart   = new dawazonBackend.Cart.Models.Cart();
        var after  = DateTime.UtcNow.AddSeconds(1);

        Assert.That(cart.CartLines,          Is.Not.Null);
        Assert.That(cart.CartLines,          Is.Empty);
        Assert.That(cart.CheckoutInProgress, Is.False);
        Assert.That(cart.CheckoutStartedAt,  Is.Null);
        Assert.That(cart.CreatedAt, Is.GreaterThan(before).And.LessThan(after));
        Assert.That(cart.UploadAt,  Is.GreaterThan(before).And.LessThan(after));
    }

    [Test]
    [Description("GetMinutesSinceCheckoutStarted: sin CheckoutStartedAt debe retornar 0")]
    public void GetMinutesSinceCheckoutStarted_WhenNull_ShouldReturnZero()
    {
        var cart = new dawazonBackend.Cart.Models.Cart { CheckoutStartedAt = null };
        Assert.That(cart.GetMinutesSinceCheckoutStarted(), Is.EqualTo(0));
    }

    [Test]
    [Description("GetMinutesSinceCheckoutStarted: con fecha de hace 5 minutos debe retornar ≥ 5; con fecha muy reciente debe retornar ≤ 1")]
    public void GetMinutesSinceCheckoutStarted_WhenRecentOrOld_ShouldReturnExpectedMinutes()
    {
        var fiveMinAgo = new dawazonBackend.Cart.Models.Cart
        {
            CheckoutStartedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        Assert.That(fiveMinAgo.GetMinutesSinceCheckoutStarted(), Is.GreaterThanOrEqualTo(5));

        var justStarted = new dawazonBackend.Cart.Models.Cart
        {
            CheckoutStartedAt = DateTime.UtcNow
        };
        Assert.That(justStarted.GetMinutesSinceCheckoutStarted(), Is.LessThanOrEqualTo(1));
    }

    [Test]
    [Description("CartLine.TotalPrice debe ser ProductPrice * Quantity para distintos valores de Quantity")]
    public void CartLine_TotalPrice_ShouldBeProductPriceTimesQuantity()
    {
        Assert.That(new CartLine { ProductPrice = 25.0,  Quantity = 3 }.TotalPrice, Is.EqualTo(75.0).Within(0.001));
        Assert.That(new CartLine { ProductPrice = 99.99, Quantity = 0 }.TotalPrice, Is.EqualTo(0.0));
        Assert.That(new CartLine { ProductPrice = 49.5,  Quantity = 1 }.TotalPrice, Is.EqualTo(49.5).Within(0.001));
    }

    [Test]
    [Description("CartLine new() inicializa Quantity en 0 y campos de cadena en cadena vacía")]
    public void CartLine_New_ShouldHaveSafeDefaults()
    {
        var line = new CartLine();
        Assert.That(line.Quantity,   Is.EqualTo(0));
        Assert.That(line.ProductId,  Is.EqualTo(string.Empty));
        Assert.That(line.CartId,     Is.EqualTo(string.Empty));
        Assert.That(line.Product,    Is.Null);
    }

    [Test]
    [Description("Status enum debe contener los valores de negocio esperados")]
    public void Status_Enum_ShouldContainExpectedValues()
    {
        var values = Enum.GetNames(typeof(Status));
        Assert.That(values, Does.Contain("EnCarrito"));
        Assert.That(values, Does.Contain("Preparado"));
        Assert.That(values, Does.Contain("Enviado"));
    }

    [Test]
    [Description("Address new() y Client new() deben inicializar campos con valores no nulos")]
    public void Address_And_Client_New_ShouldHaveNonNullFields()
    {
        var address = new Address();
        Assert.That(address.Street,   Is.Not.Null);
        Assert.That(address.City,     Is.Not.Null);
        Assert.That(address.Province, Is.Not.Null);
        Assert.That(address.Country,  Is.Not.Null);

        var client = new Client();
        Assert.That(client.Name,  Is.Not.Null);
        Assert.That(client.Email, Is.Not.Null);
        Assert.That(client.Phone, Is.Not.Null);
    }
}
