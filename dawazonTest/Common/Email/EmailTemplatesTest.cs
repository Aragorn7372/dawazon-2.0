using dawazonBackend.Cart.Models;
using dawazonBackend.Common.Mail;
using NUnit.Framework;

namespace dawazonTest.Common.Email;

[TestFixture]
[Description("EmailTemplates Unit Tests — SOLID + FIRST Principles")]
public class EmailTemplatesTest
{
    [Test]
    [Description("CreateBase: Debe incluir el título en el <title> del HTML")]
    public void CreateBase_ShouldIncludeTitleInHtmlTitle()
    {
        var result = EmailTemplates.CreateBase("Mi Título", "Contenido");

        Assert.That(result, Does.Contain("<title>Mi Título</title>"));
    }

    [Test]
    [Description("CreateBase: Debe incluir el título en el <h2> del contenido")]
    public void CreateBase_ShouldIncludeTitleInH2()
    {
        var result = EmailTemplates.CreateBase("Bienvenido", "Cuerpo del email");

        Assert.That(result, Does.Contain(">Bienvenido<"));
    }

    [Test]
    [Description("CreateBase: Debe incluir el contenido en el cuerpo del HTML")]
    public void CreateBase_ShouldIncludeContentInBody()
    {
        var result = EmailTemplates.CreateBase("Título", "Este es el contenido del email");

        Assert.That(result, Does.Contain("Este es el contenido del email"));
    }

    [Test]
    [Description("CreateBase: Debe retornar un HTML válido (empieza con <!DOCTYPE html>)")]
    public void CreateBase_ShouldReturnValidHtmlDoctype()
    {
        var result = EmailTemplates.CreateBase("Título", "Contenido");

        Assert.That(result.TrimStart(), Does.StartWith("<!DOCTYPE html>"));
    }

    [Test]
    [Description("CreateBase: El HTML debe contener la estructura del header con 'Funko API'")]
    public void CreateBase_ShouldContainBrandName()
    {
        var result = EmailTemplates.CreateBase("Título", "Contenido");

        Assert.That(result, Does.Contain("Funko API"));
    }

    [Test]
    [Description("CreateBase: El HTML debe contener el footer con el email de soporte")]
    public void CreateBase_ShouldContainFooter()
    {
        var result = EmailTemplates.CreateBase("Título", "Contenido");

        Assert.That(result, Does.Contain("soporte@funkoapi.com"));
        Assert.That(result, Does.Contain("© 2026 Funko API"));
    }

    [Test]
    [Description("CreateBase: Debe cerrar la etiqueta </html>")]
    public void CreateBase_ShouldCloseHtmlTag()
    {
        var result = EmailTemplates.CreateBase("Título", "Contenido");

        Assert.That(result, Does.Contain("</html>"));
    }

    [Test]
    [Description("CreateBase: Debe incluir meta charset UTF-8")]
    public void CreateBase_ShouldIncludeUtf8Charset()
    {
        var result = EmailTemplates.CreateBase("Título", "Contenido");

        Assert.That(result, Does.Contain("charset='UTF-8'"));
    }

