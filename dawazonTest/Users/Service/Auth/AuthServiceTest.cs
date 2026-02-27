using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service.Auth;
using dawazonBackend.Users.Service.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using dawazonBackend.Cart.Repository;

namespace dawazonTest.Users.Service.Auth;

[TestFixture]
[Description("Tests for AuthService")]
public class AuthServiceTest
{
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<IJwtService> _jwtServiceMock;
    private Mock<ICartRepository> _cartRepositoryMock;
    private AuthService _authService;

    [SetUp]
    public void SetUp()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        _jwtServiceMock = new Mock<IJwtService>();

        _cartRepositoryMock = new Mock<ICartRepository>();
        _authService = new AuthService(new NullLogger<AuthService>(), _jwtServiceMock.Object, _userManagerMock.Object, _cartRepositoryMock.Object);
    }

    [Test]
    public async Task SignUpAsync_WhenSuccess_ShouldReturnToken()
    {
        var dto = new RegisterDto { Username = "newuser", Email = "new@example.com", Password = "Password1!" };
        
        _userManagerMock.Setup(um => um.FindByNameAsync(dto.Username)).ReturnsAsync((User)null!);
        
        var userFound = new User { Id = 1, Name = dto.Username, Email = dto.Email };

        _userManagerMock.SetupSequence(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User)null!) 
            .ReturnsAsync(userFound);  
        
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _jwtServiceMock.Setup(jwt => jwt.GenerateTokenAsync(userFound)).ReturnsAsync("jwt-token");

        var result = await _authService.SignUpAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Token, Is.EqualTo("jwt-token"));
        _userManagerMock.Verify(um => um.AddToRoleAsync(userFound, "User"), Times.Once);
    }

    [Test]
    public async Task SignUpAsync_WhenDuplicateEmail_ShouldReturnUserConflictError()
    {
        var dto = new RegisterDto { Username = "newuser", Email = "old@example.com", Password = "pwd" };
        var existingUser = new User { Email = "old@example.com" };

        _userManagerMock.Setup(um => um.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User)null!);
        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

        var result = await _authService.SignUpAsync(dto);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserConflictError>());
    }

    [Test]
    public async Task SignInAsync_WithValidEmailAndPassword_ShouldReturnToken()
    {
        var dto = new LoginDto { UsernameOrEmail = "user@example.com", Password = "pwd" };
        var user = new User { Id = 1, Email = "user@example.com" };

        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.UsernameOrEmail)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _jwtServiceMock.Setup(jwt => jwt.GenerateTokenAsync(user)).ReturnsAsync("valid-token");

        var result = await _authService.SignInAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Token, Is.EqualTo("valid-token"));
    }

    [Test]
    public async Task SignInAsync_WithInvalidPassword_ShouldReturnUnauthorizedError()
    {
        var dto = new LoginDto { UsernameOrEmail = "user@example.com", Password = "wrongpwd" };
        var user = new User { Id = 1, Email = "user@example.com" };

        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.UsernameOrEmail)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

        var result = await _authService.SignInAsync(dto);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UnauthorizedError>());
    }

    [Test]
    public async Task SignInAsync_WhenUserNotFound_ShouldReturnUnauthorizedError()
    {
        var dto = new LoginDto { UsernameOrEmail = "unknown", Password = "pwd" };

        _userManagerMock.Setup(um => um.FindByNameAsync(dto.UsernameOrEmail)).ReturnsAsync((User)null!);

        var result = await _authService.SignInAsync(dto);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UnauthorizedError>());
    }

    [Test]
    public async Task SignUpAsync_WhenDuplicateUsername_ShouldReturnUserConflictError()
    {
        var dto = new RegisterDto { Username = "existinguser", Email = "new@example.com", Password = "pwd" };
        var existingUser = new User { Name = "existinguser" };

        _userManagerMock.Setup(um => um.FindByNameAsync(dto.Username)).ReturnsAsync(existingUser);
        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync((User)null!);

        var result = await _authService.SignUpAsync(dto);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserConflictError>());
    }

    [Test]
    public async Task SignUpAsync_WhenCreateAsyncFails_ShouldReturnUserError()
    {
        var dto = new RegisterDto { Username = "newuser", Email = "new@example.com", Password = "weak" };

        _userManagerMock.Setup(um => um.FindByNameAsync(dto.Username)).ReturnsAsync((User)null!);
        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync((User)null!);

        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Password too weak" });
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(failedResult);

        var result = await _authService.SignUpAsync(dto);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserError>());
    }

    [Test]
    public async Task SignUpAsync_WhenUserNotFoundAfterCreate_ShouldReturnUserNotFoundError()
    {
        var dto = new RegisterDto { Username = "newuser", Email = "ghost@example.com", Password = "Password1!" };

        _userManagerMock.Setup(um => um.FindByNameAsync(dto.Username)).ReturnsAsync((User)null!);
        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email)).ReturnsAsync((User)null!);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Even after CreateAsync succeeds, FindByEmailAsync still returns null
        // (already set up above as null)

        var result = await _authService.SignUpAsync(dto);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<UserNotFoundError>());
    }

    [Test]
    public async Task SignInAsync_WithUsernameAndValidPassword_ShouldReturnToken()
    {
        var dto = new LoginDto { UsernameOrEmail = "validuser", Password = "pwd" };
        var user = new User { Id = 1, Name = "validuser" };

        _userManagerMock.Setup(um => um.FindByNameAsync(dto.UsernameOrEmail)).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _jwtServiceMock.Setup(jwt => jwt.GenerateTokenAsync(user)).ReturnsAsync("token-by-name");

        var result = await _authService.SignInAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Token, Is.EqualTo("token-by-name"));
    }
}
