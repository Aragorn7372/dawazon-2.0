using dawazonBackend.Common.Dto;
using NUnit.Framework;

namespace dawazonTest.Common.Dto;

[TestFixture]
[Description("AdminStatsDto Unit Tests")]
public class AdminStatsDtoTest
{
    [Test]
    [Description("AdminStatsDto: TotalProducts debe poder asignarse y leerse correctamente")]
    public void AdminStatsDto_TotalProducts_ShouldGetAndSet()
    {
        var dto = new AdminStatsDto { TotalProducts = 42 };
        Assert.That(dto.TotalProducts, Is.EqualTo(42));
    }

    [Test]
    [Description("AdminStatsDto: TotalUsers debe poder asignarse y leerse correctamente")]
    public void AdminStatsDto_TotalUsers_ShouldGetAndSet()
    {
        var dto = new AdminStatsDto { TotalUsers = 100 };
        Assert.That(dto.TotalUsers, Is.EqualTo(100));
    }

    [Test]
    [Description("AdminStatsDto: TotalSales debe poder asignarse y leerse correctamente")]
    public void AdminStatsDto_TotalSales_ShouldGetAndSet()
    {
        var dto = new AdminStatsDto { TotalSales = 7 };
        Assert.That(dto.TotalSales, Is.EqualTo(7));
    }

    [Test]
    [Description("AdminStatsDto: TotalEarnings debe poder asignarse y leerse correctamente")]
    public void AdminStatsDto_TotalEarnings_ShouldGetAndSet()
    {
        var dto = new AdminStatsDto { TotalEarnings = 999.99 };
        Assert.That(dto.TotalEarnings, Is.EqualTo(999.99).Within(0.001));
    }

    [Test]
    [Description("AdminStatsDto: OutOfStockCount debe poder asignarse y leerse correctamente")]
    public void AdminStatsDto_OutOfStockCount_ShouldGetAndSet()
    {
        var dto = new AdminStatsDto { OutOfStockCount = 3 };
        Assert.That(dto.OutOfStockCount, Is.EqualTo(3));
    }

    [Test]
    [Description("AdminStatsDto: ProductsByCategory debe inicializarse como diccionario vacío por defecto")]
    public void AdminStatsDto_ProductsByCategory_DefaultShouldBeEmptyDictionary()
    {
        var dto = new AdminStatsDto();
        Assert.That(dto.ProductsByCategory, Is.Not.Null);
        Assert.That(dto.ProductsByCategory, Is.Empty);
    }

    [Test]
    [Description("AdminStatsDto: ProductsByCategory debe poder asignarse y leerse correctamente")]
    public void AdminStatsDto_ProductsByCategory_ShouldGetAndSet()
    {
        var categories = new Dictionary<string, int>
        {
            { "Electrónica", 10 },
            { "Ropa", 5 }
        };

        var dto = new AdminStatsDto { ProductsByCategory = categories };

        Assert.That(dto.ProductsByCategory, Has.Count.EqualTo(2));
        Assert.That(dto.ProductsByCategory["Electrónica"], Is.EqualTo(10));
        Assert.That(dto.ProductsByCategory["Ropa"], Is.EqualTo(5));
    }

    [Test]
    [Description("AdminStatsDto: todos los valores por defecto numéricos deben ser cero")]
    public void AdminStatsDto_DefaultNumericValues_ShouldBeZero()
    {
        var dto = new AdminStatsDto();

        Assert.That(dto.TotalProducts,  Is.EqualTo(0));
        Assert.That(dto.TotalUsers,     Is.EqualTo(0));
        Assert.That(dto.TotalSales,     Is.EqualTo(0));
        Assert.That(dto.TotalEarnings,  Is.EqualTo(0.0));
        Assert.That(dto.OutOfStockCount, Is.EqualTo(0));
    }
}
