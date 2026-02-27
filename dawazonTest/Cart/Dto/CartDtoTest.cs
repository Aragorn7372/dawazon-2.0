using System.ComponentModel.DataAnnotations;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Models;
using NUnit.Framework;

namespace dawazonTest.Cart.Dto;

[TestFixture]
[Description("Cart Dto Unit Tests — SOLID + FIRST Principles")]
public class CartDtoTest
{
    private static IList<ValidationResult> Validate(object obj)
    {
        var results = new List<ValidationResult>();
        var ctx     = new ValidationContext(obj);
        Validator.TryValidateObject(obj, ctx, results, validateAllProperties: true);
        return results;
    }

    [Test]
    [Description("ClientDto new() debe inicializar strings en string.Empty")]
    public void ClientDto_New_ShouldHaveEmptyStringDefaults()
    {
        var dto = new ClientDto();
        Assert.That(dto.Name,     Is.EqualTo(string.Empty));
        Assert.That(dto.Email,    Is.EqualTo(string.Empty));
        Assert.That(dto.Phone,    Is.EqualTo(string.Empty));
        Assert.That(dto.Street,   Is.EqualTo(string.Empty));
        Assert.That(dto.City,     Is.EqualTo(string.Empty));
        Assert.That(dto.Province, Is.EqualTo(string.Empty));
        Assert.That(dto.Country,  Is.EqualTo(string.Empty));
    }

