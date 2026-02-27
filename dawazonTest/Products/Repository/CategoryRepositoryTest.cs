using dawazonBackend.Common.Database;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Repository.Categoria;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.Products.Repository;

[TestFixture]
[Description("CategoryRepository Tests")]
public class CategoryRepositoryTest
{
    private DawazonDbContext _context = null!;
    private Mock<ILogger<CategoryRepository>> _loggerMock = null!;
    private CategoryRepository _repository = null!;

    private const string CatId1   = "CAT001";
    private const string CatId2   = "CAT002";
    private const string CatId3   = "CAT003";
    private const string CatName1 = "Electrónica";
    private const string CatName2 = "Ropa";
    private const string CatName3 = "Hogar";

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DawazonDbContext>()
            .UseInMemoryDatabase($"category_test_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context    = new DawazonDbContext(options);
        _loggerMock = new Mock<ILogger<CategoryRepository>>();

        SeedDatabase();

        _repository = new CategoryRepository(_loggerMock.Object, _context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedDatabase()
    {
        _context.Categorias.AddRange(
            new Category { Id = CatId1, Name = CatName1 },
            new Category { Id = CatId2, Name = CatName2 },
            new Category { Id = CatId3, Name = CatName3 }
        );
        _context.SaveChanges();
    }

    [Test]
    [Description("GetCategoriesAsync: Debe retornar todos los nombres de categorías")]
    public async Task GetCategoriesAsync_ShouldReturnAllCategoryNames()
    {
        var result = await _repository.GetCategoriesAsync();

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result, Contains.Item(CatName1));
        Assert.That(result, Contains.Item(CatName2));
        Assert.That(result, Contains.Item(CatName3));
    }

    [Test]
    [Description("GetCategoriesAsync: Debe retornar lista vacía si no hay categorías")]
    public async Task GetCategoriesAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        _context.Categorias.RemoveRange(_context.Categorias);
        await _context.SaveChangesAsync();

        var result = await _repository.GetCategoriesAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    [Description("GetCategoryAsync: Debe retornar la categoría con el Id correcto")]
    public async Task GetCategoryAsync_WhenExists_ShouldReturnCategory()
    {
        var result = await _repository.GetCategoryAsync(CatId1);

        Assert.That(result,       Is.Not.Null);
        Assert.That(result!.Id,   Is.EqualTo(CatId1));
        Assert.That(result.Name,  Is.EqualTo(CatName1));
    }

    [Test]
    [Description("GetCategoryAsync: Debe retornar null si el Id no existe")]
    public async Task GetCategoryAsync_WhenNotFound_ShouldReturnNull()
    {
        var result = await _repository.GetCategoryAsync("NO_EXISTE");

        Assert.That(result, Is.Null);
    }

    [Test]
    [Description("GetByNameAsync: Debe retornar la categoría con el nombre exacto")]
    public async Task GetByNameAsync_WhenExactName_ShouldReturnCategory()
    {
        var result = await _repository.GetByNameAsync(CatName2);

        Assert.That(result,       Is.Not.Null);
        Assert.That(result!.Id,   Is.EqualTo(CatId2));
        Assert.That(result.Name,  Is.EqualTo(CatName2));
    }

    [Test]
    [Description("GetByNameAsync: La búsqueda debe ser case-insensitive")]
    public async Task GetByNameAsync_CaseInsensitive_ShouldReturnCategory()
    {
        var result = await _repository.GetByNameAsync("electrónica");

        Assert.That(result,       Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo(CatName1));
    }

    [Test]
    [Description("GetByNameAsync: Debe retornar null si el nombre no existe")]
    public async Task GetByNameAsync_WhenNotFound_ShouldReturnNull()
    {
        var result = await _repository.GetByNameAsync("Inexistente");

        Assert.That(result, Is.Null);
    }

    [Test]
    [Description("GetByNameAsync: Búsqueda con nombre en mayúsculas debe funcionar")]
    public async Task GetByNameAsync_UpperCase_ShouldReturnCategory()
    {
        var result = await _repository.GetByNameAsync("ROPA");

        Assert.That(result,       Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo(CatName2));
    }
}
