using dawazonBackend.Products.Mapper;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;
using NUnit.Framework;

namespace dawazonTest.Products.Mapper;

[TestFixture]
[Description("ProductMapper Unit Tests — SOLID + FIRST Principles")]
public class ProductMapperTest
{

    [Test]
    [Description("ToDto: Debe mapear todos los campos básicos del Product")]
    public void ToDto_ShouldMapAllBasicFields()
    {
        var category = new Category { Id = "CAT001", Name = "Electrónica" };
        var product = new Product
        {
            Id = "PRD001",
            Name = "Producto Test",
            Price = 29.99,
            Stock = 50,
            CategoryId = "CAT001",
            Category = category,
            Description = "Descripción del producto",
            Images = ["img1.jpg", "img2.jpg"],
            Comments = []
        };

        var dto = product.ToDto();

        Assert.That(dto.Id, Is.EqualTo("PRD001"));
        Assert.That(dto.Name, Is.EqualTo("Producto Test"));
        Assert.That(dto.Price, Is.EqualTo(29.99));
        Assert.That(dto.Stock, Is.EqualTo(50));
        Assert.That(dto.Category, Is.EqualTo("Electrónica"));
        Assert.That(dto.Description, Is.EqualTo("Descripción del producto"));
        Assert.That(dto.Images, Has.Count.EqualTo(2));
        Assert.That(dto.Comments, Is.Empty);
    }

    [Test]
    [Description("ToDto: Cuando Id es null debe usar string.Empty")]
    public void ToDto_WhenIdIsNull_ShouldUseEmptyString()
    {
        var product = new Product { Id = null, CategoryId = "CAT001" };

        var dto = product.ToDto();

        Assert.That(dto.Id, Is.EqualTo(string.Empty));
    }

    [Test]
    [Description("ToDto: Cuando Category es null debe usar CategoryId como fallback")]
    public void ToDto_WhenCategoryIsNull_ShouldFallbackToCategoryId()
    {
        var product = new Product { Id = "PRD001", Category = null, CategoryId = "CAT_FALLBACK" };

        var dto = product.ToDto();

        Assert.That(dto.Category, Is.EqualTo("CAT_FALLBACK"));
    }

    [Test]
    [Description("ToDto: Debe mapear los comentarios usando Comment.ToDto()")]
    public void ToDto_ShouldMapComments()
    {
        var product = new Product
        {
            Id = "PRD001",
            CategoryId = "CAT001",
            Comments =
            [
                new Comment { UserId = 1, Content = "Buen producto", recommended = true, verified = true },
                new Comment { UserId = 2, Content = "Regular", recommended = false, verified = false }
            ]
        };

        var dto = product.ToDto();

        Assert.That(dto.Comments, Has.Count.EqualTo(2));
        Assert.That(dto.Comments[0].comment, Is.EqualTo("Buen producto"));
        Assert.That(dto.Comments[0].userName, Is.EqualTo("1"));
        Assert.That(dto.Comments[0].recommended, Is.True);
        Assert.That(dto.Comments[0].verified, Is.True);
        Assert.That(dto.Comments[1].comment, Is.EqualTo("Regular"));
        Assert.That(dto.Comments[1].recommended, Is.False);
    }

    [Test]
    [Description("ToDto: Debe preservar la lista de imágenes vacía")]
    public void ToDto_WhenNoImages_ShouldReturnEmptyList()
    {
        var product = new Product { Id = "PRD001", CategoryId = "CAT001", Images = [] };

        var dto = product.ToDto();

        Assert.That(dto.Images, Is.Empty);
    }

    [Test]
    [Description("Comment.ToDto: Debe mapear UserId a userName como string")]
    public void CommentToDto_ShouldMapUserIdToString()
    {
        var comment = new Comment { UserId = 42, Content = "test", recommended = true, verified = false };

        var dto = comment.ToDto();

        Assert.That(dto.userName, Is.EqualTo("42"));
    }

    [Test]
    [Description("Comment.ToDto: Debe mapear Content a comment")]
    public void CommentToDto_ShouldMapContentToComment()
    {
        var comment = new Comment { UserId = 1, Content = "Excelente calidad", recommended = true, verified = true };

        var dto = comment.ToDto();

        Assert.That(dto.comment, Is.EqualTo("Excelente calidad"));
        Assert.That(dto.recommended, Is.True);
        Assert.That(dto.verified, Is.True);
    }

    [Test]
    [Description("ToModel: Debe mapear los campos básicos del DTO al modelo")]
    public void ToModel_ShouldMapBasicFields()
    {
        var dto = new ProductRequestDto(
            Id: "PRD001",
            Name: "Nuevo Producto",
            Price: 49.99,
            Category: "Electrónica",
            Description: "Descripción test",
            Images: ["img1.jpg"],
            Stock: 100,
            CreatorId: 5
        );

        var model = dto.ToModel();

        Assert.That(model.Id, Is.EqualTo("PRD001"));
        Assert.That(model.Name, Is.EqualTo("Nuevo Producto"));
        Assert.That(model.Price, Is.EqualTo(49.99));
        Assert.That(model.Stock, Is.EqualTo(100));
        Assert.That(model.Description, Is.EqualTo("Descripción test"));
        Assert.That(model.CreatorId, Is.EqualTo(5));
        Assert.That(model.Images, Has.Count.EqualTo(1));
        Assert.That(model.IsDeleted, Is.False);
        Assert.That(model.Comments, Is.Empty);
    }

