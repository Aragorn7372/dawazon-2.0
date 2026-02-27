using System.ComponentModel.DataAnnotations;
using dawazon2._0.Models;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Models;

[TestFixture]
public class RegisterModelViewTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Test]
    public void RegisterModelView_ValidData_ShouldNotHaveValidationErrors()
    {
        var model = new RegisterModelView
        {
            Username = "juan_perez",
            Email = "juan@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void RegisterModelView_MissingUsername_ShouldHaveValidationError()
    {
        var model = new RegisterModelView
        {
            Username = "",
            Email = "juan@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("Username")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("Username")).ErrorMessage, Is.EqualTo("El nombre de usuario es obligatorio"));
    }

    [Test]
    public void RegisterModelView_UsernameTooShort_ShouldHaveValidationError()
    {
        var model = new RegisterModelView
        {
            Username = "ab",
            Email = "juan@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("Username")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("Username")).ErrorMessage, Is.EqualTo("Mínimo 3 caracteres"));
    }

    [Test]
    public void RegisterModelView_UsernameInvalidCharacters_ShouldHaveValidationError()
    {
        var model = new RegisterModelView
        {
            Username = "juan perez!",
            Email = "juan@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("Username")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("Username")).ErrorMessage, Is.EqualTo("Solo letras, números y guiones bajos"));
    }

    [Test]
    public void RegisterModelView_MissingEmail_ShouldHaveValidationError()
    {
        var model = new RegisterModelView
        {
            Username = "juan_perez",
            Email = "",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("Email")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("Email")).ErrorMessage, Is.EqualTo("El correo electrónico es obligatorio"));
    }

    [Test]
    public void RegisterModelView_InvalidEmail_ShouldHaveValidationError()
    {
        var model = new RegisterModelView
        {
            Username = "juan_perez",
            Email = "juan_at_example_com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("Email")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("Email")).ErrorMessage, Is.EqualTo("Correo inválido"));
    }

    [Test]
    public void RegisterModelView_MissingPassword_ShouldHaveValidationError()
    {
        var model = new RegisterModelView
        {
            Username = "juan_perez",
            Email = "juan@example.com",
            Password = "",
            ConfirmPassword = "Password123!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("Password")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("Password")).ErrorMessage, Is.EqualTo("La contraseña es obligatoria"));
    }

    [Test]
    public void RegisterModelView_PasswordTooShort_ShouldHaveValidationError()
    {
        var model = new RegisterModelView
        {
            Username = "juan_perez",
            Email = "juan@example.com",
            Password = "123",
            ConfirmPassword = "123"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("Password")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("Password")).ErrorMessage, Is.EqualTo("Mínimo 6 caracteres"));
    }

    [Test]
    public void RegisterModelView_MissingConfirmPassword_ShouldHaveValidationError()
    {
        var model = new RegisterModelView
        {
            Username = "juan_perez",
            Email = "juan@example.com",
            Password = "Password123!",
            ConfirmPassword = ""
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("ConfirmPassword")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("ConfirmPassword")).ErrorMessage, Is.EqualTo("Debes confirmar la contraseña"));
    }

    [Test]
    public void RegisterModelView_PasswordsDoNotMatch_ShouldHaveValidationError()
    {
        var model = new RegisterModelView
        {
            Username = "juan_perez",
            Email = "juan@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("ConfirmPassword")), Is.True);
        Assert.That(results.First(v => v.MemberNames.Contains("ConfirmPassword")).ErrorMessage, Is.EqualTo("Las contraseñas no coinciden"));
    }
}
