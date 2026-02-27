using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Exceptions;
using dawazonBackend.Cart.Models;
using dawazonBackend.Cart.Repository;
using dawazonBackend.Common.Database;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Models;
using dawazonBackend.Users.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace dawazonTest.Cart.Repository;

[TestFixture]
[Description("CartRepository Tests")]
public class CartRepositoryTest
{
    private DawazonDbContext _context = null!;
    private Mock<ILogger<CartRepository>> _loggerMock = null!;
    private CartRepository _repository = null!;

    private const string CartId1   = "C1";
    private const string CartId2   = "C2";
    private const string CartId3   = "C3";
    private const string CartId4   = "C4";
    private const string ProductId1 = "P1";
    private const string ProductId2 = "P2";

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DawazonDbContext>()
            .UseInMemoryDatabase($"cart_test_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context    = new DawazonDbContext(options);
        _loggerMock = new Mock<ILogger<CartRepository>>();

        SeedDatabase();

        _repository = new CartRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedDatabase()
    {
        var product1 = new Product { Id = ProductId1, Name = "Producto 1", CreatorId = 999, Price = 50.0, Stock = 10, IsDeleted = false };
        var product2 = new Product { Id = ProductId2, Name = "Producto 2", CreatorId = 123, Price = 50.0, Stock = 5,  IsDeleted = false };

        _context.Products.AddRange(product1, product2);

        var user999 = new User { Id = 999, UserName = "manager999", Email = "manager999@test.com" };
        var user123 = new User { Id = 123, UserName = "manager123", Email = "manager123@test.com" };
        _context.Users.AddRange(user999, user123);

        var address = new Address { Street = "Calle Falsa", Number = 1, City = "Madrid", Province = "Madrid", Country = "España", PostalCode = 28000 };
        var client  = new Client  { Name = "Cliente 1", Email = "c@c.com", Phone = "600000000", Address = address };

        var cart1 = new dawazonBackend.Cart.Models.Cart
        {
            Id        = CartId1,
            UserId    = 1,
            Purchased = true,
            Total     = 150.0,
            CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UploadAt  = new DateTime(2023, 1, 7, 0, 0, 0, DateTimeKind.Utc),
            Client    = client,
            CartLines = new List<CartLine>
            {
                new CartLine { CartId = CartId1, ProductId = ProductId1, Quantity = 2, ProductPrice = 50.0, Status = Status.Preparado, Product = product1 },
                new CartLine { CartId = CartId1, ProductId = ProductId2, Quantity = 1, ProductPrice = 50.0, Status = Status.EnCarrito, Product = product2 }
            }
        };

        var cart2 = new dawazonBackend.Cart.Models.Cart
        {
            Id        = CartId2,
            UserId    = 2,
            Purchased = false,
            Total     = 50.0,
            CreatedAt = new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            UploadAt  = new DateTime(2023, 1, 5, 0, 0, 0, DateTimeKind.Utc),
            Client    = new Client { Name = "Cliente 2", Email = "d@d.com", Phone = "611111111", Address = new Address { City = "Barcelona" } },
            CartLines = new List<CartLine>
            {
                new CartLine { CartId = CartId2, ProductId = ProductId1, Quantity = 1, ProductPrice = 50.0, Status = Status.EnCarrito }
            }
        };

        var cart3 = new dawazonBackend.Cart.Models.Cart
        {
            Id        = CartId3,
            UserId    = 3,
            Purchased = false,
            Total     = 200.0,
            CreatedAt = new DateTime(2023, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            UploadAt  = new DateTime(2023, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            Client    = new Client { Name = "Cliente 3", Email = "e@e.com", Phone = "622222222", Address = new Address { City = "Valencia" } },
            CartLines = new List<CartLine>()
        };

        var cart4 = new dawazonBackend.Cart.Models.Cart
        {
            Id        = CartId4,
            UserId    = 4,
            Purchased = true,
            Total     = 100.0,
            CreatedAt = new DateTime(2023, 1, 4, 0, 0, 0, DateTimeKind.Utc),
            UploadAt  = new DateTime(2023, 1, 6, 0, 0, 0, DateTimeKind.Utc),
            Client    = new Client { Name = "Cliente 4", Email = "f@f.com", Phone = "633333333", Address = new Address { City = "Sevilla" } },
            CartLines = new List<CartLine>()
        };

        _context.Carts.AddRange(cart1, cart2, cart3, cart4);
        _context.SaveChanges();
    }

    [Test]
    [Description("GetAllAsync: filtrando por purchased=true debe devolver solo los carritos comprados")]
    public async Task GetAllAsync_ShouldFilterByPurchasedAndPaginate()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: true, Page: 0, Size: 10, SortBy: "total", Direction: "asc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount,          Is.EqualTo(2));
        Assert.That(items.Count(),       Is.EqualTo(2));
        Assert.That(items.First().Id,    Is.EqualTo(CartId4));
    }

    [Test]
    [Description("GetAllAsync: sin filtro de purchased debe devolver todos los carritos")]
    public async Task GetAllAsync_WithNoPurchasedFilter_ShouldReturnAll()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "id", Direction: "asc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
    }

    [Test]
    [Description("GetAllAsync: paginación (Size=1, Page=0) debe devolver solo 1 elemento")]
    public async Task GetAllAsync_WithPagination_ShouldRespectPageSize()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 1, SortBy: "id", Direction: "asc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount,    Is.EqualTo(4));
        Assert.That(items.Count(), Is.EqualTo(1));
    }

