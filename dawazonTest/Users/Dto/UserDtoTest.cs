using dawazonBackend.Users.Dto;

namespace dawazonTest.Users.Dto;

[TestFixture]
[Description("Tests para validación de UserDto y el métoddo SetTeléfono")]
public class UserDtoTest
{
    [Test]
    [Description("SetTelefono: Debería limpiar el prefijo +34")]
    public void SetTelefono_WithPlus34_ShouldRemovePrefix()
    {
        var dto = new UserDto();
        dto.SetTelefono("+34600123456");
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));
    }

    [Test]
    [Description("SetTelefono: Debería limpiar el prefijo 0034")]
    public void SetTelefono_With0034_ShouldRemovePrefix()
    {
        var dto = new UserDto();
        dto.SetTelefono("0034600123456");
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));
    }

    [Test]
    [Description("SetTelefono: Se debería limpiar el prefijo 34 cuando la longitud es > 9")]
    public void SetTelefono_With34PrefixAndLengthGreaterThan9_ShouldRemovePrefix()
    {
        var dto = new UserDto();
        dto.SetTelefono("34600123456");
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));
    }

    [Test]
    [Description("SetTelefono: Debería no limpiar el prefijo 34 cuando la longitud es 9")]
    public void SetTelefono_With34PrefixAndLengthEquals9_ShouldKeepAsIs()
    {
        var dto = new UserDto();
        dto.SetTelefono("346001234");
        Assert.That(dto.Telefono, Is.EqualTo("346001234"));
    }

    [Test]
    [Description("SetTelefono: Se deberían limpiar espacios y puntos")]
    public void SetTelefono_WithSpecialCharacters_ShouldRemoveThem()
    {
        var dto = new UserDto();
        dto.SetTelefono("600 123 456");
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));

        dto.SetTelefono("600-123-456");
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));

        dto.SetTelefono("600.123.456");
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));
    }

    [Test]
    [Description("SetTelefono: Debería manejar paréntesis")]
    public void SetTelefono_WithParentheses_ShouldRemoveThem()
    {
        var dto = new UserDto();
        dto.SetTelefono("(600) 123-456");
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));
    }

    [Test]
    [Description("SetTelefono: Debería manejar espacios vacíos o en blanco")]
    public void SetTelefono_EmptyOrWhitespace_ShouldReturnEmptyString()
    {
        var dto = new UserDto();
        
        dto.SetTelefono("");
        Assert.That(dto.Telefono, Is.EqualTo(""));
        
        dto.SetTelefono("   ");
        Assert.That(dto.Telefono, Is.EqualTo(""));
    }

    [Test]
    [Description("SetTelefono: Debería manejar números limpios")]
    public void SetTelefono_AlreadyClean_ShouldKeepAsIs()
    {
        var dto = new UserDto();
        dto.SetTelefono("600123456");
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));
    }

    [Test]
    [Description("UserDto: Los valores por defecto se deberían inicializar correctamente")]
    public void UserDto_DefaultValues_ShouldBeInitialized()
    {
        var dto = new UserDto();
        
        Assert.That(dto.Roles, Is.Not.Null);
        Assert.That(dto.Roles, Is.Empty);
        Assert.That(dto.Telefono, Is.Null);
        Assert.That(dto.Avatar, Is.EqualTo(""));
    }

    [Test]
    [Description("UserDto: Las propiedades deberian ser setteables")]
    public void UserDto_Properties_ShouldBeSettable()
    {
        var dto = new UserDto
        {
            Id = 1,
            Nombre = "Test User",
            Email = "test@example.com",
            Telefono = "600123456",
            Calle = "Test Street",
            Ciudad = "Test City",
            CodigoPostal = "12345",
            Provincia = "Test Province",
            Avatar = "/avatar.jpg"
        };

        Assert.That(dto.Id, Is.EqualTo(1));
        Assert.That(dto.Nombre, Is.EqualTo("Test User"));
        Assert.That(dto.Email, Is.EqualTo("test@example.com"));
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));
        Assert.That(dto.Calle, Is.EqualTo("Test Street"));
        Assert.That(dto.Ciudad, Is.EqualTo("Test City"));
        Assert.That(dto.CodigoPostal, Is.EqualTo("12345"));
        Assert.That(dto.Provincia, Is.EqualTo("Test Province"));
        Assert.That(dto.Avatar, Is.EqualTo("/avatar.jpg"));
    }
}
