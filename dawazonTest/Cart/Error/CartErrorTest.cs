using dawazonBackend.Cart.Errors;
using dawazonBackend.Common.Error;
using NUnit.Framework;

namespace dawazonTest.Cart.Error;

[TestFixture]
[Description("CartError Unit Tests — SOLID + FIRST Principles")]
public class CartErrorTest
{
    [Test]
    [Description("CartError debe ser instancia de DomainError (LSP) y conservar el mensaje")]
    public void CartError_ShouldBeDomainErrorAndPreserveMessage()
    {
        const string msg = "error de carrito";
        var error = new CartError(msg);
        Assert.That(error, Is.InstanceOf<DomainError>());
        Assert.That(error.Message, Is.EqualTo(msg));
    }

    [Test]
    [Description("CartNotFoundError debe ser instancia de CartError (LSP) y conservar el mensaje")]
    public void CartNotFoundError_ShouldBeCartErrorAndPreserveMessage()
    {
        const string msg = "Carrito no encontrado";
        var error = new CartNotFoundError(msg);
        Assert.That(error, Is.InstanceOf<CartError>());
        Assert.That(error.Message, Is.EqualTo(msg));
    }

    [Test]
    [Description("CartProductQuantityExceededError debe ser instancia de CartError, tener mensaje por defecto y admitir mensaje personalizado")]
    public void CartProductQuantityExceededError_InheritanceAndMessages_ShouldWork()
    {
        var defaultError = new CartProductQuantityExceededError();
        Assert.That(defaultError, Is.InstanceOf<CartError>());
        Assert.That(defaultError.Message, Is.Not.Empty);

        const string custom = "stock insuficiente";
        var customError = new CartProductQuantityExceededError(custom);
        Assert.That(customError.Message, Is.EqualTo(custom));
    }

    [Test]
    [Description("CartAttemptAmountExceededError debe ser instancia de CartError y tener mensaje por defecto")]
    public void CartAttemptAmountExceededError_ShouldBeCartErrorWithDefaultMessage()
    {
        var error = new CartAttemptAmountExceededError();
        Assert.That(error, Is.InstanceOf<CartError>());
        Assert.That(error.Message, Is.Not.Empty);
    }

    [Test]
    [Description("CartUnauthorizedError debe conservar el mensaje y ser instancia de CartError")]
    public void CartUnauthorizedError_ShouldPreserveMessage()
    {
        const string msg = "no autorizado";
        var error = new CartUnauthorizedError(msg);
        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<CartError>());
    }

    [Test]
    [Description("CartMinQuantityError debe conservar el mensaje y ser instancia de CartError")]
    public void CartMinQuantityError_ShouldPreserveMessage()
    {
        const string msg = "cantidad mínima 1";
        var error = new CartMinQuantityError(msg);
        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<CartError>());
    }

    [Test]
    [Description("Records con el mismo mensaje deben ser iguales por valor; con distinto, no iguales")]
    public void CartNotFoundError_EqualityByValue_ShouldWork()
    {
        var a = new CartNotFoundError("mismo");
        var b = new CartNotFoundError("mismo");
        var c = new CartNotFoundError("diferente");

        Assert.That(a, Is.EqualTo(b));
        Assert.That(a, Is.Not.EqualTo(c));
    }
}
