using dawazonBackend.Common.Database;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Repository.Productos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.Products.Repository;

[TestFixture]
[Description("ProductRepository Tests")]
public class ProductRepositoryTest
{
    private DawazonDbContext _context = null!;
    private Mock<ILogger<ProductRepository>> _loggerMock = null!;
    private ProductRepository _repository = null!;

    private const string CatId1 = "CAT001";
    private const string CatId2 = "CAT002";

    private const string ProdId1 = "PRD001";
    private const string ProdId2 = "PRD002";
    private const string ProdId3 = "PRD003";

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DawazonDbContext>()
            .UseInMemoryDatabase($"product_repo_test_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context    = new DawazonDbContext(options);
        _loggerMock = new Mock<ILogger<ProductRepository>>();

        SeedDatabase();

        _repository = new ProductRepository(_loggerMock.Object, _context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedDatabase()
    {
        var cat1 = new Category { Id = CatId1, Name = "Electrónica" };
        var cat2 = new Category { Id = CatId2, Name = "Ropa" };
        _context.Categorias.AddRange(cat1, cat2);

        _context.Products.AddRange(
            new Product
            {
                Id          = ProdId1,
                Name        = "Producto Alfa",
                Price       = 10.0,
                Stock       = 50,
                Description = "Desc1",
                CategoryId  = CatId1,
                Category    = cat1,
                CreatorId   = 1,
                IsDeleted   = false,
                CreatedAt   = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt   = DateTime.UtcNow,
                Version     = 1
            },
            new Product
            {
                Id          = ProdId2,
                Name        = "Producto Beta",
                Price       = 25.0,
                Stock       = 5,
                Description = "Desc2",
                CategoryId  = CatId2,
                Category    = cat2,
                CreatorId   = 2,
                IsDeleted   = false,
                CreatedAt   = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt   = DateTime.UtcNow,
                Version     = 1
            },
            new Product
            {
                Id          = ProdId3,
                Name        = "Producto Borrado",
                Price       = 5.0,
                Stock       = 0,
                Description = "Desc3",
                CategoryId  = CatId1,
                Category    = cat1,
                CreatorId   = 1,
                IsDeleted   = true,
                CreatedAt   = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt   = DateTime.UtcNow,
                Version     = 1
            }
        );
        _context.SaveChanges();
    }

    [Test]
    [Description("GetAllAsync: Debe retornar solo productos no eliminados")]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedProducts()
    {
        var filter = new FilterDto(null, null);
        var (items, count) = await _repository.GetAllAsync(filter);

        Assert.That(count, Is.EqualTo(2));
        Assert.That(items.Any(p => p.Id == ProdId3), Is.False);
    }

    [Test]
    [Description("GetAllAsync: Filtrar por nombre debe retornar solo los que contienen el texto")]
    public async Task GetAllAsync_WithNameFilter_ShouldFilterByName()
    {
        var filter = new FilterDto(Nombre: "Alfa", null);
        var (items, count) = await _repository.GetAllAsync(filter);

        Assert.That(count, Is.EqualTo(1));
        Assert.That(items.First().Id, Is.EqualTo(ProdId1));
    }

    [Test]
    [Description("GetAllAsync: Filtrar por categoría debe retornar solo los de esa categoría")]
    public async Task GetAllAsync_WithCategoryFilter_ShouldFilterByCategory()
    {
        var filter = new FilterDto(null, Categoria: "Ropa");
        var (items, count) = await _repository.GetAllAsync(filter);

        Assert.That(count, Is.EqualTo(1));
        Assert.That(items.First().Id, Is.EqualTo(ProdId2));
    }

    [Test]
    [Description("GetAllAsync: La paginación debe respetar Page y Size")]
    public async Task GetAllAsync_WithPagination_ShouldRespectPageAndSize()
    {
        var filter = new FilterDto(null, null, Page: 0, Size: 1);
        var (items, count) = await _repository.GetAllAsync(filter);

        Assert.That(count,        Is.EqualTo(2));
        Assert.That(items.Count(), Is.EqualTo(1));
    }

    [Test]
    [Description("GetAllAsync: Ordenar por precio ascendente")]
    public async Task GetAllAsync_SortByPriceAsc_ShouldOrderCorrectly()
    {
        var filter = new FilterDto(null, null, SortBy: "price", Direction: "asc");
        var (items, _) = await _repository.GetAllAsync(filter);

        var list = items.ToList();
        Assert.That(list[0].Id, Is.EqualTo(ProdId1));
    }

    [Test]
    [Description("GetAllAsync: Ordenar por precio descendente")]
    public async Task GetAllAsync_SortByPriceDesc_ShouldOrderCorrectly()
    {
        var filter = new FilterDto(null, null, SortBy: "precio", Direction: "desc");
        var (items, _) = await _repository.GetAllAsync(filter);

        var list = items.ToList();
        Assert.That(list[0].Id, Is.EqualTo(ProdId2));
    }

    [Test]
    [Description("GetAllAsync: Ordenar por nombre (nombre/name)")]
    public async Task GetAllAsync_SortByNombre_ShouldOrderByName()
    {
        var filter = new FilterDto(null, null, SortBy: "nombre", Direction: "asc");
        var (items, _) = await _repository.GetAllAsync(filter);
        var list = items.ToList();

        Assert.That(list[0].Name, Is.EqualTo("Producto Alfa"));
    }

    [Test]
    [Description("GetAllAsync: Ordenar por stock")]
    public async Task GetAllAsync_SortByStock_ShouldOrderByStock()
    {
        var filter = new FilterDto(null, null, SortBy: "stock", Direction: "asc");
        var (items, _) = await _repository.GetAllAsync(filter);
        var list = items.ToList();

        Assert.That(list[0].Id, Is.EqualTo(ProdId2));
    }

    [Test]
    [Description("GetAllAsync: SortBy desconocido debe ordenar por Id por defecto")]
    public async Task GetAllAsync_SortByUnknown_ShouldDefaultToId()
    {
        var filter = new FilterDto(null, null, SortBy: "desconocido", Direction: "asc");
        var (items, _) = await _repository.GetAllAsync(filter);

        Assert.That(items, Is.Not.Empty);
    }

    [Test]
    [Description("GetAllByCreatedAtBetweenAsync: Debe devolver productos creados en el rango de fechas")]
    public async Task GetAllByCreatedAtBetweenAsync_ShouldReturnProductsInRange()
    {
        var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end   = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc);

        var result = await _repository.GetAllByCreatedAtBetweenAsync(start, end);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(ProdId1));
    }

