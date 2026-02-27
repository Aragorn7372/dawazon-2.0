using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Models;
using dawazonBackend.Cart.Repository;
using dawazonBackend.Common.Database;
using dawazonBackend.Common.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace dawazonTest.Container.Cart;

[TestFixture]
[Description("Pruebas de integración para CartRepository utilizando contenedores de prueba PostgreSQL")]
public class CartRepositoryTestContainer
{
    private PostgreSqlContainer _dbContainer;
    private DawazonDbContext _dbContext;
    private ICartRepository _cartRepository;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("dawazondb_test_cart")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();

        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DawazonDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _dbContext = new DawazonDbContext(options);

        await _dbContext.Database.EnsureCreatedAsync();

        _dbContext.Users.AddRange(
            new dawazonBackend.Users.Models.User 
            { 
                Id = 1L, 
                UserName = "manager1", 
                Email = "m1@test.com", 
                Name = "Manager 1"
            },
            new dawazonBackend.Users.Models.User 
            { 
                Id = 3L, 
                UserName = "manager3", 
                Email = "m3@test.com", 
                Name = "Manager 3"
            }
        );
        await _dbContext.SaveChangesAsync();

        _cartRepository = new CartRepository(_dbContext, new NullLogger<CartRepository>());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _dbContext.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    [SetUp]
    public void SetUp()
    {
        _dbContext.ChangeTracker.Clear();
    }

    [Test]
    [Description("GetAllAsync: Debe retornar carritos filtrados con sus relaciones")]
    public async Task GetAllAsync_ShouldReturnPagedAndFilteredCarts()
    {
        var filter = new FilterCartDto(
            managerId: null, 
            isAdmin: null, 
            purchased: true, 
            Page: 0, 
            Size: 10, 
            SortBy: "id", 
            Direction: "asc"
        );

        var (items, count) = await _cartRepository.GetAllAsync(filter);
        var itemsList = items.ToList();

        Assert.That(count, Is.EqualTo(1));
        Assert.That(itemsList.Count, Is.EqualTo(1));

        var cart = itemsList.First();
        Assert.That(cart.Id, Is.EqualTo("CART00000001"));
        Assert.That(cart.Purchased, Is.True);
        Assert.That(cart.CartLines, Is.Not.Empty);
        Assert.That(cart.CartLines.First().Product, Is.Not.Null); 
    }

    [Test]
    [Description("CalculateTotalEarningsAsync: Suma de Price * Quantity en la BBDD")]
    public async Task CalculateTotalEarningsAsync_ShouldReturnSumOfPurchasedLines()
    {
        var totalAdmin = await _cartRepository.CalculateTotalEarningsAsync(managerId: null, isAdmin: true);
        Assert.That(totalAdmin, Is.EqualTo(25.98).Within(0.01));

        var totalManager = await _cartRepository.CalculateTotalEarningsAsync(managerId: 3L, isAdmin: false);
        Assert.That(totalManager, Is.EqualTo(25.98).Within(0.01));

        var totalOtherManager = await _cartRepository.CalculateTotalEarningsAsync(managerId: 1L, isAdmin: false);
        Assert.That(totalOtherManager, Is.EqualTo(0));
    }

    [Test]
    [Description("UpdateCartLineStatusAsync: Debe actualizar el estado de una línea persistida como String en PostgreSQL")]
    public async Task UpdateCartLineStatusAsync_ShouldUpdateEnumStoredAsString()
    {
        var cartId = "CART00000001";
        var productId = "PRD000000001"; 

        var result = await _cartRepository.UpdateCartLineStatusAsync(cartId, productId, Status.Enviado);
        Assert.That(result, Is.True);

        _dbContext.ChangeTracker.Clear();
        var cart = await _cartRepository.FindCartByIdAsync(cartId);
        
        var line = cart!.CartLines.First(l => l.ProductId == productId);
        Assert.That(line.Status, Is.EqualTo(Status.Enviado));
        
        await _cartRepository.UpdateCartLineStatusAsync(cartId, productId, Status.Preparado);
    }

    [Test]
    [Description("GetSalesAsLinesAsync: Debe retornar las líneas de venta correctamente mapeadas mediante un JOIN")]
    public async Task GetSalesAsLinesAsync_ShouldReturnFlattenedSaleLinesWithCreatorInfo()
    {
        var filter = new FilterDto(Nombre: null, Categoria: null, Page: 0, Size: 10, SortBy: "id", Direction: "asc");
        
        var (items, count) = await _cartRepository.GetSalesAsLinesAsync(managerId: null, isAdmin: true, filter);

        Assert.That(count, Is.EqualTo(2)); 
        Assert.That(items.Count, Is.EqualTo(2));

        var saleLine = items.First(i => i.ProductId == "PRD000000001");
        Assert.That(saleLine.SaleId, Is.EqualTo("CART00000001"));
        Assert.That(saleLine.ManagerId, Is.EqualTo(3L)); 
        Assert.That(saleLine.ProductName, Is.EqualTo("Funko Pop Iron Man"));
        Assert.That(saleLine.Status, Is.EqualTo(Status.Preparado));
    }
}