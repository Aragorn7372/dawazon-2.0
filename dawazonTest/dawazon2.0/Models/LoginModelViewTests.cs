using System.ComponentModel.DataAnnotations;
using dawazon2._0.Models;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Models;

[TestFixture]
public class LoginModelViewTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Test]
    public void LoginModelView_ValidData_ShouldNotHaveValidationErrors()
    {
        var model = new LoginModelView
        {
            UsernameOrEmail = "juanperez",
            Password = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void LoginModelView_MissingUsernameOrEmail_ShouldHaveValidationError()
    {
        var model = new LoginModelView
        {
            UsernameOrEmail = "",
            Password = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("UsernameOrEmail")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("UsernameOrEmail")).ErrorMessage, Is.EqualTo("El nombre de usuario es obligatorio"));
    }

    [Test]
    public void LoginModelView_MissingPassword_ShouldHaveValidationError()
    {
        var model = new LoginModelView
        {
            UsernameOrEmail = "juanperez",
            Password = ""
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("Password")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("Password")).ErrorMessage, Is.EqualTo("La contraseña es obligatoria"));
    }
}
