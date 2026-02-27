using dawazonBackend.Common.Error;
using dawazonBackend.Products.Mapper;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Products.Repository.Productos;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service.Favs;
using dawazonBackend.Common.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace dawazonTest.Users.Service.Favs;

[TestFixture]
[Description("Tests para FavService")]
public class FavServiceTest
{
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<IProductRepository> _productRepoMock;
    private FavService _favService;

    [SetUp]
    public void SetUp()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        _productRepoMock = new Mock<IProductRepository>();

        _favService = new FavService(new NullLogger<FavService>(), _userManagerMock.Object, _productRepoMock.Object);
    }

    private User BuildUser(long id, List<string> favs)
    {
        return new User
        {
            Id = id,
            Name = "John Doe",
            ProductsFavs = favs ?? new List<string>()
        };
    }

    [Test]
    public async Task AddFav_WhenSuccess_ShouldReturnTrue()
    {
        var user = BuildUser(1L, new List<string>());
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _favService.AddFav("PRD01", 1L);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(user.ProductsFavs.Contains("PRD01"), Is.True);
    }

    [Test]
    public async Task AddFav_WhenUserNotFound_ShouldReturnUserNotFoundError()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((User)null!);

        var result = await _favService.AddFav("PRD01", 1L);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserNotFoundError>());
    }

    [Test]
    public async Task AddFav_WhenAlreadyFaved_ShouldReturnUserHasThatProductError()
    {
        var user = BuildUser(1L, new List<string> { "PRD01" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var result = await _favService.AddFav("PRD01", 1L);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserHasThatProductError>());
    }

    [Test]
    public async Task RemoveFav_WhenSuccess_ShouldReturnTrue()
    {
        var user = BuildUser(1L, new List<string> { "PRD01" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _favService.RemoveFav("PRD01", 1L);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(user.ProductsFavs.Contains("PRD01"), Is.False);
    }

    [Test]
    public async Task RemoveFav_WhenNotFaved_ShouldReturnError()
    {
        var user = BuildUser(1L, new List<string>()); // No tiene el producto guardado
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var result = await _favService.RemoveFav("PRD01", 1L);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserHasThatProductError>());
    }

    [Test]
    public async Task GetFavs_ShouldReturnPopulatedProductDtos()
    {
        var user = BuildUser(1L, new List<string> { "PRD01", "PRD02" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var product1 = new Product { Id = "PRD01", Name = "Pro 1", Price = 10, Category = new Category() { Name = "C1"} };
        var product2 = new Product { Id = "PRD02", Name = "Pro 2", Price = 20, Category = new Category() { Name = "C2"} };

        _productRepoMock.Setup(repo => repo.GetProductAsync("PRD01")).ReturnsAsync(product1);
        _productRepoMock.Setup(repo => repo.GetProductAsync("PRD02")).ReturnsAsync(product2);

        var filter = new FilterDto(Nombre: null, Categoria: null, Page: 0, Size: 10, SortBy: "id", Direction: "asc");
        var result = await _favService.GetFavs(1L, filter);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalElements, Is.EqualTo(2));
        Assert.That(result.Value.Content.Count, Is.EqualTo(2));
        Assert.That(result.Value.Content.Any(p => p.Name == "Pro 1"), Is.True);
        Assert.That(result.Value.Content.Any(p => p.Name == "Pro 2"), Is.True);
    }

    [Test]
    public async Task AddFav_WhenUpdateFails_ShouldReturnUserError()
    {
        var user = BuildUser(1L, new List<string>());
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "DB error" }));

        var result = await _favService.AddFav("PRD01", 1L);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task RemoveFav_WhenUpdateFails_ShouldReturnUserError()
    {
        var user = BuildUser(1L, new List<string> { "PRD01" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "DB error" }));

        var result = await _favService.RemoveFav("PRD01", 1L);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task GetFavs_WhenUserNotFound_ShouldReturnUserNotFoundError()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("99")).ReturnsAsync((User)null!);

        var filter = new FilterDto(Nombre: null, Categoria: null, Page: 0, Size: 10, SortBy: "id", Direction: "asc");
        var result = await _favService.GetFavs(99L, filter);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserNotFoundError>());
    }

    [Test]
    public async Task GetFavs_SortByName_Asc_ShouldReturnSortedByName()
    {
        var user = BuildUser(1L, new List<string> { "PRD01", "PRD02" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var product1 = new Product { Id = "PRD01", Name = "Zebra", Price = 10, Stock = 5, Category = new Category { Name = "C1" } };
        var product2 = new Product { Id = "PRD02", Name = "Apple", Price = 20, Stock = 3, Category = new Category { Name = "C2" } };

        _productRepoMock.Setup(r => r.GetProductAsync("PRD01")).ReturnsAsync(product1);
        _productRepoMock.Setup(r => r.GetProductAsync("PRD02")).ReturnsAsync(product2);

        var filter = new FilterDto(null, null, 0, 10, "name", "asc");
        var result = await _favService.GetFavs(1L, filter);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Content.First().Name, Is.EqualTo("Apple"));
    }

    [Test]
    public async Task GetFavs_SortByName_Desc_ShouldReturnReverseSortedByName()
    {
        var user = BuildUser(1L, new List<string> { "PRD01", "PRD02" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var product1 = new Product { Id = "PRD01", Name = "Ant", Price = 10, Stock = 5, Category = new Category { Name = "C1" } };
        var product2 = new Product { Id = "PRD02", Name = "Zebra", Price = 20, Stock = 3, Category = new Category { Name = "C2" } };

        _productRepoMock.Setup(r => r.GetProductAsync("PRD01")).ReturnsAsync(product1);
        _productRepoMock.Setup(r => r.GetProductAsync("PRD02")).ReturnsAsync(product2);

        var filter = new FilterDto(null, null, 0, 10, "name", "desc");
        var result = await _favService.GetFavs(1L, filter);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Content.First().Name, Is.EqualTo("Zebra"));
    }

    [Test]
    public async Task GetFavs_SortByPrice_Asc_ShouldReturnSortedByPrice()
    {
        var user = BuildUser(1L, new List<string> { "PRD01", "PRD02" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var product1 = new Product { Id = "PRD01", Name = "Expensive", Price = 100, Stock = 1, Category = new Category { Name = "C1" } };
        var product2 = new Product { Id = "PRD02", Name = "Cheap", Price = 5, Stock = 2, Category = new Category { Name = "C2" } };

        _productRepoMock.Setup(r => r.GetProductAsync("PRD01")).ReturnsAsync(product1);
        _productRepoMock.Setup(r => r.GetProductAsync("PRD02")).ReturnsAsync(product2);

        var filter = new FilterDto(null, null, 0, 10, "price", "asc");
        var result = await _favService.GetFavs(1L, filter);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Content.First().Price, Is.EqualTo(5));
    }

    [Test]
    public async Task GetFavs_SortByPrice_Desc_ShouldReturnReverseSortedByPrice()
    {
        var user = BuildUser(1L, new List<string> { "PRD01", "PRD02" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var product1 = new Product { Id = "PRD01", Name = "Cheap", Price = 5, Stock = 1, Category = new Category { Name = "C1" } };
        var product2 = new Product { Id = "PRD02", Name = "Expensive", Price = 100, Stock = 2, Category = new Category { Name = "C2" } };

        _productRepoMock.Setup(r => r.GetProductAsync("PRD01")).ReturnsAsync(product1);
        _productRepoMock.Setup(r => r.GetProductAsync("PRD02")).ReturnsAsync(product2);

        var filter = new FilterDto(null, null, 0, 10, "price", "desc");
        var result = await _favService.GetFavs(1L, filter);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Content.First().Price, Is.EqualTo(100));
    }

    [Test]
    public async Task GetFavs_SortByStock_Asc_ShouldReturnSortedByStock()
    {
        var user = BuildUser(1L, new List<string> { "PRD01", "PRD02" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var product1 = new Product { Id = "PRD01", Name = "LowStock", Price = 10, Stock = 1, Category = new Category { Name = "C1" } };
        var product2 = new Product { Id = "PRD02", Name = "HighStock", Price = 20, Stock = 100, Category = new Category { Name = "C2" } };

        _productRepoMock.Setup(r => r.GetProductAsync("PRD01")).ReturnsAsync(product1);
        _productRepoMock.Setup(r => r.GetProductAsync("PRD02")).ReturnsAsync(product2);

        var filter = new FilterDto(null, null, 0, 10, "stock", "asc");
        var result = await _favService.GetFavs(1L, filter);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Content.First().Stock, Is.EqualTo(1));
    }

    [Test]
    public async Task GetFavs_SortByStock_Desc_ShouldReturnReverseSortedByStock()
    {
        var user = BuildUser(1L, new List<string> { "PRD01", "PRD02" });
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);

        var product1 = new Product { Id = "PRD01", Name = "LowStock", Price = 10, Stock = 1, Category = new Category { Name = "C1" } };
        var product2 = new Product { Id = "PRD02", Name = "HighStock", Price = 20, Stock = 100, Category = new Category { Name = "C2" } };

        _productRepoMock.Setup(r => r.GetProductAsync("PRD01")).ReturnsAsync(product1);
        _productRepoMock.Setup(r => r.GetProductAsync("PRD02")).ReturnsAsync(product2);

        var filter = new FilterDto(null, null, 0, 10, "stock", "desc");
        var result = await _favService.GetFavs(1L, filter);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Content.First().Stock, Is.EqualTo(100));
    }

    [Test]
    public async Task RemoveFav_WhenUserNotFound_ShouldReturnUserNotFoundError()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("99")).ReturnsAsync((User)null!);

        var result = await _favService.RemoveFav("PRD01", 99L);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserNotFoundError>());
    }
}
