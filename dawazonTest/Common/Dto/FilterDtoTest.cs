using dawazonBackend.Common.Dto;
using NUnit.Framework;

namespace dawazonTest.Common.Dto;

[TestFixture]
[Description("FilterDto Unit Tests")]
public class FilterDtoTest
{
    [Test]
    [Description("FilterDto con solo parámetros obligatorios debe tener defaults de paginación sensatos")]
    public void FilterDto_WithNullableParamsOnly_ShouldHaveSensibleDefaults()
    {
        var filter = new FilterDto(null, null);

        Assert.That(filter.Page,      Is.EqualTo(0));
        Assert.That(filter.Size,      Is.EqualTo(10));
        Assert.That(filter.SortBy,    Is.EqualTo("id"));
        Assert.That(filter.Direction, Is.EqualTo("asc"));
    }

    [Test]
    [Description("FilterDto con Nombre y Categoria nulos debe tener esas propiedades en null")]
    public void FilterDto_WhenNombreAndCategoriaAreNull_ShouldBeNull()
    {
        var filter = new FilterDto(null, null);

        Assert.That(filter.Nombre,    Is.Null);
        Assert.That(filter.Categoria, Is.Null);
    }

    [Test]
    [Description("FilterDto permite sobreescribir todos los valores")]
    public void FilterDto_WithAllCustomValues_ShouldRespectThem()
    {
        var filter = new FilterDto(
            Nombre: "Funko",
            Categoria: "Figuras",
            Page: 3,
            Size: 20,
            SortBy: "precio",
            Direction: "desc");

        Assert.That(filter.Nombre,    Is.EqualTo("Funko"));
        Assert.That(filter.Categoria, Is.EqualTo("Figuras"));
        Assert.That(filter.Page,      Is.EqualTo(3));
        Assert.That(filter.Size,      Is.EqualTo(20));
        Assert.That(filter.SortBy,    Is.EqualTo("precio"));
        Assert.That(filter.Direction, Is.EqualTo("desc"));
    }

    [Test]
    [Description("FilterDto con los mismos valores debe ser igual por valor (record semantics)")]
    public void FilterDto_WithSameValues_ShouldBeEqual()
    {
        var a = new FilterDto("Funko", "Figuras", 0, 10, "id", "asc");
        var b = new FilterDto("Funko", "Figuras", 0, 10, "id", "asc");

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    [Description("FilterDto con distintos valores NO debe ser igual")]
    public void FilterDto_WithDifferentValues_ShouldNotBeEqual()
    {
        var a = new FilterDto("Funko", null, 0, 10, "id", "asc");
        var b = new FilterDto("Batman", null, 0, 10, "id", "asc");

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    [Description("FilterDto es inmutable: con (Nombre: 'X') debe crear un nuevo record sin modificar el original")]
    public void FilterDto_WithExpression_ShouldCreateNewRecord()
    {
        var original = new FilterDto("Funko", null);
        var modified = original with { Nombre = "Batman" };

        Assert.That(original.Nombre, Is.EqualTo("Funko"));
        Assert.That(modified.Nombre, Is.EqualTo("Batman"));
        Assert.That(original, Is.Not.EqualTo(modified));
    }
}