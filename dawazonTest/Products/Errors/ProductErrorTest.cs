using dawazonBackend.Common.Error;
using dawazonBackend.Products.Errors;
using NUnit.Framework;

namespace dawazonTest.Products.Errors;

[TestFixture]
[Description("ProductError Tests Unitarios — Principios SOLID + FIRST")]
public class ProductErrorTest
{
    [Test]
    [Description("ProductError: Debe almacenar y exponer el Message")]
    public void ProductError_ShouldStoreMessage()
    {
        var error = new ProductError("error genérico");
        Assert.That(error.Message, Is.EqualTo("error genérico"));
    }

    [Test]
    [Description("ProductError: Debe heredar de DomainError")]
    public void ProductError_ShouldInheritFromDomainError()
    {
        DomainError error = new ProductError("test");
        Assert.That(error, Is.InstanceOf<DomainError>());
    }

    [Test]
    [Description("ProductError: Dos instancias con el mismo Message deben ser iguales (record)")]
    public void ProductError_SameMessage_ShouldBeEqual()
    {
        var a = new ProductError("duplicado");
        var b = new ProductError("duplicado");
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    [Description("ProductError: Dos instancias con distinto Message no deben ser iguales")]
    public void ProductError_DifferentMessage_ShouldNotBeEqual()
    {
        var a = new ProductError("uno");
        var b = new ProductError("dos");
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    [Description("ProductNotFoundError: Debe almacenar el Message")]
    public void ProductNotFoundError_ShouldStoreMessage()
    {
        var error = new ProductNotFoundError("no encontrado");
        Assert.That(error.Message, Is.EqualTo("no encontrado"));
    }

    [Test]
    [Description("ProductNotFoundError: Debe heredar de ProductError")]
    public void ProductNotFoundError_ShouldInheritFromProductError()
    {
        ProductError error = new ProductNotFoundError("test");
        Assert.That(error, Is.InstanceOf<ProductError>());
    }

    [Test]
    [Description("ProductValidationError: Debe almacenar el Message")]
    public void ProductValidationError_ShouldStoreMessage()
    {
        var error = new ProductValidationError("validación fallida");
        Assert.That(error.Message, Is.EqualTo("validación fallida"));
    }

    [Test]
    [Description("ProductValidationError: Debe heredar de ProductError")]
    public void ProductValidationError_ShouldInheritFromProductError()
    {
        ProductError error = new ProductValidationError("test");
        Assert.That(error, Is.InstanceOf<ProductError>());
    }

    [Test]
    [Description("ProductBadRequestError: Debe almacenar el Message")]
    public void ProductBadRequestError_ShouldStoreMessage()
    {
        var error = new ProductBadRequestError("bad request");
        Assert.That(error.Message, Is.EqualTo("bad request"));
    }

    [Test]
    [Description("ProductBadRequestError: Debe heredar de ProductError")]
    public void ProductBadRequestError_ShouldInheritFromProductError()
    {
        ProductError error = new ProductBadRequestError("test");
        Assert.That(error, Is.InstanceOf<ProductError>());
    }

    [Test]
    [Description("ProductConflictError: Debe almacenar el Message")]
    public void ProductConflictError_ShouldStoreMessage()
    {
        var error = new ProductConflictError("conflicto");
        Assert.That(error.Message, Is.EqualTo("conflicto"));
    }

    [Test]
    [Description("ProductConflictError: Debe heredar de ProductError")]
    public void ProductConflictError_ShouldInheritFromProductError()
    {
        ProductError error = new ProductConflictError("test");
        Assert.That(error, Is.InstanceOf<ProductError>());
    }

    [Test]
    [Description("ProductStorageError: Debe almacenar el Message")]
    public void ProductStorageError_ShouldStoreMessage()
    {
        var error = new ProductStorageError("storage error");
        Assert.That(error.Message, Is.EqualTo("storage error"));
    }

    [Test]
    [Description("ProductStorageError: Debe heredar de ProductError")]
    public void ProductStorageError_ShouldInheritFromProductError()
    {
        ProductError error = new ProductStorageError("test");
        Assert.That(error, Is.InstanceOf<ProductError>());
    }

    [Test]
    [Description("InsufficientStockError: Debe almacenar el Message")]
    public void InsufficientStockError_ShouldStoreMessage()
    {
        var error = new InsufficientStockError("stock insuficiente");
        Assert.That(error.Message, Is.EqualTo("stock insuficiente"));
    }

    [Test]
    [Description("InsufficientStockError: Debe heredar de ProductError")]
    public void InsufficientStockError_ShouldInheritFromProductError()
    {
        ProductError error = new InsufficientStockError("test");
        Assert.That(error, Is.InstanceOf<ProductError>());
    }

    [Test]
    [Description("InsufficientStockError: Debe ser instancia de DomainError (herencia transitiva)")]
    public void InsufficientStockError_ShouldAlsoInheritFromDomainError()
    {
        DomainError error = new InsufficientStockError("test");
        Assert.That(error, Is.InstanceOf<DomainError>());
    }
}