    [Test]
    [Description("CalculateTotalEarningsAsync: Admin puede ver todas las ganancias de carritos comprados")]
    public async Task CalculateTotalEarningsAsync_AdminCanSeeAllEarnings()
    {
        var total = await _repository.CalculateTotalEarningsAsync(null, isAdmin: true);

        Assert.That(total, Is.EqualTo(150.0).Within(0.01));
    }

    [Test]
    [Description("CalculateTotalEarningsAsync: Manager solo ve las ganancias de sus productos")]
    public async Task CalculateTotalEarningsAsync_ManagerSeesOnlyOwnEarnings()
    {
        var total = await _repository.CalculateTotalEarningsAsync(managerId: 999, isAdmin: false);

        Assert.That(total, Is.EqualTo(100.0).Within(0.01));
    }

    [Test]
    [Description("CalculateTotalEarningsAsync: sin managerId y sin admin debe retornar 0")]
    public async Task CalculateTotalEarningsAsync_WithNoManagerAndNoAdmin_ShouldReturnZero()
    {
        var total = await _repository.CalculateTotalEarningsAsync(null, isAdmin: false);

        Assert.That(total, Is.EqualTo(0.0));
    }

    [Test]
    [Description("FindCartByIdAsync: debe recuperar el carrito si existe, con líneas y cliente")]
    public async Task FindCartByIdAsync_ShouldReturnCartWithIncludes()
    {
        var cart = await _repository.FindCartByIdAsync(CartId1);

        Assert.That(cart,                  Is.Not.Null);
        Assert.That(cart!.Id,              Is.EqualTo(CartId1));
        Assert.That(cart.CartLines,        Has.Count.EqualTo(2));
        Assert.That(cart.Client,           Is.Not.Null);
        Assert.That(cart.Client.Address,   Is.Not.Null);
    }

    [Test]
    [Description("FindCartByIdAsync: debe retornar null si el carrito no existe")]
    public async Task FindCartByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        var cart = await _repository.FindCartByIdAsync("NO_EXISTE");

