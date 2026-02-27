using dawazonBackend.Users.Dto;

namespace dawazonTest.Users.Dto;

[TestFixture]
[Description("Tests para validdacion de RegisterDto")]
public class RegisterDtoTest
{
    [Test]
    [Description("RegisterDto: Debería crear con propiedades Requeridas")]
    public void RegisterDto_ShouldCreateWithRequiredProperties()
    {
        var dto = new RegisterDto { Username = "juanperez", Email = "juan@example.com", Password = "password123" };
        
        Assert.That(dto.Username, Is.EqualTo("juanperez"));
        Assert.That(dto.Email, Is.EqualTo("juan@example.com"));
        Assert.That(dto.Password, Is.EqualTo("password123"));
    }

    [Test]
    [Description("RegisterDto: Los re")]
    public void RegisterDto_Equality_ShouldWork()
    {
        var dto1 = new RegisterDto { Username = "user", Email = "user@test.com", Password = "pass" };
        var dto2 = new RegisterDto { Username = "user", Email = "user@test.com", Password = "pass" };
        
        Assert.That(dto1, Is.EqualTo(dto2));
    }

    [Test]
    [Description("RegisterDto: Debería permitir alfanuméricos y guión bajo")]
    public void RegisterDto_ValidUsernameCharacters_ShouldWork()
    {
        var dto = new RegisterDto { Username = "user_123", Email = "test@test.com", Password = "pass123" };
        
        Assert.That(dto.Username, Is.EqualTo("user_123"));
    }

    [Test]
    [Description("RegisterDto: Distintas instancias deberían diferir")]
    public void RegisterDto_DifferentInstances_ShouldNotBeEqual()
    {
        var dto1 = new RegisterDto { Username = "user1", Email = "user1@test.com", Password = "pass" };
        var dto2 = new RegisterDto { Username = "user2", Email = "user2@test.com", Password = "pass" };
        
        Assert.That(dto1, Is.Not.EqualTo(dto2));
    }
}
