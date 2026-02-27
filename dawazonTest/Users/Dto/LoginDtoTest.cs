using dawazonBackend.Users.Dto;

namespace dawazonTest.Users.Dto;

[TestFixture]
[Description("Test para validación de LoginDto")]
public class LoginDtoTest
{
    [Test]
    [Description("LoginDto: debería crear instancia con las propiedades requeridas")]
    public void LoginDto_ShouldCreateWithRequiredProperties()
    {
        var dto = new LoginDto { UsernameOrEmail = "username", Password = "password" };
        
        Assert.That(dto.UsernameOrEmail, Is.EqualTo("username"));
        Assert.That(dto.Password, Is.EqualTo("password"));
    }

    [Test]
    [Description("LoginDto: Debería devolver username vacío")]
    public void LoginDto_EmptyUsername_ShouldBeAllowed()
    {
        var dto = new LoginDto { UsernameOrEmail = "", Password = "password" };
        
        Assert.That(dto.UsernameOrEmail, Is.EqualTo(""));
    }

    [Test]
    [Description("LoginDto: Debería permitir email como username")]
    public void LoginDto_EmailAsUsername_ShouldBeAllowed()
    {
        var dto = new LoginDto { UsernameOrEmail = "user@example.com", Password = "password" };
        
        Assert.That(dto.UsernameOrEmail, Is.EqualTo("user@example.com"));
    }

    [Test]
    [Description("LoginDto: Son iguales")]
    public void LoginDto_Equality_ShouldWork()
    {
        var dto1 = new LoginDto { UsernameOrEmail = "user", Password = "pass" };
        var dto2 = new LoginDto { UsernameOrEmail = "user", Password = "pass" };
        
        Assert.That(dto1, Is.EqualTo(dto2));
    }

    [Test]
    [Description("LoginDto: Diferentes instancias deberían no ser iguales")]
    public void LoginDto_DifferentInstances_ShouldNotBeEqual()
    {
        var dto1 = new LoginDto { UsernameOrEmail = "user1", Password = "pass" };
        var dto2 = new LoginDto { UsernameOrEmail = "user2", Password = "pass" };
        
        Assert.That(dto1, Is.Not.EqualTo(dto2));
    }
}
