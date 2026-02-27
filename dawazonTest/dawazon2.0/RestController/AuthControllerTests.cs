using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using dawazon2._0.RestControllers;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Service.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.RestController;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _authServiceMock;
    private Mock<ILogger<AuthController>> _loggerMock;
    private AuthController _controller;

    [SetUp]
    public void SetUp()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_loggerMock.Object, _authServiceMock.Object);
    }

    [Test]
    public void Constructor_ShouldInitializeSuccessfully()
    {
        Assert.DoesNotThrow(() =>
        {
            var controller = new AuthController(_loggerMock.Object, _authServiceMock.Object);
            Assert.That(controller, Is.Not.Null);
        });
    }

    [Test]
    public async Task Login_ReturnsOk_WhenSignInSucceeds()
    {
        var dto = new LoginDto { UsernameOrEmail = "testuser", Password = "Password123!" };
        var authResponse = new AuthResponseDto("fake-token");
        _authServiceMock
            .Setup(s => s.SignInAsync(dto))
            .ReturnsAsync(Result.Success<AuthResponseDto, UserError>(authResponse));

        var result = await _controller.Login(dto);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(authResponse));
    }

    [Test]
    public async Task Login_ReturnsNotFound_WhenUserNotFound()
    {
        var dto = new LoginDto { UsernameOrEmail = "wronguser", Password = "Password123!" };
        _authServiceMock
            .Setup(s => s.SignInAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, UserError>(new UserNotFoundError("User not found")));

        var result = await _controller.Login(dto);

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task Login_ReturnsConflict_WhenUserConflict()
    {
        var dto = new LoginDto { UsernameOrEmail = "testuser", Password = "Password123!" };
        _authServiceMock
            .Setup(s => s.SignInAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, UserError>(new UserConflictError("Conflict")));

        var result = await _controller.Login(dto);

        var conflictResult = result as ConflictObjectResult;
        Assert.That(conflictResult, Is.Not.Null);
    }

    [Test]
    public async Task Login_ReturnsUnauthorized_WhenUnauthorized()
    {
        var dto = new LoginDto { UsernameOrEmail = "testuser", Password = "wrongpassword" };
        _authServiceMock
            .Setup(s => s.SignInAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, UserError>(new UnauthorizedError("Invalid credentials")));

        var result = await _controller.Login(dto);

        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null);
    }

    [Test]
    public async Task Login_ReturnsBadRequest_WhenOtherError()
    {
        var dto = new LoginDto { UsernameOrEmail = "testuser", Password = "Password123!" };
        _authServiceMock
            .Setup(s => s.SignInAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, UserError>(new UserError("Generic error")));

        var result = await _controller.Login(dto);

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
    }

    [Test]
    public async Task Register_ReturnsOk_WhenSignUpSucceeds()
    {
        var dto = new RegisterDto { Username = "newuser", Password = "Password123!", Email = "test@ex.com" };
        var authResponse = new AuthResponseDto("fake-token");
        _authServiceMock
            .Setup(s => s.SignUpAsync(dto))
            .ReturnsAsync(Result.Success<AuthResponseDto, UserError>(authResponse));

        var result = await _controller.Register(dto);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(authResponse));
    }

    [Test]
    public async Task Register_ReturnsConflict_WhenUserConflict()
    {
        var dto = new RegisterDto { Username = "existinguser", Password = "Password123!", Email = "test@ex.com" };
        _authServiceMock
            .Setup(s => s.SignUpAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, UserError>(new UserConflictError("User exists")));

        var result = await _controller.Register(dto);

        var conflictResult = result as ConflictObjectResult;
        Assert.That(conflictResult, Is.Not.Null);
    }

    [Test]
    public async Task Register_ReturnsUnauthorized_WhenUnauthorized()
    {
        var dto = new RegisterDto { Username = "newuser", Password = "Password123!", Email = "test@ex.com" };
        _authServiceMock
            .Setup(s => s.SignUpAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, UserError>(new UnauthorizedError("Unauthorized")));

        var result = await _controller.Register(dto);

        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null);
    }

    [Test]
    public async Task Register_ReturnsBadRequest_WhenOtherError()
    {
        var dto = new RegisterDto { Username = "newuser", Password = "Password123!", Email = "test@ex.com" };
        _authServiceMock
            .Setup(s => s.SignUpAsync(dto))
            .ReturnsAsync(Result.Failure<AuthResponseDto, UserError>(new UserError("Generic error")));

        var result = await _controller.Register(dto);

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
    }
}
