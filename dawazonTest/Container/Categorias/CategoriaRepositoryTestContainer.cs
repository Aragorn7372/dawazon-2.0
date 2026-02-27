using dawazonBackend.Common.Database;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Repository.Categoria;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace dawazonTest.Container.Categorias;

[TestFixture]
[Description("Integration Tests for CategoryRepository using PostgreSQL Testcontainers")]
public class CategoriaRepositoryTestContainer
{
    private PostgreSqlContainer _dbContainer;
    private DawazonDbContext _dbContext;
    private ICategoriaRepository _categoryRepository;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("dawazondb_test_categories")
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
    [Description("GetCategoriesAsync: Should return all category names")]
    public async Task GetCategoriesAsync_ShouldReturnListOfCategoryNames()
    {
        var categoryNames = await _categoryRepository.GetCategoriesAsync();

        Assert.That(categoryNames, Is.Not.Null);
        Assert.That(categoryNames.Count, Is.EqualTo(3));
        Assert.That(categoryNames, Contains.Item("Figuras"));
        Assert.That(categoryNames, Contains.Item("Comics"));
        Assert.That(categoryNames, Contains.Item("Ropa"));
    }

    [Test]
    [Description("GetCategoryAsync: Should return a category by its ID")]
    public async Task GetCategoryAsync_ShouldReturnCategoryById()
    {
        var category = await _categoryRepository.GetCategoryAsync("COM000000001");

        Assert.That(category, Is.Not.Null);
        Assert.That(category.Id, Is.EqualTo("COM000000001"));
        Assert.That(category.Name, Is.EqualTo("Comics"));
    }

    [Test]
    [Description("GetCategoryAsync: Should return null when ID does not exist")]
    public async Task GetCategoryAsync_ShouldReturnNullWhenIdDoesNotExist()
    {
        var category = await _categoryRepository.GetCategoryAsync("CAT99999999");

        Assert.That(category, Is.Null);
    }

    [Test]
    [Description("GetByNameAsync: Should return a category by its name, ignoring case")]
    public async Task GetByNameAsync_ShouldReturnCategoryByNameCaseInsensitive()
    {
        var category = await _categoryRepository.GetByNameAsync("fIgUrAs");

        Assert.That(category, Is.Not.Null);
        Assert.That(category.Id, Is.EqualTo("FIG000000001"));
        Assert.That(category.Name, Is.EqualTo("Figuras"));
    }

    [Test]
    [Description("GetByNameAsync: Should return null when name does not exist")]
    public async Task GetByNameAsync_ShouldReturnNullWhenNameDoesNotExist()
    {
        var category = await _categoryRepository.GetByNameAsync("NonExistingCategory");

        Assert.That(category, Is.Null);
    }
}
