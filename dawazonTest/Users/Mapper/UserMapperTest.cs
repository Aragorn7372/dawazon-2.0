using dawazonBackend.Cart.Models;
using dawazonBackend.Users.Mapper;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace dawazonTest.Users.Mapper;

[TestFixture]
[Description("Tests para UserMapper")]
public class UserMapperTest
{
    private Mock<UserManager<User>> _userManagerMock;

    [SetUp]
    public void SetUp()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    [Test]
    [Description("ToDtoAsync: Debe asignar correctamente el usuario y los roles a UserDto.")]
    public async Task ToDtoAsync_ShouldMapCorrectly()
    {
        var user = new User
        {
            Id = 1L,
            Name = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "123456789",
            Client = new Client
            {
                Name = "John Doe",
                Address = new Address
                {
                    Street = "Main St",
                    City = "Springfield",
                    PostalCode = 12345,
                    Province = "State"
                }
            }
        };

        var roles = new List<string> { "Admin", "User" };
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);

        var dto = await user.ToDtoAsync(_userManagerMock.Object);

        Assert.That(dto.Id, Is.EqualTo(1L));
        Assert.That(dto.Nombre, Is.EqualTo("John Doe"));
        Assert.That(dto.Email, Is.EqualTo("john@example.com"));
        Assert.That(dto.Telefono, Is.EqualTo("123456789"));
        Assert.That(dto.Calle, Is.EqualTo("Main St"));
        Assert.That(dto.Ciudad, Is.EqualTo("Springfield"));
        Assert.That(dto.CodigoPostal, Is.EqualTo("12345"));
        Assert.That(dto.Provincia, Is.EqualTo("State"));
        
        Assert.That(dto.Roles.Count, Is.EqualTo(2));
        Assert.That(dto.Roles.Contains("Admin"), Is.True);
        Assert.That(dto.Roles.Contains("User"), Is.True);
    }
}