    [Test]
    [Description("GetAllByCreatedAtBetweenAsync: No debe incluir productos eliminados en el rango")]
    public async Task GetAllByCreatedAtBetweenAsync_ShouldExcludeDeletedProducts()
    {
        var start = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc);
        var end   = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = await _repository.GetAllByCreatedAtBetweenAsync(start, end);

        Assert.That(result, Is.Empty);
    }

    [Test]
    [Description("GetProductAsync: Debe retornar el producto con includes si existe")]
    public async Task GetProductAsync_WhenExists_ShouldReturnProductWithIncludes()
    {
        var result = await _repository.GetProductAsync(ProdId1);

        Assert.That(result,           Is.Not.Null);
        Assert.That(result!.Id,       Is.EqualTo(ProdId1));
        Assert.That(result.Category,  Is.Not.Null);
    }

    [Test]
    [Description("GetProductAsync: No debe retornar productos eliminados")]
    public async Task GetProductAsync_WhenDeleted_ShouldReturnNull()
    {
        var result = await _repository.GetProductAsync(ProdId3);

        Assert.That(result, Is.Null);
    }

    [Test]
    [Description("GetProductAsync: Debe retornar null si no existe")]
    public async Task GetProductAsync_WhenNotFound_ShouldReturnNull()
    {
        var result = await _repository.GetProductAsync("NO_EXISTE");

        Assert.That(result, Is.Null);
    }

    [Test]
    [Description("FindAllByCreatorId: Debe retornar solo productos del creador indicado")]
    public async Task FindAllByCreatorId_ShouldReturnOnlyCreatorsProducts()
    {
        var filter = new FilterDto(null, null);
        var (items, count) = await _repository.FindAllByCreatorId(1, filter);

        Assert.That(count, Is.EqualTo(1));
        Assert.That(items.First().Id, Is.EqualTo(ProdId1));
    }

    [Test]
    [Description("FindAllByCreatorId: No debe incluir productos eliminados del creador")]
    public async Task FindAllByCreatorId_ShouldExcludeDeletedProducts()
    {
        var filter = new FilterDto(null, null);
        var (items, _) = await _repository.FindAllByCreatorId(1, filter);

        Assert.That(items.Any(p => p.IsDeleted), Is.False);
    }

    [Test]
    [Description("FindAllByCreatorId: Debe retornar vacío si el creador no tiene productos")]
    public async Task FindAllByCreatorId_WhenNoProducts_ShouldReturnEmpty()
    {
        var filter = new FilterDto(null, null);
        var (items, count) = await _repository.FindAllByCreatorId(999, filter);

        Assert.That(count, Is.EqualTo(0));
        Assert.That(items, Is.Empty);
    }

    [Test]
    [Description("CreateProductAsync: Debe persistir el producto y retornarlo")]
    public async Task CreateProductAsync_ShouldPersistAndReturnProduct()
    {
        var newProduct = new Product
        {
            Id          = "PRD_NEW",
            Name        = "Nuevo",
            Price       = 9.99,
            Stock       = 10,
            Description = "Nuevo desc",
            CategoryId  = CatId1,
            CreatorId   = 3,
            IsDeleted   = false
        };

        var result = await _repository.CreateProductAsync(newProduct);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id,   Is.EqualTo("PRD_NEW"));
        Assert.That(result.Name,  Is.EqualTo("Nuevo"));

        var persisted = await _context.Products.FindAsync("PRD_NEW");
        Assert.That(persisted, Is.Not.Null);
    }

    [Test]
    [Description("UpdateProductAsync: Debe actualizar los campos del producto y retornarlo")]
    public async Task UpdateProductAsync_WhenExists_ShouldUpdateAndReturn()
    {
        var updated = new Product
        {
            Name        = "Nombre Actualizado",
            Price       = 99.99,
            Stock       = 100,
            Description = "Desc actualizada",
            CategoryId  = CatId2,
            Category    = _context.Categorias.Find(CatId2),
            Comments    = [],
            Images      = []
        };

        var result = await _repository.UpdateProductAsync(updated, ProdId1);

        Assert.That(result,              Is.Not.Null);
        Assert.That(result!.Name,        Is.EqualTo("Nombre Actualizado"));
        Assert.That(result.Price,        Is.EqualTo(99.99));
        Assert.That(result.Stock,        Is.EqualTo(100));
        Assert.That(result.Description,  Is.EqualTo("Desc actualizada"));
    }

    [Test]
    [Description("UpdateProductAsync: Debe retornar null si el producto no existe")]
    public async Task UpdateProductAsync_WhenNotFound_ShouldReturnNull()
    {
        var updated = new Product { Name = "Test", CategoryId = CatId1, Comments = [], Images = [] };

        var result = await _repository.UpdateProductAsync(updated, "NO_EXISTE");

        Assert.That(result, Is.Null);
    }

    [Test]
    [Description("UpdateProductAsync: No debe actualizar si el producto está eliminado")]
    public async Task UpdateProductAsync_WhenDeleted_ShouldReturnNull()
    {
        var updated = new Product { Name = "Intento", CategoryId = CatId1, Comments = [], Images = [] };

        var result = await _repository.UpdateProductAsync(updated, ProdId3);

        Assert.That(result, Is.Null);
    }

    [Test]
    [Description("UpdateProductAsync: Si se pasan imágenes nuevas debe reemplazarlas")]
    public async Task UpdateProductAsync_WithImages_ShouldUpdateImages()
    {
        var updated = new Product
        {
            Name        = "Alfa",
            Price       = 10.0,
            Stock       = 50,
            Description = "Desc1",
            CategoryId  = CatId1,
            Comments    = [],
            Images      = ["nueva1.jpg", "nueva2.jpg"]
        };

        var result = await _repository.UpdateProductAsync(updated, ProdId1);

        Assert.That(result,       Is.Not.Null);
        Assert.That(result!.Images, Has.Count.EqualTo(2));
    }

    [Test]
    [Description("DeleteByIdAsync: Debe marcar el producto como IsDeleted=true (borrado lógico)")]
    public async Task DeleteByIdAsync_ShouldMarkProductAsDeleted()
    {
        await _repository.DeleteByIdAsync(ProdId1);

        var product = await _context.Products.FindAsync(ProdId1);
        Assert.That(product,            Is.Not.Null);
        Assert.That(product!.IsDeleted, Is.True);
    }

    [Test]
    [Description("DeleteByIdAsync: Debe lanzar excepción si el producto no existe")]
    public void DeleteByIdAsync_WhenNotFound_ShouldThrowException()
    {
        Assert.ThrowsAsync<Exception>(() => _repository.DeleteByIdAsync("NO_EXISTE"));
    }

    [Test]
    [Description("DeleteByIdAsync: Debe lanzar excepción si el producto ya está borrado")]
    public void DeleteByIdAsync_WhenAlreadyDeleted_ShouldThrowException()
    {
        Assert.ThrowsAsync<Exception>(() => _repository.DeleteByIdAsync(ProdId3));
    }

    [Test]
    [Description("SubstractStockAsync: Debe restar el stock y retornar 1 si versión coincide")]
    public async Task SubstractStockAsync_WhenVersionMatches_ShouldDecrementStockAndReturnOne()
    {
        var result = await _repository.SubstractStockAsync(ProdId1, 5, version: 1);

        Assert.That(result, Is.EqualTo(1));

        var product = await _context.Products.FindAsync(ProdId1);
        Assert.That(product!.Stock, Is.EqualTo(45)); // 50 - 5
    }

    [Test]
    [Description("SubstractStockAsync: Debe retornar 0 si versión no coincide")]
    public async Task SubstractStockAsync_WhenVersionMismatch_ShouldReturnZero()
    {
        var result = await _repository.SubstractStockAsync(ProdId1, 5, version: 999);

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    [Description("SubstractStockAsync: Debe retornar 0 si el producto no existe")]
    public async Task SubstractStockAsync_WhenProductNotFound_ShouldReturnZero()
    {
        var result = await _repository.SubstractStockAsync("NO_EXISTE", 1, version: 1);

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    [Description("SubstractStockAsync: Debe lanzar excepción si el stock resultante sería negativo")]
    public void SubstractStockAsync_WhenStockGoesNegative_ShouldThrowException()
    {
        Assert.ThrowsAsync<Exception>(() =>
            _repository.SubstractStockAsync(ProdId2, 10, version: 1));
    }
}
