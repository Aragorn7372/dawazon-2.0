using dawazon2._0.Mapper;
using dawazon2._0.Models;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Mapper;

[TestFixture]
public class UserMapperTests
{
    [Test]
    public void ToDto_FromLoginModelView_ShouldMapCorrectly()
    {
        // Arrange
        var vm = new LoginModelView
        {
            UsernameOrEmail = "testuser",
            Password = "password123"
        };

        // Act
        var dto = vm.ToDto();

        // Assert
        Assert.That(dto.UsernameOrEmail, Is.EqualTo("testuser"));
        Assert.That(dto.Password, Is.EqualTo("password123"));
    }

    [Test]
    public void ToDto_FromRegisterModelView_ShouldMapCorrectly()
    {
        // Arrange
        var vm = new RegisterModelView
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        var dto = vm.ToDto();

        // Assert
        Assert.That(dto.Username, Is.EqualTo("newuser"));
        Assert.That(dto.Email, Is.EqualTo("newuser@example.com"));
        Assert.That(dto.Password, Is.EqualTo("password123"));
    }
}