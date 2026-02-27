using dawazonBackend.Users.Errors;

namespace dawazonTest.Users.Errors;

[TestFixture]
[Description("Tests for UserError and its derived records")]
public class UserErrorTest
{
    [Test]
    [Description("UserNotFoundError: Should retain message and inherit from UserError")]
    public void UserNotFoundError_ShouldRetainMessage()
    {
        var msg = "User not found";
        var error = new UserNotFoundError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("UserUpdateError: Should retain message and inherit from UserError")]
    public void UserUpdateError_ShouldRetainMessage()
    {
        var msg = "Update failed";
        var error = new UserUpdateError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("UserConflictError: Should retain message and inherit from UserError")]
    public void UserConflictError_ShouldRetainMessage()
    {
        var msg = "Conflict";
        var error = new UserConflictError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("UnauthorizedError: Should retain message and inherit from UserError")]
    public void UnauthorizedError_ShouldRetainMessage()
    {
        var msg = "Unauthorized";
        var error = new UnauthorizedError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("UserHasThatProductError: Should retain message and inherit from UserError")]
    public void UserHasThatProductError_ShouldRetainMessage()
    {
        var msg = "Has product";
        var error = new UserHasThatProductError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("Records: Two equal records should be considered equal")]
    public void RecordEquality_ShouldBeEqual()
    {
        var error1 = new UserNotFoundError("Same message");
        var error2 = new UserNotFoundError("Same message");

        Assert.That(error1, Is.EqualTo(error2));
        Assert.That(error1 == error2, Is.True);
    }
}
