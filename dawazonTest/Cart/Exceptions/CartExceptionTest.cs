using dawazonBackend.Cart.Exceptions;
using NUnit.Framework;

namespace dawazonTest.Cart.Exceptions;

[TestFixture]
[Description("CartException Unit Tests — SOLID + FIRST Principles")]
public class CartExceptionTest
{
    [Test]
    [Description("CartException debe ser instancia de System.Exception, conservar el mensaje y poder lanzarse/capturarse")]
    public void CartException_InheritanceMessageAndThrow_ShouldWork()
    {
        const string msg = "error de carrito";
        var ex = new CartException(msg);
        Assert.That(ex, Is.InstanceOf<Exception>());
        Assert.That(ex.Message, Is.EqualTo(msg));

        var caught = Assert.Catch<Exception>(() => throw new CartException("boom"));
        Assert.That(caught?.Message, Is.EqualTo("boom"));
        Assert.That(caught, Is.InstanceOf<CartException>());
    }

    [Test]
    [Description("CartNotFoundException debe ser instancia de CartException, conservar el mensaje y poder capturarse como CartException y Exception base (LSP)")]
    public void CartNotFoundException_InheritanceMessageAndThrow_ShouldWork()
    {
        const string msg = "Carrito 'C1' no encontrado";
        var ex = new CartNotFoundException(msg);
        Assert.That(ex, Is.InstanceOf<CartException>());
        Assert.That(ex.Message, Is.EqualTo(msg));

        var caughtAsCart = Assert.Catch<CartException>(() =>
            throw new CartNotFoundException("not found"));
        Assert.That(caughtAsCart?.Message, Is.EqualTo("not found"));
        Assert.That(caughtAsCart, Is.InstanceOf<CartNotFoundException>());

        var caughtAsBase = Assert.Catch<Exception>(() =>
            throw new CartNotFoundException("not found"));
        Assert.That(caughtAsBase, Is.InstanceOf<CartNotFoundException>());
    }
}
