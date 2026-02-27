using dawazonBackend.Users.Dto;

namespace dawazonTest.Users.Dto;

[TestFixture]
[Description("Tests for RegisterDto validation")]
public class RegisterDtoTest
{
    [Test]
    [Description("RegisterDto: Should create instance with required properties")]
    public void RegisterDto_ShouldCreateWithRequiredProperties()
    {
        var dto = new RegisterDto { Username = "juanperez", Email = "juan@example.com", Password = "password123" };
        
        Assert.That(dto.Username, Is.EqualTo("juanperez"));
        Assert.That(dto.Email, Is.EqualTo("juan@example.com"));
        Assert.That(dto.Password, Is.EqualTo("password123"));
    }

    [Test]
    [Description("RegisterDto: Record equality should work")]
    public void RegisterDto_Equality_ShouldWork()
    {
        var dto1 = new RegisterDto { Username = "user", Email = "user@test.com", Password = "pass" };
        var dto2 = new RegisterDto { Username = "user", Email = "user@test.com", Password = "pass" };
        
        Assert.That(dto1, Is.EqualTo(dto2));
    }

    [Test]
    [Description("RegisterDto: Should allow alphanumeric and underscore in username")]
    public void RegisterDto_ValidUsernameCharacters_ShouldWork()
    {
        var dto = new RegisterDto { Username = "user_123", Email = "test@test.com", Password = "pass123" };
        
        Assert.That(dto.Username, Is.EqualTo("user_123"));
    }

    [Test]
    [Description("RegisterDto: Different instances should not be equal")]
    public void RegisterDto_DifferentInstances_ShouldNotBeEqual()
    {
        var dto1 = new RegisterDto { Username = "user1", Email = "user1@test.com", Password = "pass" };
        var dto2 = new RegisterDto { Username = "user2", Email = "user2@test.com", Password = "pass" };
        
        Assert.That(dto1, Is.Not.EqualTo(dto2));
    }
}