    [Test]
    [Description("ToModel: CategoryId siempre se establece a string.Empty (se resuelve en el servicio)")]
    public void ToModel_ShouldSetCategoryIdToEmpty()
    {
        var dto = new ProductRequestDto("id", "name", 10, "Cat", "desc", null, 1, null);

        var model = dto.ToModel();

        Assert.That(model.CategoryId, Is.EqualTo(string.Empty));
    }

    [Test]
    [Description("ToModel: Cuando Images es null debe usar lista vacía")]
    public void ToModel_WhenImagesNull_ShouldUseEmptyList()
    {
        var dto = new ProductRequestDto("id", "name", 10, "Cat", "desc", null, 1, null);

        var model = dto.ToModel();

        Assert.That(model.Images, Is.Empty);
    }

    [Test]
    [Description("ToModel: Cuando CreatorId es null debe usar 0")]
    public void ToModel_WhenCreatorIdNull_ShouldUseZero()
    {
        var dto = new ProductRequestDto("id", "name", 10, "Cat", "desc", null, 1, null);

        var model = dto.ToModel();

        Assert.That(model.CreatorId, Is.EqualTo(0));
    }

    [Test]
    [Description("ToModel: Cuando Id es null debe mantenerlo como null")]
    public void ToModel_WhenIdNull_ShouldKeepNull()
    {
        var dto = new ProductRequestDto(null, "name", 10, "Cat", "desc", null, 1, null);

        var model = dto.ToModel();

        Assert.That(model.Id, Is.Null);
    }

    [Test]
    [Description("Copy: Sin parámetros debe crear una copia idéntica")]
    public void Copy_WithNoOverrides_ShouldCreateIdenticalCopy()
    {
        var original = new ProductRequestDto("PRD001", "Nombre", 25.0, "Cat", "Desc",
            ["img.jpg"], 10, 1);

        var copy = original.Copy();

        Assert.That(copy.Id, Is.EqualTo(original.Id));
        Assert.That(copy.Name, Is.EqualTo(original.Name));
        Assert.That(copy.Price, Is.EqualTo(original.Price));
        Assert.That(copy.Category, Is.EqualTo(original.Category));
        Assert.That(copy.Description, Is.EqualTo(original.Description));
        Assert.That(copy.Images, Is.EqualTo(original.Images));
        Assert.That(copy.Stock, Is.EqualTo(original.Stock));
        Assert.That(copy.CreatorId, Is.EqualTo(original.CreatorId));
    }

    [Test]
    [Description("Copy: Debe sobrescribir solo los campos proporcionados")]
    public void Copy_WithSomeOverrides_ShouldOverrideOnlyThoseFields()
    {
        var original = new ProductRequestDto("PRD001", "Nombre", 25.0, "Cat", "Desc",
            ["img.jpg"], 10, 1);

        var copy = original.Copy(Name: "Nuevo Nombre", Price: 99.99);

        Assert.That(copy.Name, Is.EqualTo("Nuevo Nombre"));
        Assert.That(copy.Price, Is.EqualTo(99.99));
        Assert.That(copy.Id, Is.EqualTo("PRD001"));
        Assert.That(copy.Category, Is.EqualTo("Cat"));
        Assert.That(copy.Description, Is.EqualTo("Desc"));
        Assert.That(copy.Stock, Is.EqualTo(10));
    }

    [Test]
    [Description("Copy: Debe poder sobrescribir todos los campos")]
    public void Copy_WithAllOverrides_ShouldOverrideAllFields()
    {
        var original = new ProductRequestDto("PRD001", "Nombre", 25.0, "Cat", "Desc",
            ["img.jpg"], 10, 1);

        var copy = original.Copy(
            Id: "PRD999",
            Name: "Otro",
            Price: 50.0,
            Category: "OtraCat",
            Description: "OtraDesc",
            Images: ["new.png"],
            Stock: 999,
            CreatorId: 77
        );

        Assert.That(copy.Id, Is.EqualTo("PRD999"));
        Assert.That(copy.Name, Is.EqualTo("Otro"));
        Assert.That(copy.Price, Is.EqualTo(50.0));
        Assert.That(copy.Category, Is.EqualTo("OtraCat"));
        Assert.That(copy.Description, Is.EqualTo("OtraDesc"));
        Assert.That(copy.Images, Has.Count.EqualTo(1));
        Assert.That(copy.Images![0], Is.EqualTo("new.png"));
        Assert.That(copy.Stock, Is.EqualTo(999));
        Assert.That(copy.CreatorId, Is.EqualTo(77));
    }
}
