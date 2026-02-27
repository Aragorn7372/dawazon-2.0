using dawazonBackend.Users.Dto;

namespace dawazonTest.Users.Dto;

[TestFixture]
[Description("Tests para lógica de UserRequestDtofor")]
public class UserRequestDtoTest
{
    [Test]
    [Description("Telefono setter: Debería limpiar espacios, tabulaciones, puntos....")]
    public void Telefono_Setter_ShouldCleanPrefixesAndSymbols()
    {
        var dto = new UserRequestDto();

        dto.Telefono = "+34 600 123 456";
        Assert.That(dto.Telefono, Is.EqualTo("600123456"));

        dto.Telefono = "0034-600-123-457";
        Assert.That(dto.Telefono, Is.EqualTo("600123457"));

        dto.Telefono = "(34) 600.123.458";
        Assert.That(dto.Telefono, Is.EqualTo("600123458"));

        dto.Telefono = "600123459";
        Assert.That(dto.Telefono, Is.EqualTo("600123459"));

        dto.Telefono = null;
        Assert.That(dto.Telefono, Is.EqualTo(""));
    }
}
