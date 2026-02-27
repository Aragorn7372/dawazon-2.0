using System.ComponentModel.DataAnnotations;
using dawazon2._0.Models;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Models;

[TestFixture]
public class AddCommentViewModelTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Test]
    public void AddCommentViewModel_ValidData_ShouldNotHaveValidationErrors()
    {
        var model = new AddCommentViewModel
        {
            ProductId = "PROD-1",
            CommentText = "This is a great product!",
            Recommended = true
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void AddCommentViewModel_MissingCommentText_ShouldHaveValidationError()
    {
        var model = new AddCommentViewModel
        {
            ProductId = "PROD-1",
            CommentText = "",
            Recommended = true
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Any(v => v.MemberNames.Contains("CommentText")), Is.True);
    }
}
