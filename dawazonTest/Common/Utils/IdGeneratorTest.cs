using dawazonBackend.Common.Utils;
using NUnit.Framework;

namespace dawazonTest.Common.Utils;

/// <summary>
/// Tests unitarios para IdGenerator.
/// SRP: cada test valida una única responsabilidad del generador.
/// OCP: se extienden casos sin modificar la clase bajo prueba.
/// </summary>
[TestFixture]
[Description("IdGenerator Unit Tests — SOLID + FIRST Principles")]
public class IdGeneratorTest
{
    private const string AllowedChars = "QWRTYPSDFGHJKLZXCVBNMqwrtypsdfghjklzxcvbnm1234567890-_";
    private const int ExpectedLength = 12;

    private IdGenerator _generator;

    [SetUp]
    public void SetUp()
    {
        // ISP: instanciamos solo lo que necesitamos; no hay dependencias externas
        _generator = new IdGenerator();
    }

    // ──────────────────────────────────────────
    //  GeneratesTemporaryValues
    // ──────────────────────────────────────────

    [Test]
    [Description("GeneratesTemporaryValues debe retornar false para que EF Core trate el valor como permanente")]
    public void GeneratesTemporaryValues_ShouldBeFalse()
    {
        Assert.That(_generator.GeneratesTemporaryValues, Is.False);
    }

    // ──────────────────────────────────────────
    //  Next: longitud
    // ──────────────────────────────────────────

    [Test]
    [Description("Next debe retornar exactamente 12 caracteres")]
    public void Next_ShouldReturn12CharacterString()
    {
        // Arrange & Act
        var id = _generator.Next(null!);

        // Assert
        Assert.That(id, Has.Length.EqualTo(ExpectedLength));
    }

    // ──────────────────────────────────────────
    //  Next: charset
    // ──────────────────────────────────────────

    [Test]
    [Description("Next debe retornar solo caracteres del charset permitido")]
    public void Next_ShouldOnlyContainAllowedCharacters()
    {
        // Arrange & Act
        var id = _generator.Next(null!);

        // Assert — todos los caracteres están en el charset
        Assert.That(id.All(c => AllowedChars.Contains(c)), Is.True,
            $"El ID '{id}' contiene caracteres no permitidos.");
    }

    // ──────────────────────────────────────────
    //  Next: unicidad / aleatoriedad
    // ──────────────────────────────────────────

    [Test]
    [Description("Next llamado 1000 veces debe generar IDs únicos (aleatoriedad criptográfica)")]
    public void Next_CalledMultipleTimes_ShouldGenerateUniqueValues()
    {
        // Arrange
        const int iterations = 1000;

        // Act
        var ids = Enumerable.Range(0, iterations)
                            .Select(_ => _generator.Next(null!))
                            .ToHashSet();

        // Assert — en 1000 iteraciones no debe haber colisiones
        Assert.That(ids, Has.Count.EqualTo(iterations),
            "Se detectaron IDs duplicados, lo que indica aleatoriedad insuficiente.");
    }

    // ──────────────────────────────────────────
    //  Next: no nulo / no vacío
    // ──────────────────────────────────────────

    [Test]
    [Description("Next no debe retornar null ni cadena vacía")]
    public void Next_ShouldReturnNonNullAndNonEmptyString()
    {
        // Arrange & Act
        var id = _generator.Next(null!);

        // Assert
        Assert.That(id, Is.Not.Null.And.Not.Empty);
    }

    // ──────────────────────────────────────────
    //  Next: charset cubre dígitos, letras y símbolos
    // ──────────────────────────────────────────

    [Test]
    [Description("En 500 IDs generados el charset completo debe estar cubierto estadísticamente")]
    public void Next_Over500Ids_ShouldCoverAllAllowedCharacters()
    {
        // Arrange
        var usedChars = new HashSet<char>();

        // Act
        for (int i = 0; i < 500; i++)
        {
            foreach (var c in _generator.Next(null!))
                usedChars.Add(c);
        }

        // Assert — todos los caracteres del charset deben aparecer al menos una vez
        var missingChars = AllowedChars.Where(c => !usedChars.Contains(c)).ToList();
        Assert.That(missingChars, Is.Empty,
            $"Faltan estos caracteres en las muestras: {string.Join(", ", missingChars)}");
    }
}