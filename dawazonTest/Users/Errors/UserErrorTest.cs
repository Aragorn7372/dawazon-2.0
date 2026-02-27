using dawazonBackend.Users.Errors;

namespace dawazonTest.Users.Errors;

[TestFixture]
[Description("Tests para UserError y sus clases derivadas")]
public class UserErrorTest
{
    [Test]
    [Description("UserNotFoundError: Debería retener el mensaje heredado de UserError")]
    public void UserNotFoundError_ShouldRetainMessage()
    {
        var msg = "User not found";
        var error = new UserNotFoundError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("UserUpdateError: Debería retener el mensaje y heredar de UserError")]
    public void UserUpdateError_ShouldRetainMessage()
    {
        var msg = "Update failed";
        var error = new UserUpdateError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("UserConflictError: Debería retener el mensaje heredado de UserError")]
    public void UserConflictError_ShouldRetainMessage()
    {
        var msg = "Conflict";
        var error = new UserConflictError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("UnauthorizedError: Debería retener el mensaje heredado de UserError")]
    public void UnauthorizedError_ShouldRetainMessage()
    {
        var msg = "Unauthorized";
        var error = new UnauthorizedError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("UserHasThatProductError: Debería retener el mensaje heredado de UserError")]
    public void UserHasThatProductError_ShouldRetainMessage()
    {
        var msg = "Has product";
        var error = new UserHasThatProductError(msg);

        Assert.That(error.Message, Is.EqualTo(msg));
        Assert.That(error, Is.InstanceOf<UserError>());
    }

    [Test]
    [Description("Records: Dos records iguales deberían equivaler")]
    public void RecordEquality_ShouldBeEqual()
    {
        var error1 = new UserNotFoundError("Same message");
        var error2 = new UserNotFoundError("Same message");

        Assert.That(error1, Is.EqualTo(error2));
        Assert.That(error1 == error2, Is.True);
    }
}
