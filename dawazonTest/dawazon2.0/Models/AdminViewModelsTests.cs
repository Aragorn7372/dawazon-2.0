using System.ComponentModel.DataAnnotations;
using dawazon2._0.Models;
using dawazonBackend.Cart.Models;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Models;

[TestFixture]
public class AdminViewModelsTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Test]
    public void AdminSaleEditViewModel_ValidData_ShouldNotHaveValidationErrors()
    {
        var model = new AdminSaleEditViewModel
        {
            SaleId = "S1",
            NewStatus = Status.Enviado
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void AdminSaleEditViewModel_MissingNewStatus_ShouldHaveValidationError()
    {
        var model = new AdminSaleEditViewModel
        {
            SaleId = "S1"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void AdminSaleListViewModel_Instantiation_SetsProperties()
    {
        var model = new AdminSaleListViewModel
        {
            PageNumber = 1,
            TotalPages = 5,
            TotalElements = 50,
            PageSize = 10,
            TotalEarnings = 1500.50
        };

        Assert.Multiple(() =>
        {
            Assert.That(model.PageNumber, Is.EqualTo(1));
            Assert.That(model.TotalPages, Is.EqualTo(5));
            Assert.That(model.TotalElements, Is.EqualTo(50));
            Assert.That(model.PageSize, Is.EqualTo(10));
            Assert.That(model.TotalEarnings, Is.EqualTo(1500.50));
            Assert.That(model.Sales, Is.Not.Null);
            Assert.That(model.Sales, Is.Empty);
        });
    }

    [Test]
    public void AdminUserDetailViewModel_Instantiation_SetsProperties()
    {
        var model = new AdminUserDetailViewModel();

        Assert.That(model.User, Is.Not.Null);
    }

    [Test]
    public void AdminUserListViewModel_Instantiation_SetsProperties()
    {
        var model = new AdminUserListViewModel
        {
            PageNumber = 2,
            TotalPages = 10,
            TotalElements = 100,
            PageSize = 10
        };

        Assert.Multiple(() =>
        {
            Assert.That(model.PageNumber, Is.EqualTo(2));
            Assert.That(model.TotalPages, Is.EqualTo(10));
            Assert.That(model.TotalElements, Is.EqualTo(100));
            Assert.That(model.PageSize, Is.EqualTo(10));
            Assert.That(model.Users, Is.Not.Null);
            Assert.That(model.Users, Is.Empty);
        });
    }
}
