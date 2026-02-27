using System.ComponentModel.DataAnnotations;
using dawazon2._0.Models;
using dawazonBackend.Cart.Models;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Models;

[TestFixture]
public class CartViewModelsTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Test]
    public void CartLineViewModel_StatusLabelAndBadge_ShouldReturnExpectedValues()
    {
        Assert.Multiple(() =>
        {
            var line1 = new CartLineViewModel { Status = Status.EnCarrito };
            Assert.That(line1.StatusLabel, Is.EqualTo("En carrito"));
            Assert.That(line1.StatusBadgeClass, Is.EqualTo("bg-secondary"));

            var line2 = new CartLineViewModel { Status = Status.Preparado };
            Assert.That(line2.StatusLabel, Is.EqualTo("Preparado"));
            Assert.That(line2.StatusBadgeClass, Is.EqualTo("bg-warning text-dark"));

            var line3 = new CartLineViewModel { Status = Status.Enviado };
            Assert.That(line3.StatusLabel, Is.EqualTo("Enviado"));
            Assert.That(line3.StatusBadgeClass, Is.EqualTo("bg-info text-dark"));

            var line4 = new CartLineViewModel { Status = Status.Recibido };
            Assert.That(line4.StatusLabel, Is.EqualTo("Recibido"));
            Assert.That(line4.StatusBadgeClass, Is.EqualTo("bg-success"));

            var line5 = new CartLineViewModel { Status = Status.Cancelado };
            Assert.That(line5.StatusLabel, Is.EqualTo("Cancelado"));
            Assert.That(line5.StatusBadgeClass, Is.EqualTo("bg-danger"));

            var line6 = new CartLineViewModel { Status = (Status)999 };
            Assert.That(line6.StatusLabel, Is.EqualTo("999"));
            Assert.That(line6.StatusBadgeClass, Is.EqualTo("bg-secondary"));
        });
    }

    [Test]
    public void CartOrderListViewModel_PaginationLogic_ShouldBeCorrect()
    {
        var model = new CartOrderListViewModel
        {
            PageNumber = 0,
            TotalPages = 3,
            TotalElements = 15
        };

        Assert.Multiple(() =>
        {
            Assert.That(model.First, Is.True);
            Assert.That(model.Last, Is.False);
            Assert.That(model.PrevPage, Is.EqualTo(0));
            Assert.That(model.NextPage, Is.EqualTo(1));
            
            model.PageNumber = 1;
            Assert.That(model.First, Is.False);
            Assert.That(model.Last, Is.False);
            Assert.That(model.PrevPage, Is.EqualTo(0));
            Assert.That(model.NextPage, Is.EqualTo(2));

            model.PageNumber = 2;
            Assert.That(model.First, Is.False);
            Assert.That(model.Last, Is.True);
            Assert.That(model.PrevPage, Is.EqualTo(1));
            Assert.That(model.NextPage, Is.EqualTo(2));
        });
    }

    [Test]
    public void CheckoutViewModel_ValidData_ShouldNotHaveValidationErrors()
    {
        var model = new CheckoutViewModel
        {
            Name = "Juan Perez",
            Email = "juan@example.com",
            Phone = "123456789",
            Street = "Gran Via",
            Number = 10,
            City = "Madrid",
            Province = "Madrid",
            Country = "España",
            PostalCode = 28001
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void CheckoutViewModel_MissingRequiredFields_ShouldHaveValidationErrors()
    {
        var model = new CheckoutViewModel
        {
            Name = "",
            Email = "",
            Phone = "",
            Street = "",
            Number = 0,
            City = "",
            Province = "",
            Country = "",
            PostalCode = 0
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(results.Any(v => v.MemberNames.Contains("Name")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Email")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Phone")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Street")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Number")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("City")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Province")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Country")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("PostalCode")), Is.True);
        });
    }

    [Test]
    public void CheckoutViewModel_InvalidRangesAndFormats_ShouldHaveValidationErrors()
    {
        var model = new CheckoutViewModel
        {
            Name = "Juan Perez",
            Email = "no_es_un_correo",
            Phone = "123",
            Street = "Gran Via",
            Number = 100000,
            City = "Madrid",
            Province = "Madrid",
            Country = "España",
            PostalCode = 999
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(results.Any(v => v.MemberNames.Contains("Email")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Phone")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Number")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("PostalCode")), Is.True);
        });
    }

    [Test]
    public void CartOrderDetailViewModel_Instantiation_SetsProperties()
    {
        var model = new CartOrderDetailViewModel { Id = "C1", Total = 10.5 };
        Assert.That(model.Id, Is.EqualTo("C1"));
        Assert.That(model.Total, Is.EqualTo(10.5));
    }

    [Test]
    public void CartOrderSummaryViewModel_Instantiation_SetsProperties()
    {
        var model = new CartOrderSummaryViewModel { Id = "S1", Total = 20.0 };
        Assert.That(model.Id, Is.EqualTo("S1"));
        Assert.That(model.Total, Is.EqualTo(20.0));
    }
}
