using System.ComponentModel.DataAnnotations;
using dawazon2._0.Models;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Models;

[TestFixture]
public class ProductViewModelsTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Test]
    public void ProductDetailViewModel_MainImage_ShouldReturnFirstImageOrPlaceholder()
    {
        var modelWithImages = new ProductDetailViewModel
        {
            Images = new List<string> { "image1.png", "image2.png" }
        };

        var modelWithoutImages = new ProductDetailViewModel
        {
            Images = new List<string>()
        };

        Assert.Multiple(() =>
        {
            Assert.That(modelWithImages.MainImage, Is.EqualTo("image1.png"));
            Assert.That(modelWithoutImages.MainImage, Is.EqualTo("placeholder.png"));
        });
    }

    [Test]
    public void ProductFormViewModel_ValidData_ShouldNotHaveValidationErrors()
    {
        var model = new ProductFormViewModel
        {
            Name = "Product 1",
            Price = 10.5,
            Category = "Electronics",
            Description = "Great product",
            Stock = 100
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void ProductFormViewModel_InvalidPriceAndStock_ShouldHaveValidationErrors()
    {
        var model = new ProductFormViewModel
        {
            Name = "Product 1",
            Price = 0,
            Category = "Electronics",
            Description = "Great product",
            Stock = -5
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(results.Any(v => v.MemberNames.Contains("Price")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Stock")), Is.True);
        });
    }

    [Test]
    public void ProductFormViewModel_IsEdit_ShouldReturnCorrectly()
    {
        var createModel = new ProductFormViewModel { Id = null };
        var editModel = new ProductFormViewModel { Id = "P1" };

        Assert.Multiple(() =>
        {
            Assert.That(createModel.IsEdit, Is.False);
            Assert.That(editModel.IsEdit, Is.True);
        });
    }

    [Test]
    public void ProductListViewModel_PaginationLogic_ShouldBeCorrect()
    {
        var model = new ProductListViewModel
        {
            PageNumber = 1,
            TotalPages = 3,
            TotalElements = 15
        };

        Assert.Multiple(() =>
        {
            Assert.That(model.First, Is.False);
            Assert.That(model.Last, Is.False);
            Assert.That(model.PrevPage, Is.EqualTo(0));
            Assert.That(model.NextPage, Is.EqualTo(2));
        });
    }

    [Test]
    public void ProductPageViewModel_Instantiation_SetsProperties()
    {
        var model = new ProductPageViewModel { TotalPages = 5, PageNumber = 2 };
        Assert.That(model.TotalPages, Is.EqualTo(5));
        Assert.That(model.Content, Is.Not.Null);
    }

    [Test]
    public void ProductSummaryViewModel_Instantiation_SetsProperties()
    {
        var model = new ProductSummaryViewModel { Id = "P1", Name = "Laptop", Price = 999.99 };
        Assert.That(model.Id, Is.EqualTo("P1"));
        Assert.That(model.Name, Is.EqualTo("Laptop"));
    }
}
