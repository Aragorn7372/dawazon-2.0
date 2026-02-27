using dawazonBackend.Common.Utils;
using NUnit.Framework;

namespace dawazonTest.Common.Utils;

/// <summary>
/// Tests unitarios para IdGenerator.
/// SRP: cada test valida una única responsabilidad del generador.
/// OCP: se extienden casos sin modificar la clase bajo prueba.
/// </summary>
[TestFixture]
[Description("IdGenerator Unit Tests")]
public class IdGeneratorTest
{
    private const string AllowedChars = "QWRTYPSDFGHJKLZXCVBNMqwrtypsdfghjklzxcvbnm1234567890-_";
    private const int ExpectedLength = 12;

    private IdGenerator _generator;

    [SetUp]
    public void SetUp()
    {
        _generator = new IdGenerator();
    }

    [Test]
    [Description("GeneratesTemporaryValues: Debe retornar false para que EF Core trate el valor como permanente")]
    public void GeneratesTemporaryValues_ShouldBeFalse()
    {
        Assert.That(_generator.GeneratesTemporaryValues, Is.False);
    }

    [Test]
    [Description("Next debe retornar exactamente 12 caracteres")]
    public void Next_ShouldReturn12CharacterString()
    {
        var id = _generator.Next(null!);

        Assert.That(id, Has.Length.EqualTo(ExpectedLength));
    }

    [Test]
    [Description("Next debe retornar solo caracteres del charset permitido")]
    public void Next_ShouldOnlyContainAllowedCharacters()
    {
        var id = _generator.Next(null!);

        Assert.That(id.All(c => AllowedChars.Contains(c)), Is.True,
            $"El ID '{id}' contiene caracteres no permitidos.");
    }


    [Test]
    [Description("Next llamado 1000 veces debe generar IDs únicos (aleatoriedad criptográfica)")]
    public void Next_CalledMultipleTimes_ShouldGenerateUniqueValues()
    {
        const int iterations = 1000;

        var ids = Enumerable.Range(0, iterations)
                            .Select(_ => _generator.Next(null!))
                            .ToHashSet();

        Assert.That(ids, Has.Count.EqualTo(iterations),
            "Se detectaron IDs duplicados, lo que indica aleatoriedad insuficiente.");
    }

    [Test]
    [Description("Next no debe retornar null ni cadena vacía")]
    public void Next_ShouldReturnNonNullAndNonEmptyString()
    {
        var id = _generator.Next(null!);

        Assert.That(id, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    [Description("En 500 IDs generados el charset completo debe estar cubierto estadísticamente")]
    public void Next_Over500Ids_ShouldCoverAllAllowedCharacters()
    {
        var usedChars = new HashSet<char>();

        for (int i = 0; i < 500; i++)
        {
            foreach (var c in _generator.Next(null!))
                usedChars.Add(c);
        }

        var missingChars = AllowedChars.Where(c => !usedChars.Contains(c)).ToList();
        Assert.That(missingChars, Is.Empty,
            $"Faltan estos caracteres en las muestras: {string.Join(", ", missingChars)}");
    }
}