    [Test]
    [Description("ClientDto válido no debe generar errores de validación")]
    public void ClientDto_WhenValid_ShouldPassValidation()
    {
        var dto = new ClientDto
        {
            Name       = "Juan Pérez",
            Email      = "juan@example.com",
            Phone      = "600123456",
            Number     = 42,
            Street     = "Calle Principal",
            City       = "Madrid",
            Province   = "Madrid",
            Country    = "España",
            PostalCode = 28001
        };

        var errors = Validate(dto);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    [Description("ClientDto con Email inválido debe generar error de validación")]
    public void ClientDto_WithInvalidEmail_ShouldFailValidation()
    {
        var dto = new ClientDto
        {
            Name = "Test", Email = "no-es-email", Phone = "600000000",
            Street = "Calle A", City = "Madrid", Province = "Madrid", Country = "España"
        };

        var errors = Validate(dto);
        Assert.That(errors, Has.Count.GreaterThan(0));
        Assert.That(errors.Any(e => e.MemberNames.Contains("Email")), Is.True);
    }

    [Test]
    [Description("ClientDto con teléfono que no cumple el patrón (9 dígitos) debe fallar validación")]
    public void ClientDto_WithInvalidPhone_ShouldFailValidation()
    {
        var dto = new ClientDto
        {
            Name = "Test", Email = "test@test.com", Phone = "123",
            Street = "Calle A", City = "Madrid", Province = "Madrid", Country = "España"
        };

        var errors = Validate(dto);
        Assert.That(errors.Any(e => e.MemberNames.Contains("Phone")), Is.True);
    }

    [Test]
    [Description("ClientDto es un record: instancias con mismos valores deben ser iguales")]
    public void ClientDto_WithSameValues_ShouldBeEqual()
    {
        var a = new ClientDto { Name = "Ana", City = "Bilbao" };
        var b = new ClientDto { Name = "Ana", City = "Bilbao" };
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    [Description("SaleLineDto new() debe tener defaults seguros y GetUserName/GetUserEmail deben retornar los datos del cliente")]
    public void SaleLineDto_DefaultsAndClientHelpers_ShouldWork()
    {
        var dto = new SaleLineDto();
        Assert.That(dto.SaleId,      Is.EqualTo(string.Empty));
        Assert.That(dto.ProductId,   Is.EqualTo(string.Empty));
        Assert.That(dto.ProductName, Is.EqualTo(string.Empty));
        Assert.That(dto.ManagerName, Is.EqualTo(string.Empty));
        Assert.That(dto.Client,      Is.Not.Null);

        var withClient = new SaleLineDto
        {
            Client = new Client { Name = "María López", Email = "cliente@shop.com" }
        };
        Assert.That(withClient.GetUserName(),  Is.EqualTo("María López"));
        Assert.That(withClient.GetUserEmail(), Is.EqualTo("cliente@shop.com"));
    }

    [Test]
    [Description("SaleLineDto permite asignar todos sus campos y recuperarlos correctamente")]
    public void SaleLineDto_Properties_ShouldBeAssignableAndReadable()
    {
        var now = DateTime.UtcNow;
        var dto = new SaleLineDto
        {
            SaleId       = "S1",
            ProductId    = "P1",
            ProductName  = "Funko Pop",
            Quantity     = 2,
            ProductPrice = 19.99,
            TotalPrice   = 39.98,
            Status       = Status.Preparado,
            ManagerId    = 5L,
            ManagerName  = "Admin",
            UserId       = 10L,
            CreateAt     = now,
            UpdateAt     = now,
        };

        Assert.That(dto.SaleId,       Is.EqualTo("S1"));
        Assert.That(dto.ProductId,    Is.EqualTo("P1"));
        Assert.That(dto.Quantity,     Is.EqualTo(2));
        Assert.That(dto.ProductPrice, Is.EqualTo(19.99).Within(0.001));
        Assert.That(dto.TotalPrice,   Is.EqualTo(39.98).Within(0.001));
        Assert.That(dto.Status,       Is.EqualTo(Status.Preparado));
        Assert.That(dto.ManagerId,    Is.EqualTo(5L));
        Assert.That(dto.UserId,       Is.EqualTo(10L));
    }

    [Test]
    [Description("CartResponseDto constructor")]
    public void CartResponseDto_Constructor_ShouldSetAllProperties()
    {
        var client    = new ClientDto { Name = "Test", City = "Madrid" };
        var lines     = new List<SaleLineDto> { new SaleLineDto { ProductId = "P1" } };
        var dto       = new CartResponseDto("C1", 5L, true, client, lines, 1, 99.9);

        Assert.That(dto.Id,         Is.EqualTo("C1"));
        Assert.That(dto.UserId,     Is.EqualTo(5L));
        Assert.That(dto.Purchased,  Is.True);
        Assert.That(dto.Total,      Is.EqualTo(99.9).Within(0.001));
        Assert.That(dto.TotalItems, Is.EqualTo(1));
        Assert.That(dto.CartLines,  Has.Count.EqualTo(1));
        Assert.That(dto.Client,     Is.EqualTo(client));
    }

    [Test]
    [Description("CartResponseDto record: valores iguales")]
    public void CartResponseDto_WithSameValues_ShouldBeEqual()
    {
        var client = new ClientDto { Name = "X" };
        var lines  = new List<SaleLineDto>();
        var a      = new CartResponseDto("C1", 1L, false, client, lines, 0, 0.0);
        var b      = new CartResponseDto("C1", 1L, false, client, lines, 0, 0.0);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    [Description("FilterCartDto defaults y personalizados")]
    public void FilterCartDto_DefaultsCustomAndEquality_ShouldWork()
    {
        var defaults = new FilterCartDto(null, null, null);
        Assert.That(defaults.Page,      Is.EqualTo(0));
        Assert.That(defaults.Size,      Is.EqualTo(10));
        Assert.That(defaults.SortBy,    Is.EqualTo("id"));
        Assert.That(defaults.Direction, Is.EqualTo("asc"));

        var custom = new FilterCartDto(
            managerId: 1L, isAdmin: true, purchased: false,
            Page: 2, Size: 5, SortBy: "total", Direction: "desc");
        Assert.That(custom.managerId,  Is.EqualTo(1L));
        Assert.That(custom.isAdmin,    Is.True);
        Assert.That(custom.purchased,  Is.False);
        Assert.That(custom.Page,       Is.EqualTo(2));
        Assert.That(custom.Size,       Is.EqualTo(5));
        Assert.That(custom.SortBy,     Is.EqualTo("total"));
        Assert.That(custom.Direction,  Is.EqualTo("desc"));

        var eq1 = new FilterCartDto(null, null, true, 0, 10, "id", "asc");
        var eq2 = new FilterCartDto(null, null, true, 0, 10, "id", "asc");
        Assert.That(eq1, Is.EqualTo(eq2));
    }

    [Test]
    [Description("LineRequestDto, defaults y campos asignables")]
    public void LineRequestDto_DefaultsAndProperties_ShouldWork()
    {
        var empty = new LineRequestDto();
        Assert.That(empty.CartId,    Is.EqualTo(string.Empty));
        Assert.That(empty.ProductId, Is.EqualTo(string.Empty));

        var dto = new LineRequestDto
        {
            CartId    = "C1",
            ProductId = "P1",
            Status    = Status.Enviado
        };
        Assert.That(dto.CartId,    Is.EqualTo("C1"));
        Assert.That(dto.ProductId, Is.EqualTo("P1"));
        Assert.That(dto.Status,    Is.EqualTo(Status.Enviado));
    }

    [Test]
    [Description("CartStockRequestDto defaults, igualdad y campos asignables")]
    public void CartStockRequestDto_DefaultsEqualityAndProperties_ShouldWork()
    {
        var empty = new CartStockRequestDto();
        Assert.That(empty.CartId,   Is.Null);
        Assert.That(empty.UserId,   Is.EqualTo(string.Empty));
        Assert.That(empty.Quantity, Is.EqualTo(0));

        var a = new CartStockRequestDto { CartId = "C1", UserId = "1", Quantity = 5 };
        var b = new CartStockRequestDto { CartId = "C1", UserId = "1", Quantity = 5 };
        Assert.That(a, Is.EqualTo(b));

        var dto = new CartStockRequestDto
        {
            CartId   = "CART_001",
            UserId   = "42",
            Quantity = 10
        };
        Assert.That(dto.CartId,   Is.EqualTo("CART_001"));
        Assert.That(dto.UserId,   Is.EqualTo("42"));
        Assert.That(dto.Quantity, Is.EqualTo(10));
    }
}