    [Test]
    [Description("CreateBase: Con título y contenido vacíos no debe lanzar excepción")]
    public void CreateBase_WithEmptyStrings_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() => EmailTemplates.CreateBase("", ""));
    }

    [Test]
    [Description("PedidoConfirmado: Debe incluir el nombre del cliente")]
    public void PedidoConfirmado_ShouldIncludeClientName()
    {
        var cart = BuildCart();

        var result = EmailTemplates.PedidoConfirmado(cart);

        Assert.That(result, Does.Contain("Juan Pérez"));
    }

    [Test]
    [Description("PedidoConfirmado: Debe incluir la dirección del cliente")]
    public void PedidoConfirmado_ShouldIncludeClientAddress()
    {
        var cart = BuildCart();

        var result = EmailTemplates.PedidoConfirmado(cart);

        Assert.That(result, Does.Contain("Calle Mayor"));
        Assert.That(result, Does.Contain("Madrid"));
    }

    [Test]
    [Description("PedidoConfirmado: Debe incluir el ID de cada producto del carrito")]
    public void PedidoConfirmado_ShouldIncludeProductIds()
    {
        var cart = BuildCart();

        var result = EmailTemplates.PedidoConfirmado(cart);

        Assert.That(result, Does.Contain("PRD001"));
        Assert.That(result, Does.Contain("PRD002"));
    }

    [Test]
    [Description("PedidoConfirmado: Debe incluir el total del carrito")]
    public void PedidoConfirmado_ShouldIncludeCartTotal()
    {
        var cart = BuildCart();

        var result = EmailTemplates.PedidoConfirmado(cart);

        Assert.That(result, Does.Contain("125"));
    }

    [Test]
    [Description("PedidoConfirmado: Debe incluir el total de items")]
    public void PedidoConfirmado_ShouldIncludeTotalItems()
    {
        var cart = BuildCart();

        var result = EmailTemplates.PedidoConfirmado(cart);

        Assert.That(result, Does.Contain("Total Items:"));
        Assert.That(result, Does.Contain("3"));
    }

    [Test]
    [Description("PedidoConfirmado: Debe incluir el mensaje de agradecimiento")]
    public void PedidoConfirmado_ShouldIncludeThanksMessage()
    {
        var cart = BuildCart();

        var result = EmailTemplates.PedidoConfirmado(cart);

        Assert.That(result, Does.Contain("¡Gracias por tu compra!"));
    }

    [Test]
    [Description("PedidoConfirmado: Con carrito sin líneas debe retornar HTML sin filas de producto")]
    public void PedidoConfirmado_WithEmptyCartLines_ShouldReturnValidHtml()
    {
        var cart = new dawazonBackend.Cart.Models.Cart
        {
            Id = "C1",
            TotalItems = 0,
            Total = 0,
            CartLines = [],
            Client = new Client
            {
                Name = "Sin productos",
                Address = new Address { Street = "Calle X", Number = 1, City = "Burgos" }
            }
        };

        Assert.DoesNotThrow(() => EmailTemplates.PedidoConfirmado(cart));

        var result = EmailTemplates.PedidoConfirmado(cart);
        Assert.That(result, Does.Contain("Sin productos"));
    }

    [Test]
    [Description("PedidoConfirmado: Debe incluir la cantidad de cada línea")]
    public void PedidoConfirmado_ShouldIncludeLineQuantities()
    {
        var cart = BuildCart();

        var result = EmailTemplates.PedidoConfirmado(cart);

        Assert.That(result, Does.Contain(">2<"));
    }

    [Test]
    [Description("ProductoCreado: Debe incluir el nombre del producto")]
    public void ProductoCreado_ShouldIncludeProductName()
    {
        var result = EmailTemplates.ProductoCreado("Funko Iron Man", 15.99, "Figuras", 42);

        Assert.That(result, Does.Contain("Funko Iron Man"));
    }

    [Test]
    [Description("ProductoCreado: Debe incluir el precio formateado")]
    public void ProductoCreado_ShouldIncludePrice()
    {
        var result = EmailTemplates.ProductoCreado("Producto Test", 99.50, "Ropa", 1);

        Assert.That(result, Does.Contain("99"));
    }

    [Test]
    [Description("ProductoCreado: Debe incluir la categoría")]
    public void ProductoCreado_ShouldIncludeCategory()
    {
        var result = EmailTemplates.ProductoCreado("Producto", 10.0, "Comics", 7);

        Assert.That(result, Does.Contain("Comics"));
    }

    [Test]
    [Description("ProductoCreado: Debe incluir el ID del creador")]
    public void ProductoCreado_ShouldIncludeCreatorId()
    {
        var result = EmailTemplates.ProductoCreado("Producto", 10.0, "Ropa", 99);

        Assert.That(result, Does.Contain("99"));
    }

    [Test]
    [Description("ProductoCreado: Debe incluir el mensaje de confirmación de disponibilidad")]
    public void ProductoCreado_ShouldIncludeAvailabilityMessage()
    {
        var result = EmailTemplates.ProductoCreado("Producto", 10.0, "Ropa", 1);

        Assert.That(result, Does.Contain("El producto ya está disponible en la tienda."));
    }

    [Test]
    [Description("ProductoCreado: Debe incluir las cabeceras de la tabla (ID, Nombre, Precio, Categoría)")]
    public void ProductoCreado_ShouldIncludeTableHeaders()
    {
        var result = EmailTemplates.ProductoCreado("Producto", 10.0, "Ropa", 1);

        Assert.That(result, Does.Contain("ID:"));
        Assert.That(result, Does.Contain("Nombre:"));
        Assert.That(result, Does.Contain("Precio:"));
        Assert.That(result, Does.Contain("Categoría:"));
    }

    [Test]
    [Description("ProductoCreado: Con precio 0 no debe lanzar excepción")]
    public void ProductoCreado_WithZeroPrice_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() => EmailTemplates.ProductoCreado("Gratis", 0.0, "Promo", 1));
    }

    [Test]
    [Description("ProductoCreado: Con nombre y categoría vacíos no debe lanzar excepción")]
    public void ProductoCreado_WithEmptyStrings_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() => EmailTemplates.ProductoCreado("", 5.0, "", 0));
    }

    private static dawazonBackend.Cart.Models.Cart BuildCart() => new()
    {
        Id = "CART001",
        TotalItems = 3,
        Total = 125.98,
        Client = new Client
        {
            Name = "Juan Pérez",
            Address = new Address
            {
                Street = "Calle Mayor",
                Number = 5,
                City = "Madrid"
            }
        },
        CartLines =
        [
            new CartLine { ProductId = "PRD001", Quantity = 2, ProductPrice = 15.99 },
            new CartLine { ProductId = "PRD002", Quantity = 1, ProductPrice = 94.00 }
        ]
    };
}