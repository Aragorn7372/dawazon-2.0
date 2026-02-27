using System.ComponentModel.DataAnnotations;
using dawazon2._0.Models;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Models;

[TestFixture]
public class UserViewModelsTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Test]
    public void UserEditViewModel_ValidData_ShouldNotHaveValidationErrors()
    {
        var model = new UserEditViewModel
        {
            Nombre = "Juan Perez",
            Email = "juan@example.com",
            Telefono = "123456789",
            Calle = "Gran Via 1",
            Ciudad = "Madrid",
            CodigoPostal = "28001",
            Provincia = "Madrid"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void UserEditViewModel_MissingRequiredFields_ShouldHaveValidationErrors()
    {
        var model = new UserEditViewModel
        {
            Nombre = "",
            Email = "",
            Calle = "",
            Ciudad = "",
            CodigoPostal = "",
            Provincia = ""
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(results.Any(v => v.MemberNames.Contains("Nombre")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Email")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Calle")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Ciudad")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("CodigoPostal")), Is.True);
            Assert.That(results.Any(v => v.MemberNames.Contains("Provincia")), Is.True);
        });
    }

    [Test]
    public void UserEditViewModel_InvalidTelefono_ShouldHaveValidationError()
    {
        var model = new UserEditViewModel
        {
            Nombre = "Juan Perez",
            Email = "juan@example.com",
            Telefono = "123",
            Calle = "Gran Via 1",
            Ciudad = "Madrid",
            CodigoPostal = "28001",
            Provincia = "Madrid"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("Telefono")), Is.True);
    }

    [Test]
    public void UserFavsViewModel_PaginationLogic_ShouldBeCorrect()
    {
        var model = new UserFavsViewModel
        {
            PageNumber = 0,
            TotalPages = 3,
            TotalElements = 15
        };

        Assert.Multiple(() =>
        {
            Assert.That(model.First, Is.True, "Should be First page");
            Assert.That(model.Last, Is.False, "Should not be Last page");
            Assert.That(model.PrevPage, Is.EqualTo(0), "PrevPage from 0 should be 0");
            Assert.That(model.NextPage, Is.EqualTo(1), "NextPage from 0 should be 1");
            
            model.PageNumber = 1;
            Assert.That(model.First, Is.False, "Should not be First page");
            Assert.That(model.Last, Is.False, "Should not be Last page");
            Assert.That(model.PrevPage, Is.EqualTo(0), "PrevPage from 1 should be 0");
            Assert.That(model.NextPage, Is.EqualTo(2), "NextPage from 1 should be 2");

            model.PageNumber = 2;
            Assert.That(model.First, Is.False, "Should not be First page");
            Assert.That(model.Last, Is.True, "Should be Last page");
            Assert.That(model.PrevPage, Is.EqualTo(1), "PrevPage from 2 should be 1");
            Assert.That(model.NextPage, Is.EqualTo(2), "NextPage from 2 should be 2");
        });
    }

    [Test]
    public void UserProfileViewModel_AvatarUrl_DefaultBehavior()
    {
        var model = new UserProfileViewModel
        {
        };

        var avatarUrl = model.AvatarUrl;

        Assert.That(avatarUrl, Is.EqualTo("/uploads/users/default.png"));
    }

    [Test]
    public void UserProfileViewModel_AvatarUrl_CustomAvatar()
    {
        var model = new UserProfileViewModel
        {
            Avatar = "/custom/path/image.png"
        };
        var avatarUrl = model.AvatarUrl;

        Assert.That(avatarUrl, Is.EqualTo("/custom/path/image.png"));
    }

    [Test]
    public void UserProfileViewModel_AvatarUrl_NullOrWhitespace()
    {
        var model1 = new UserProfileViewModel { Avatar = "" };
        var model2 = new UserProfileViewModel { Avatar = "   " };
        var model3 = new UserProfileViewModel { Avatar = null! };
        Assert.Multiple(() =>
        {
            Assert.That(model1.AvatarUrl, Is.EqualTo("/uploads/users/default.png"));
            Assert.That(model2.AvatarUrl, Is.EqualTo("/uploads/users/default.png"));
            Assert.That(model3.AvatarUrl, Is.EqualTo("/uploads/users/default.png"));
        });
    }
}
