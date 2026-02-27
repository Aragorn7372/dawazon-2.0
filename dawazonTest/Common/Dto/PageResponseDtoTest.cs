using dawazonBackend.Common.Dto;
using NUnit.Framework;

namespace dawazonTest.Common.Dto;

[TestFixture]
[Description("PageResponseDto Unit Tests")]
public class PageResponseDtoTest
{
    private static PageResponseDto<string> MakePage(
        List<string> content,
        int totalPages,
        long totalElements,
        int pageNumber,
        int pageSize = 10,
        string sortBy = "id",
        string direction = "asc")
        => new(content, totalPages, totalElements, pageSize, pageNumber,
               content.Count, sortBy, direction);

    [Test]
    [Description("PageResponseDto debe conservar los valores pasados al constructor")]
    public void PageResponseDto_Constructor_ShouldPreserveAllValues()
    {
        var items = new List<string> { "a", "b" };
        var page  = MakePage(items, totalPages: 5, totalElements: 50, pageNumber: 2);

        Assert.That(page.Content,           Is.EqualTo(items));
        Assert.That(page.TotalPages,         Is.EqualTo(5));
        Assert.That(page.TotalElements,      Is.EqualTo(50));
        Assert.That(page.PageNumber,         Is.EqualTo(2));
        Assert.That(page.PageSize,           Is.EqualTo(10));
        Assert.That(page.TotalPageElements,  Is.EqualTo(2));
        Assert.That(page.SortBy,             Is.EqualTo("id"));
        Assert.That(page.Direction,          Is.EqualTo("asc"));
    }

    [Test]
    [Description("Empty debe ser true cuando Content no tiene elementos")]
    public void Empty_WhenNoContent_ShouldBeTrue()
    {
        var page = MakePage(new List<string>(), totalPages: 0, totalElements: 0, pageNumber: 0);
        Assert.That(page.Empty, Is.True);
    }

    [Test]
    [Description("Empty debe ser false cuando Content tiene al menos un elemento")]
    public void Empty_WhenContentHasItems_ShouldBeFalse()
    {
        var page = MakePage(new List<string> { "item" }, totalPages: 1, totalElements: 1, pageNumber: 0);
        Assert.That(page.Empty, Is.False);
    }

    [Test]
    [Description("First debe ser true cuando PageNumber es 0")]
    public void First_WhenPageNumberIsZero_ShouldBeTrue()
    {
        var page = MakePage(new List<string> { "x" }, totalPages: 3, totalElements: 30, pageNumber: 0);
        Assert.That(page.First, Is.True);
    }

    [Test]
    [Description("First debe ser false cuando PageNumber es mayor que 0")]
    public void First_WhenPageNumberIsGreaterThanZero_ShouldBeFalse()
    {
        var page = MakePage(new List<string> { "x" }, totalPages: 3, totalElements: 30, pageNumber: 1);
        Assert.That(page.First, Is.False);
    }

    [Test]
    [Description("Last debe ser true cuando PageNumber >= TotalPages - 1")]
    public void Last_WhenOnLastPage_ShouldBeTrue()
    {
        var page = MakePage(new List<string> { "x" }, totalPages: 3, totalElements: 30, pageNumber: 2);
        Assert.That(page.Last, Is.True);
    }

    [Test]
    [Description("Last debe ser false cuando hay páginas posteriores")]
    public void Last_WhenNotOnLastPage_ShouldBeFalse()
    {
        var page = MakePage(new List<string> { "x" }, totalPages: 3, totalElements: 30, pageNumber: 1);
        Assert.That(page.Last, Is.False);
    }

    [Test]
    [Description("Last debe ser true cuando solo hay 1 página (TotalPages=1, PageNumber=0)")]
    public void Last_WhenOnlyOnePage_ShouldBeTrue()
    {
        var page = MakePage(new List<string> { "a", "b" }, totalPages: 1, totalElements: 2, pageNumber: 0);
        Assert.That(page.Last, Is.True);
    }

    [Test]
    [Description("Last debe ser true cuando PageNumber excede TotalPages - 1")]
    public void Last_WhenPageNumberExceedsTotalPages_ShouldBeTrue()
    {
        var page = MakePage(new List<string>(), totalPages: 2, totalElements: 0, pageNumber: 5);
        Assert.That(page.Last, Is.True);
    }

    [Test]
    [Description("PageResponseDto<int> debe funcionar igual que con strings")]
    public void PageResponseDto_WithIntType_ShouldWorkCorrectly()
    {
        var content = new List<int> { 1, 2, 3 };
        var page    = new PageResponseDto<int>(content, 2, 20, 10, 0, 3, "id", "asc");

        Assert.That(page.Content,       Has.Count.EqualTo(3));
        Assert.That(page.Empty,         Is.False);
        Assert.That(page.First,         Is.True);
        Assert.That(page.Last,          Is.False);
    }

    [Test]
    [Description("PageResponseDto con los mismos valores debe ser igual por valor (record semantics)")]
    public void PageResponseDto_WithSameValues_ShouldBeEqual()
    {
        var content = new List<string> { "a" };
        var a = MakePage(content, 1, 1, 0);
        var b = MakePage(content, 1, 1, 0);

        Assert.That(a, Is.EqualTo(b));
    }
}