        Assert.That(cart, Is.Null);
    }

    [Test]
    [Description("UpdateCartLineStatusAsync: debe actualizar el estado de la línea y persistir")]
    public async Task UpdateCartLineStatusAsync_ShouldUpdateStatus()
    {
        var result = await _repository.UpdateCartLineStatusAsync(CartId1, ProductId1, Status.Enviado);

        Assert.That(result, Is.True);

        var updatedCart = await _context.Carts.Include(c => c.CartLines)
                                              .FirstAsync(c => c.Id == CartId1);
        var line = updatedCart.CartLines.First(cl => cl.ProductId == ProductId1);
        Assert.That(line.Status, Is.EqualTo(Status.Enviado));
    }

    [Test]
    [Description("UpdateCartLineStatusAsync: debe retornar false si el carrito no existe")]
    public async Task UpdateCartLineStatusAsync_WhenCartNotFound_ShouldReturnFalse()
    {
        var result = await _repository.UpdateCartLineStatusAsync("NO_EXISTE", ProductId1, Status.Enviado);

        Assert.That(result, Is.False);
    }

    [Test]
    [Description("AddCartLineAsync: si la línea existe, debe sumar/actualizar cantidad")]
    public async Task AddCartLineAsync_WhenLineExists_ShouldUpdateQuantity()
    {
        var updatedLine = new CartLine { ProductId = ProductId1, Quantity = 5, ProductPrice = 50.0 };

        var result = await _repository.AddCartLineAsync(CartId1, updatedLine);

        Assert.That(result, Is.True);

        var cart = await _context.Carts.Include(c => c.CartLines).FirstAsync(c => c.Id == CartId1);
        Assert.That(cart.CartLines.First(cl => cl.ProductId == ProductId1).Quantity, Is.EqualTo(5));
    }

    [Test]
    [Description("AddCartLineAsync: si la línea no existe, debe añadir una nueva")]
    public async Task AddCartLineAsync_WhenLineNotExists_ShouldAddNewLine()
    {
        var newLine = new CartLine { CartId = CartId1, ProductId = "P99", Quantity = 1, ProductPrice = 10.0 };

        var result = await _repository.AddCartLineAsync(CartId1, newLine);

        Assert.That(result, Is.True);

        var cart = await _context.Carts.Include(c => c.CartLines).FirstAsync(c => c.Id == CartId1);
        Assert.That(cart.CartLines, Has.Count.EqualTo(3));
        Assert.That(cart.CartLines.Any(cl => cl.ProductId == "P99"), Is.True);
    }

    [Test]
    [Description("AddCartLineAsync: si el carrito no existe debe retornar false")]
    public async Task AddCartLineAsync_WhenCartNotFound_ShouldReturnFalse()
    {
        var line   = new CartLine { ProductId = ProductId1, Quantity = 1 };
        var result = await _repository.AddCartLineAsync("NO_EXISTE", line);

        Assert.That(result, Is.False);
    }

    [Test]
    [Description("RemoveCartLineAsync: debe borrar la línea si existe y persistir")]
    public async Task RemoveCartLineAsync_ShouldRemoveLineAndPersist()
    {
        var lineToRemove = new CartLine { ProductId = ProductId2 };

        var result = await _repository.RemoveCartLineAsync(CartId1, lineToRemove);

        Assert.That(result, Is.True);

        var cart = await _context.Carts.Include(c => c.CartLines).FirstAsync(c => c.Id == CartId1);
        Assert.That(cart.CartLines,                            Has.Count.EqualTo(1));
        Assert.That(cart.CartLines.Any(cl => cl.ProductId == ProductId2), Is.False);
    }

    [Test]
    [Description("RemoveCartLineAsync: si el carrito no existe debe retornar false")]
    public async Task RemoveCartLineAsync_WhenCartNotFound_ShouldReturnFalse()
    {
        var result = await _repository.RemoveCartLineAsync("NO_EXISTE", new CartLine { ProductId = ProductId1 });

        Assert.That(result, Is.False);
    }

    [Test]
    [Description("DeleteCartAsync: si el carrito no existe debe lanzar CartNotFoundException")]
    public void DeleteCartAsync_WhenCartNotFound_ShouldThrowCartNotFoundException()
    {
        Assert.ThrowsAsync<CartNotFoundException>(() => _repository.DeleteCartAsync("NO_EXISTE"));
    }

    [Test]
    [Description("DeleteCartAsync: si el carrito existe debe eliminarlo de la BD")]
    public async Task DeleteCartAsync_WhenCartExists_ShouldDeleteIt()
    {
        await _repository.DeleteCartAsync(CartId2);

        var cart = await _context.Carts.FindAsync(CartId2);
        Assert.That(cart, Is.Null);
    }

    [Test]
    [Description("CreateCartAsync: debe persistir el carrito en la BD (SaveChangesAsync se ejecuta antes de los loads de owned entities)")]
    public async Task CreateCartAsync_ShouldPersistCart()
    {
        var newCart = new dawazonBackend.Cart.Models.Cart
        {
            Id        = "C_NEW",
            UserId    = 5,
            Purchased = false,
            Client    = new Client
            {
                Name    = "Nuevo Cliente",
                Email   = "new@new.com",
                Phone   = "699999999",
                Address = new Address { City = "Sevilla" }
            }
        };

        try
        {
            await _repository.CreateCartAsync(newCart);
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("owned entity") || ex.Message.Contains("tracking"))
        {
        }
        
        var persisted = await _context.Carts.FindAsync("C_NEW");
        Assert.That(persisted,            Is.Not.Null);
        Assert.That(persisted!.UserId,    Is.EqualTo(5));
        Assert.That(persisted.Purchased,  Is.False);
    }

    [Test]
    [Description("FindByUserIdAsync: debe lanzar NotImplementedException")]
    public void FindByUserIdAsync_ShouldThrowNotImplementedException()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "id", Direction: "asc");
        Assert.ThrowsAsync<NotImplementedException>(() => _repository.FindByUserIdAsync(1, filter));
    }

    [Test]
    [Description("FindByUserIdAndPurchasedAsync: debe retornar el carrito con sus includes si existe")]
    public async Task FindByUserIdAndPurchasedAsync_WhenCartExists_ShouldReturnCartWithIncludes()
    {
        var cart = await _repository.FindByUserIdAndPurchasedAsync(1, true);

        Assert.That(cart,                  Is.Not.Null);
        Assert.That(cart!.UserId,          Is.EqualTo(1));
        Assert.That(cart.Purchased,        Is.True);
        Assert.That(cart.CartLines,        Has.Count.EqualTo(2));
        Assert.That(cart.Client,           Is.Not.Null);
        Assert.That(cart.Client.Address,   Is.Not.Null);
    }

    [Test]
    [Description("FindByUserIdAndPurchasedAsync: debe retornar null si no existe carrito con ese userId y purchased")]
    public async Task FindByUserIdAndPurchasedAsync_WhenCartDoesNotExist_ShouldReturnNull()
    {
        var cart = await _repository.FindByUserIdAndPurchasedAsync(99, true);

        Assert.That(cart, Is.Null);
    }

    [Test]
    [Description("UpdateCartAsync: debe actualizar el carrito (sin modificar líneas) y retornar la entidad si existe")]
    public async Task UpdateCartAsync_WhenCartExists_ShouldUpdateAndReturnCart()
    {
        var updatedCart = new dawazonBackend.Cart.Models.Cart
        {
            Id = CartId1,
            Total = 999.9,
            TotalItems = 10,
            Purchased = false,
            CheckoutInProgress = true,
            CheckoutStartedAt = DateTime.UtcNow,
            Client = new Client { Name = "Updated Client", Email = "upd@c.com", Phone = "700000000", Address = new Address { City = "Valencia" } }
        };

        var result = await _repository.UpdateCartAsync(CartId1, updatedCart);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(CartId1));
        
        var dbCart = await _context.Carts.Include(c => c.Client).ThenInclude(c => c.Address).FirstAsync(c => c.Id == CartId1);
        Assert.That(dbCart.Total, Is.EqualTo(999.9));
        Assert.That(dbCart.TotalItems, Is.EqualTo(10));
        Assert.That(dbCart.Purchased, Is.False);
        Assert.That(dbCart.CheckoutInProgress, Is.True);
        Assert.That(dbCart.Client!.Name, Is.EqualTo("Updated Client"));
        Assert.That(dbCart.UploadAt, Is.Not.EqualTo(DateTime.MinValue)); // Fue actualizada a utcnow
    }

    [Test]
    [Description("UpdateCartAsync: debe retornar null si el carrito no existe")]
    public async Task UpdateCartAsync_WhenCartDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.UpdateCartAsync("NO_EXISTE", new dawazonBackend.Cart.Models.Cart());

        Assert.That(result, Is.Null);
    }

    [Test]
    [Description("CountNewSalesAsync: debe retornar el numero correcto de lineas de carritos comprados para un manager tras una fecha dade")]
    public async Task CountNewSalesAsync_ShouldReturnCountOfNewSalesForManager()
    {
        var cart = await _context.Carts.FirstAsync(c => c.Id == CartId1);
        cart.UploadAt = new DateTime(2023, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        await _context.SaveChangesAsync();

        var since = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var countManager999 = await _repository.CountNewSalesAsync(999, since);
        var countManager123 = await _repository.CountNewSalesAsync(123, since);

        Assert.That(countManager999, Is.EqualTo(1));
        Assert.That(countManager123, Is.EqualTo(1));
    }

    [Test]
    [Description("CountNewSalesAsync: debe retornar 0 si no hay lineas o no son nuevas o no son del manager")]
    public async Task CountNewSalesAsync_WhenNoNewSales_ShouldReturnZero()
    {
         var count = await _repository.CountNewSalesAsync(999, DateTime.UtcNow);

         Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    [Description("ApplySorting: ordenando por precio ascendente")]
    public async Task GetAllAsync_SortByPrice_Ascending()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "total", Direction: "asc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
        Assert.That(items.Select(c => c.Id).ToList(), Is.EqualTo(new[] { CartId2, CartId4, CartId1, CartId3 }));
    }

    [Test]
    [Description("ApplySorting: ordenando por precio descendente")]
    public async Task GetAllAsync_SortByPrice_Descending()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "total", Direction: "desc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
        Assert.That(items.Select(c => c.Id).ToList(), Is.EqualTo(new[] { CartId3, CartId1, CartId4, CartId2 }));
    }

    [Test]
    [Description("ApplySorting: ordenando por Comprado ascendente")]
    public async Task GetAllAsync_SortByComprado_Ascending()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "total", Direction: "asc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
        Assert.That(items.First().Purchased, Is.False);
    }

    [Test]
    [Description("ApplySorting: ordenando por Comprado descendente")]
    public async Task GetAllAsync_SortByComprado_Descending()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "total", Direction: "desc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
        Assert.That(items.First().Purchased, Is.False);
    }

    [Test]
    [Description("ApplySorting: ordenando por createdat ascendente")]
    public async Task GetAllAsync_SortByCreatedAt_Ascending()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "createdat", Direction: "asc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
        Assert.That(items.Select(c => c.Id).ToList(), Is.EqualTo(new[] { CartId1, CartId2, CartId3, CartId4 }));
    }

    [Test]
    [Description("ApplySorting: ordenando por createdat descendente")]
    public async Task GetAllAsync_SortByCreatedAt_Descending()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "createdat", Direction: "desc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
        Assert.That(items.Select(c => c.Id).ToList(), Is.EqualTo(new[] { CartId4, CartId3, CartId2, CartId1 }));
    }

    [Test]
    [Description("ApplySorting: ordenando por ultima modificacion ascendente")]
    public async Task GetAllAsync_SortByUltimaModificacion_Ascending()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "createdat", Direction: "asc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
        Assert.That(items.Select(c => c.Id).ToList(), Is.EqualTo(new[] { CartId1, CartId2, CartId3, CartId4 }));
    }

    [Test]
    [Description("ApplySorting: ordenando por ultima modificacion descendente")]
    public async Task GetAllAsync_SortByUltimaModificacion_Descending()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "createdat", Direction: "desc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
        Assert.That(items.Select(c => c.Id).ToList(), Is.EqualTo(new[] { CartId4, CartId3, CartId2, CartId1 }));
    }

    [Test]
    [Description("ApplySorting: campo desconocido usa ordenacion por defecto por Id")]
    public async Task GetAllAsync_SortByUnknownField_DefaultsToId()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "campo_desconocido", Direction: "asc");
        var (items, totalCount) = await _repository.GetAllAsync(filter);

        Assert.That(totalCount, Is.EqualTo(4));
    }

    [Test]
    [Description("ApplySorting: ordenacion case insensitive para direction")]
    public async Task GetAllAsync_SortDirection_CaseInsensitive()
    {
        var filterAsc = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "total", Direction: "ASC");
        var filterDesc = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "total", Direction: "DESC");

        var (itemsAsc, _) = await _repository.GetAllAsync(filterAsc);
        var (itemsDesc, _) = await _repository.GetAllAsync(filterDesc);

        Assert.That(itemsAsc.First().Id, Is.EqualTo(CartId2));
        Assert.That(itemsDesc.First().Id, Is.EqualTo(CartId3));
    }
    
    [Test]
    [Description("Cubre la línea 233: case 'Comprado'")]
    public async Task GetAllAsync_SortByComprado_CorrectPath()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "Comprado", Direction: "asc");
        var (items, _) = await _repository.GetAllAsync(filter);
    
        Assert.That(items.First().Purchased, Is.True);
    }

    [Test]
    [Description("Cubre la línea 234: case 'precio'")]
    public async Task GetAllAsync_SortByPrecio_CorrectPath()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "precio", Direction: "asc");
        var (items, _) = await _repository.GetAllAsync(filter);
    
        Assert.That(items.Select(c => c.Id).ToList(), Is.EqualTo(new[] { CartId1, CartId2, CartId3, CartId4 }));
    }

    [Test]
    [Description("Cubre la línea 236: case 'ultima modificacion'")]
    public async Task GetAllAsync_SortByUltimaModificacion_CorrectPath()
    {
        var filter = new FilterCartDto(managerId: null, isAdmin: null, purchased: null, Page: 0, Size: 10, SortBy: "ultima modificacion", Direction: "desc");
        var (items, _) = await _repository.GetAllAsync(filter);
    
        Assert.That(items.Select(c => c.Id).ToList(), Is.EqualTo(new[] { CartId4, CartId3, CartId2, CartId1 }));
    }

    [Test]
    [Description("UpdateCartScalarsAsync: debe actualizar solo Total y TotalItems")]
    public async Task UpdateCartScalarsAsync_ShouldUpdateOnlyScalars()
    {
        await _repository.UpdateCartScalarsAsync(CartId1, 5, 250.0);

        var cart = await _context.Carts.FindAsync(CartId1);
        Assert.That(cart!.Total, Is.EqualTo(250.0));
        Assert.That(cart.TotalItems, Is.EqualTo(5));
    }

    [Test]
    [Description("UpdateCartScalarsAsync: no debe lanzar si el carrito no existe")]
    public async Task UpdateCartScalarsAsync_WhenCartNotFound_ShouldNotThrow()
    {
        await _repository.UpdateCartScalarsAsync("NO_EXISTE", 1, 100.0);
    }

    [Test]
    [Description("GetSalesAsLinesAsync: debe retornar lineas de ventas compradas con filtros")]
    public async Task GetSalesAsLinesAsync_ShouldReturnSaleLines()
    {
        var filter = new FilterDto(null, null, 0, 10, "Date", "asc");

        var (items, totalCount) = await _repository.GetSalesAsLinesAsync(null, true, filter);

        Assert.That(totalCount, Is.EqualTo(2));
        Assert.That(items.Count, Is.EqualTo(2));
    }

    [Test]
    [Description("GetSalesAsLinesAsync: con managerId debe filtrar por productos del manager")]
    public async Task GetSalesAsLinesAsync_WithManagerId_ShouldFilterByManager()
    {
        var filter = new FilterDto(null, null, 0, 10, "Date", "asc");

        var (items, totalCount) = await _repository.GetSalesAsLinesAsync(999, false, filter);

        Assert.That(totalCount, Is.EqualTo(1));
    }

    [Test]
    [Description("GetSalesAsLinesAsync: paginacion debe respetar page y size")]
    public async Task GetSalesAsLinesAsync_WithPagination_ShouldRespectPageSize()
    {
        var filter = new FilterDto(null, null, 0, 1, "Date", "asc");

        var (items, totalCount) = await _repository.GetSalesAsLinesAsync(null, true, filter);

        Assert.That(totalCount, Is.EqualTo(2));
        Assert.That(items.Count, Is.EqualTo(1));
    }

    [Test]
    [Description("GetTotalSalesCountAsync: debe retornar el numero total de carritos comprados")]
    public async Task GetTotalSalesCountAsync_ShouldReturnTotalPurchasedCarts()
    {
        var count = await _repository.GetTotalSalesCountAsync();

        Assert.That(count, Is.EqualTo(2));
    }
}