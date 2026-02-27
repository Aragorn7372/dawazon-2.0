using dawazonBackend.Common.Database;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Repository.Categoria;
using dawazonBackend.Products.Repository.Productos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace dawazonTest.Container.Productos;

[TestFixture]
[Description("Integration Tests for Product/Category Repositories using PostgreSQL Testcontainers")]
public class ProductsRepositoryTestContainer
{
    private PostgreSqlContainer _dbContainer;
    private DawazonDbContext _dbContext;
    private IProductRepository _productRepository;
    private ICategoriaRepository _categoryRepository;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("dawazondb_test")
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

        _productRepository = new ProductRepository(new NullLogger<ProductRepository>(), _dbContext);
        _categoryRepository = new CategoryRepository(new NullLogger<CategoryRepository>(), _dbContext);
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
    [Description("GetCategoriesAsync: Debe retornar los nombres de las categorías sembradas (SeedData)")]
    public async Task CategoryRepository_GetCategoriesAsync_ShouldReturnSeededNames()
    {
        var result = await _categoryRepository.GetCategoriesAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("Figuras"));
        Assert.That(result, Does.Contain("Comics"));
        Assert.That(result, Does.Contain("Ropa"));
    }

    [Test]
    [Description("GetAllAsync: Debe retornar la página solicitada de productos no borrados con sus categorías")]
    public async Task ProductRepository_GetAllAsync_ShouldReturnPagedProducts()
    {
        var filter = new FilterDto(Nombre: null, Categoria: null, Page: 0, Size: 5, SortBy: "id", Direction: "asc");
        var (items, count) = await _productRepository.GetAllAsync(filter);

        Assert.That(count, Is.GreaterThan(0));
        Assert.That(items.Count(), Is.LessThanOrEqualTo(5));
        
        var first = items.First();
        Assert.That(first.Category, Is.Not.Null); 
    }

    [Test]
    [Description("CreateProductAsync: Debe persistir un producto en la BD PostgreSQL y recuperarlo con la FK resuelta")]
    public async Task ProductRepository_CreateProductAsync_ShouldPersistAndRetrieve()
    {
        var newProduct = new Product
        {
            Id = "PRD999999999",
            Name = "Container Test Product",
            Price = 42.0,
            Stock = 10,
            Description = "Integration test description",
            CreatorId = 1L,
            CategoryId = "FIG000000001", 
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1L
        };

        var created = await _productRepository.CreateProductAsync(newProduct);
        
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Id, Is.EqualTo("PRD999999999"));
        Assert.That(created.Category, Is.Not.Null);
        Assert.That(created.Category!.Name, Is.EqualTo("Figuras"));

        _dbContext.ChangeTracker.Clear();
        var retrieved = await _productRepository.GetProductAsync("PRD999999999");
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Name, Is.EqualTo("Container Test Product"));
    }

    [Test]
    [Description("SubstractStockAsync: Concurrencia optimista real en PostgreSQL")]
    public async Task ProductRepository_SubstractStockAsync_ConcurrencyCheck()
    {
        var productId = "PRD000000002";
        
        var resultSuccess = await _productRepository.SubstractStockAsync(productId, 5, 1L);
        Assert.That(resultSuccess, Is.EqualTo(1));

        _dbContext.ChangeTracker.Clear();
        var updatedProduct = await _productRepository.GetProductAsync(productId);
        Assert.That(updatedProduct!.Stock, Is.EqualTo(15));
        
        var resultFail = await _productRepository.SubstractStockAsync(productId, 5, 999L);
        Assert.That(resultFail, Is.EqualTo(0)); 
    }

    [Test]
    [Description("DeleteByIdAsync: Borrado lógico real en BBDD PostgreSQL")]
    public async Task ProductRepository_DeleteByIdAsync_ShouldLogicalDelete()
    {
        var productId = "PRD000000003"; 
        await _productRepository.DeleteByIdAsync(productId);

        _dbContext.ChangeTracker.Clear();
        var deleted = await _productRepository.GetProductAsync(productId);
        Assert.That(deleted, Is.Null);

        var realExistence = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
        Assert.That(realExistence, Is.Not.Null);
        Assert.That(realExistence!.IsDeleted, Is.True);
    }
}