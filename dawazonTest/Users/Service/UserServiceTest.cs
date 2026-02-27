using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Common.Storage;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using MockQueryable.Moq;
using Moq;

namespace dawazonTest.Users.Service;

[TestFixture]
[Description("Tests for UserService")]
public class UserServiceTest
{
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<IStorage> _storageMock;
    private UserService _userService;

    [SetUp]
    public void SetUp()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        _storageMock = new Mock<IStorage>();

        _userService = new UserService(new NullLogger<UserService>(), _userManagerMock.Object, _storageMock.Object);
    }

    private User BuildUser(long id, string name, bool isDeleted = false)
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = $"{name.Replace(" ", "").ToLower()}@example.com",
            IsDeleted = isDeleted,
            Client = new dawazonBackend.Cart.Models.Client
            {
                Name = name,
                Address = new dawazonBackend.Cart.Models.Address()
            }
        };
    }

    [Test]
    [Description("GetAllAsync: Should return paginated users excluding deleted ones")]
    public async Task GetAllAsync_ShouldReturnPaginatedActiveUsers()
    {
        var filter = new FilterDto(Nombre: null, Categoria: null, Page: 0, Size: 10, SortBy: "nombre", Direction: "asc");
        var users = new List<User>
        {
            BuildUser(1, "Alice"),
            BuildUser(2, "Bob"),
            BuildUser(3, "Charlie", isDeleted: true)
        };

        var mockUsers = users.BuildMock();
        _userManagerMock.Setup(um => um.Users).Returns(mockUsers);
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "Usuario" });

        var result = await _userService.GetAllAsync(filter);

        Assert.That(result.TotalElements, Is.EqualTo(2));
        Assert.That(result.Content.Count, Is.EqualTo(2));
        Assert.That(result.Content.First().Nombre, Is.EqualTo("Alice"));
    }

    [Test]
    [Description("GetByIdAsync: Should return user DTO if found")]
    public async Task GetByIdAsync_WhenFound_ShouldReturnDto()
    {
        var user = BuildUser(1, "Alice");
        _userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var result = await _userService.GetByIdAsync("1");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("Alice"));
    }

    [Test]
    [Description("GetByIdAsync: Should return UserNotFoundError if not found")]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnError()
    {
        _userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync((User)null!);

        var result = await _userService.GetByIdAsync("1");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserNotFoundError>());
    }

    [Test]
    [Description("UpdateByIdAsync: Should update fields and return DTO on success")]
    public async Task UpdateByIdAsync_Success_ShouldReturnUpdatedDto()
    {
        var id = 1L;
        var existingUser = BuildUser(id, "Old Name");
        var users = new List<User> { existingUser }.BuildMock();
        _userManagerMock.Setup(um => um.Users).Returns(users);

        var request = new UserRequestDto
        {
            Nombre = "New Name",
            Ciudad = "New City",
            Provincia = "New Province",
            CodigoPostal = "12345",
            Calle = "New St",
            Email = "new@example.com",
            Telefono = "600123456"
        };

        _userManagerMock.Setup(um => um.UpdateAsync(existingUser)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.GetRolesAsync(existingUser)).ReturnsAsync(new List<string>());

        var result = await _userService.UpdateByIdAsync(id, request, image: null);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Nombre, Is.EqualTo("New Name"));
        Assert.That(existingUser.Email, Is.EqualTo("new@example.com"));
        Assert.That(existingUser.PhoneNumber, Is.EqualTo("600123456"));
    }

    [Test]
    [Description("UpdateByIdAsync: Should update image if provided")]
    public async Task UpdateByIdAsync_WithImage_ShouldUpdateImage()
    {
        var id = 1L;
        var existingUser = BuildUser(id, "User");
        var users = new List<User> { existingUser }.BuildMock();
        _userManagerMock.Setup(um => um.Users).Returns(users);

        var mockFile = new Mock<IFormFile>();
        _storageMock.Setup(s => s.SaveFileAsync(mockFile.Object, "users"))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success<string, dawazonBackend.Products.Errors.ProductError>("/new/avatar.png"));

        _userManagerMock.Setup(um => um.UpdateAsync(existingUser)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.GetRolesAsync(existingUser)).ReturnsAsync(new List<string>());
        
        var request = new UserRequestDto { CodigoPostal = "1" };

        var result = await _userService.UpdateByIdAsync(id, request, mockFile.Object);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(existingUser.Avatar, Is.EqualTo("/new/avatar.png"));
    }

    [Test]
    [Description("BanUserById: Should set IsDeleted to true")]
    public async Task BanUserById_WhenFound_ShouldLogicalDelete()
    {
        var user = BuildUser(1, "To Ban");
        _userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);

        await _userService.BanUserById("1");

        Assert.That(user.IsDeleted, Is.True);
        _userManagerMock.Verify(um => um.UpdateAsync(user), Times.Once);
    }

    [Test]
    [Description("BanUserById: Should do nothing when user is not found")]
    public async Task BanUserById_WhenNotFound_ShouldReturnWithoutUpdate()
    {
        _userManagerMock.Setup(um => um.FindByIdAsync("99")).ReturnsAsync((User)null!);

        await _userService.BanUserById("99");

        _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    [Description("GetTotalUsersCountAsync: Should return count of non-deleted users")]
    public async Task GetTotalUsersCountAsync_ShouldReturnActiveUserCount()
    {
        var users = new List<User>
        {
            BuildUser(1, "Alice"),
            BuildUser(2, "Bob"),
            BuildUser(3, "Deleted", isDeleted: true)
        };

        var mockUsers = users.BuildMock();
        _userManagerMock.Setup(um => um.Users).Returns(mockUsers);

        var result = await _userService.GetTotalUsersCountAsync();

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    [Description("UpdateByIdAsync: Should return UserNotFoundError when user does not exist")]
    public async Task UpdateByIdAsync_WhenUserNotFound_ShouldReturnError()
    {
        var emptyUsers = new List<User>().BuildMock();
        _userManagerMock.Setup(um => um.Users).Returns(emptyUsers);

        var request = new UserRequestDto { Nombre = "Test", CodigoPostal = "1" };
        var result = await _userService.UpdateByIdAsync(99L, request, image: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserNotFoundError>());
    }

    [Test]
    [Description("UpdateByIdAsync: Should return UserUpdateError when UpdateAsync fails")]
    public async Task UpdateByIdAsync_WhenUpdateFails_ShouldReturnError()
    {
        var id = 1L;
        var existingUser = BuildUser(id, "User");
        var users = new List<User> { existingUser }.BuildMock();
        _userManagerMock.Setup(um => um.Users).Returns(users);

        var request = new UserRequestDto
        {
            Nombre = "New Name",
            Ciudad = "City",
            Provincia = "Province",
            CodigoPostal = "12345",
            Calle = "Street",
            Email = "new@example.com",
            Telefono = "600000000"
        };

        var failedResult = IdentityResult.Failed(new IdentityError { Description = "DB error" });
        _userManagerMock.Setup(um => um.UpdateAsync(existingUser)).ReturnsAsync(failedResult);

        var result = await _userService.UpdateByIdAsync(id, request, image: null);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserUpdateError>());
    }

    [Test]
    [Description("UpdateByIdAsync: Should return UserUpdateError when image upload fails")]
    public async Task UpdateByIdAsync_WhenImageUploadFails_ShouldReturnError()
    {
        var id = 1L;
        var existingUser = BuildUser(id, "User");
        var users = new List<User> { existingUser }.BuildMock();
        _userManagerMock.Setup(um => um.Users).Returns(users);

        var mockFile = new Mock<IFormFile>();
        _storageMock.Setup(s => s.SaveFileAsync(mockFile.Object, "users"))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Failure<string, dawazonBackend.Products.Errors.ProductError>(
                new dawazonBackend.Products.Errors.ProductStorageError("Upload failed")));

        var request = new UserRequestDto { CodigoPostal = "1" };
        var result = await _userService.UpdateByIdAsync(id, request, mockFile.Object);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserUpdateError>());
    }

    [Test]
    [Description("GetAllAsync: Should sort by id when sortBy is unknown field")]
    public async Task GetAllAsync_WithDefaultSorting_ShouldReturnUsersById()
    {
        var filter = new FilterDto(Nombre: null, Categoria: null, Page: 0, Size: 10, SortBy: "id", Direction: "asc");
        var users = new List<User>
        {
            BuildUser(2, "Bob"),
            BuildUser(1, "Alice")
        };

        var mockUsers = users.BuildMock();
        _userManagerMock.Setup(um => um.Users).Returns(mockUsers);
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());

        var result = await _userService.GetAllAsync(filter);

        Assert.That(result.TotalElements, Is.EqualTo(2));
        Assert.That(result.Content.First().Nombre, Is.EqualTo("Alice"));
    }

    [Test]
    [Description("GetAllAsync: Should sort by nombre descending when direction is desc")]
    public async Task GetAllAsync_WithDescendingSort_ShouldReturnUsersInReverseOrder()
    {
        var filter = new FilterDto(Nombre: null, Categoria: null, Page: 0, Size: 10, SortBy: "nombre", Direction: "desc");
        var users = new List<User>
        {
            BuildUser(1, "Alice"),
            BuildUser(2, "Bob")
        };

        var mockUsers = users.BuildMock();
        _userManagerMock.Setup(um => um.Users).Returns(mockUsers);
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());

        var result = await _userService.GetAllAsync(filter);

        Assert.That(result.Content.First().Nombre, Is.EqualTo("Bob"));
    }
}
