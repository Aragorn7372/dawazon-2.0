using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Mapper;
using dawazonBackend.Cart.Models;
using NUnit.Framework;

namespace dawazonTest.Cart.Mapper;

[TestFixture]
[Description("CartMapper Unit Tests — SOLID + FIRST Principles para el Backend")]
public class CartMapperTest
{
    private const string CartId = "CART-001";
    private const long UserId = 123L;
    private const double Total = 299.97;
    private const int TotalItems = 2;

    private const string ClientName = "Juan Pérez";
    private const string ClientEmail = "juan@example.com";
    private const string ClientPhone = "600123456";
    private const short ClientNumber = 42;
    private const string ClientStreet = "Calle Principal";
    private const string ClientCity = "Madrid";
    private const string ClientProvince = "Madrid";
    private const string ClientCountry = "España";
    private const int ClientPostal = 28001;

    private const string ProductId1 = "PROD-001";
    private const string ProductName1 = "Producto Test";
    private const double ProductPrice1 = 99.99;
    private const int Quantity1 = 3;
    private const double TotalPrice1 = 299.97;

    [Test]
    [Description("ClientToDto: Debe aplanar correctamente las propiedades de Address")]
    public void ClientToDto_ShouldFlattenAddressProperties()
    {
        // Arrange
        var model = new Client
        {
            Name = ClientName,
            Email = ClientEmail,
            Phone = ClientPhone,
            Address = new Address
            {
                Number = ClientNumber,
                Street = ClientStreet,
                City = ClientCity,
                Province = ClientProvince,
                Country = ClientCountry,
                PostalCode = ClientPostal
            }
        };

        // Act
        ClientDto result = model.ToDto();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(ClientName));
        Assert.That(result.Email, Is.EqualTo(ClientEmail));
        Assert.That(result.Phone, Is.EqualTo(ClientPhone));
        Assert.That(result.Number, Is.EqualTo(ClientNumber));
        Assert.That(result.Street, Is.EqualTo(ClientStreet));
        Assert.That(result.City, Is.EqualTo(ClientCity));
        Assert.That(result.Province, Is.EqualTo(ClientProvince));
        Assert.That(result.Country, Is.EqualTo(ClientCountry));
        Assert.That(result.PostalCode, Is.EqualTo(ClientPostal));
    }

    [Test]
    [Description("ClientToModel: Debe reconstruir correctamente el objeto Address a partir del DTO")]
    public void ClientToModel_ShouldRebuildAddressObject()
    {
        // Arrange
        var dto = new ClientDto
        {
            Name = ClientName,
            Email = ClientEmail,
            Phone = ClientPhone,
            Number = ClientNumber,
            Street = ClientStreet,
            City = ClientCity,
            Province = ClientProvince,
            Country = ClientCountry,
            PostalCode = ClientPostal
        };

        // Act
        Client result = dto.ToModel();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(ClientName));
        Assert.That(result.Email, Is.EqualTo(ClientEmail));
        Assert.That(result.Phone, Is.EqualTo(ClientPhone));
        Assert.That(result.Address, Is.Not.Null);
        Assert.That(result.Address!.Number, Is.EqualTo(ClientNumber));
        Assert.That(result.Address.Street, Is.EqualTo(ClientStreet));
        Assert.That(result.Address.City, Is.EqualTo(ClientCity));
        Assert.That(result.Address.Province, Is.EqualTo(ClientProvince));
        Assert.That(result.Address.Country, Is.EqualTo(ClientCountry));
        Assert.That(result.Address.PostalCode, Is.EqualTo(ClientPostal));
    }

    [Test]
    [Description("CartLineToDto: Debe mapear la línea y extraer datos del carrito padre si se proporciona")]
    public void CartLineToDto_ShouldMapLineAndExtractParentData()
    {
        // Arrange
        var parentCart = new dawazonBackend.Cart.Models.Cart
        {
            Id = CartId,
            UserId = UserId,
            Client = new Client { Name = ClientName }
        };

        var model = new CartLine
        {
            CartId = CartId,
            ProductId = ProductId1,
            Product = new dawazonBackend.Products.Models.Product { Name = ProductName1 },
            Quantity = Quantity1,
            ProductPrice = ProductPrice1,
            Status = Status.EnCarrito
        };

        // Act
        SaleLineDto result = model.ToDto(parentCart);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SaleId, Is.EqualTo(CartId));
        Assert.That(result.ProductId, Is.EqualTo(ProductId1));
        Assert.That(result.ProductName, Is.EqualTo(ProductName1));
        Assert.That(result.Quantity, Is.EqualTo(Quantity1));
        Assert.That(result.ProductPrice, Is.EqualTo(ProductPrice1).Within(0.01));
        Assert.That(result.TotalPrice, Is.EqualTo(TotalPrice1).Within(0.01));
        Assert.That(result.Status, Is.EqualTo(Status.EnCarrito));
        
        // Verifica datos del padre
        Assert.That(result.UserId, Is.EqualTo(UserId));
        Assert.That(result.Client, Is.Not.Null);
        Assert.That(result.Client.Name, Is.EqualTo(ClientName));
    }

    [Test]
    [Description("SaleLineDtoToModel: Debe mapear correctamente de vuelta a CartLine")]
    public void SaleLineDtoToModel_ShouldMapToCartLineCorrectly()
    {
        // Arrange
        var dto = new SaleLineDto
        {
            SaleId = CartId,
            ProductId = ProductId1,
            Quantity = Quantity1,
            ProductPrice = ProductPrice1,
            Status = Status.EnCarrito
        };

        // Act
        CartLine result = dto.ToModel();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CartId, Is.EqualTo(CartId));
        Assert.That(result.ProductId, Is.EqualTo(ProductId1));
        Assert.That(result.Quantity, Is.EqualTo(Quantity1));
        Assert.That(result.ProductPrice, Is.EqualTo(ProductPrice1));
        Assert.That(result.Status, Is.EqualTo(Status.EnCarrito));
    }

    [Test]
    [Description("CartToDto: Debe mapear correctamente el carrito y sus componentes hijos")]
    public void CartToDto_ShouldMapCartAndChildren()
    {
        // Arrange
        var client = new Client { Name = ClientName, Address = new Address { City = ClientCity } };
        var line = new CartLine { CartId = CartId, ProductId = ProductId1, Quantity = Quantity1 };

        var model = new dawazonBackend.Cart.Models.Cart
        {
            Id = CartId,
            UserId = UserId,
            Purchased = true,
            Client = client,
            CartLines = [line],
            TotalItems = 1,
            Total = 99.99
        };

        // Act
        CartResponseDto result = model.ToDto();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(CartId));
        Assert.That(result.UserId, Is.EqualTo(UserId));
        Assert.That(result.Purchased, Is.True);
        Assert.That(result.TotalItems, Is.EqualTo(1));
        Assert.That(result.Total, Is.EqualTo(99.99));
        
        Assert.That(result.Client, Is.Not.Null);
        Assert.That(result.Client.Name, Is.EqualTo(ClientName));
        Assert.That(result.Client.City, Is.EqualTo(ClientCity));
        
        Assert.That(result.CartLines, Has.Count.EqualTo(1));
        Assert.That(result.CartLines[0].SaleId, Is.EqualTo(CartId));
        Assert.That(result.CartLines[0].ProductId, Is.EqualTo(ProductId1));
    }

    [Test]
    [Description("CartDtoToModel: Debe mapear el DTO a un modelo Cart e inicializar campos de gestión")]
    public void CartDtoToModel_ShouldMapAndInitializeManagementFields()
    {
        // Arrange
        var clientDto = new ClientDto { Name = ClientName, City = ClientCity };
        var saleLineDto = new SaleLineDto { SaleId = CartId, ProductId = ProductId1, Quantity = Quantity1 };

        var dto = new CartResponseDto(
            Id: CartId,
            UserId: UserId,
            Purchased: false,
            Client: clientDto,
            CartLines: [saleLineDto],
            TotalItems: 1,
            Total: 99.99
        );

        // Act
        dawazonBackend.Cart.Models.Cart result = dto.ToModel();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(CartId));
        Assert.That(result.UserId, Is.EqualTo(UserId));
        Assert.That(result.Purchased, Is.False);
        Assert.That(result.TotalItems, Is.EqualTo(1));
        Assert.That(result.Total, Is.EqualTo(99.99));
        
        Assert.That(result.Client, Is.Not.Null);
        Assert.That(result.Client.Name, Is.EqualTo(ClientName));
        Assert.That(result.Client.Address!.City, Is.EqualTo(ClientCity));

        Assert.That(result.CartLines, Has.Count.EqualTo(1));
        Assert.That(result.CartLines[0].CartId, Is.EqualTo(CartId));
        
        // Verificamos los campos de gestión (CreatedAt, UploadAt, CheckoutInProgress, CheckoutStartedAt)
        Assert.That(result.CreatedAt.Date, Is.EqualTo(DateTime.UtcNow.Date));
        Assert.That(result.UploadAt.Date, Is.EqualTo(DateTime.UtcNow.Date));
        Assert.That(result.CheckoutInProgress, Is.False);
        Assert.That(result.CheckoutStartedAt, Is.Null);
    }
}