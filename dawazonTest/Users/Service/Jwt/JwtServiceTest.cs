using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace dawazonTest.Users.Service.Jwt;

[TestFixture]
[Description("Tests for JwtService")]
public class JwtServiceTest
{
    private Mock<IConfiguration> _configMock;
    private Mock<UserManager<User>> _userManagerMock;
    private JwtService _jwtService;
    
    private const string SecretKey = "TestSuperSecretKeyForJwtTokenGenerationMustBeLongEnough";

    [SetUp]
    public void SetUp()
    {
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["Jwt:Key"]).Returns(SecretKey);
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("60");

        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

        _jwtService = new JwtService(_configMock.Object, new NullLogger<JwtService>(), _userManagerMock.Object);
    }

    [Test]
    public async Task GenerateTokenAsync_ShouldReturnValidJwtToken()
    {
        var user = new User { Id = 1, Name = "testuser", Email = "test@example.com" };
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Usuario" });

        var tokenString = await _jwtService.GenerateTokenAsync(user);

        Assert.That(tokenString, Is.Not.Null.Or.Empty);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(tokenString);

        Assert.That(jwtToken.Issuer, Is.EqualTo("TestIssuer"));
        Assert.That(jwtToken.Audiences.First(), Is.EqualTo("TestAudience"));
        
        var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        Assert.That(nameClaim, Is.EqualTo("testuser"));
        Assert.That(roleClaim, Is.EqualTo("Usuario"));
    }

    [Test]
    public async Task ValidateToken_WithValidToken_ShouldReturnUsername()
    {
        var user = new User { Id = 1, Name = "validuser", Email = "valid@example.com" };
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        var token = await _jwtService.GenerateTokenAsync(user);

        var username = _jwtService.ValidateToken(token);

        Assert.That(username, Is.EqualTo("validuser"));
    }

    [Test]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        var invalidToken = "ey.invalid.token";

        var result = _jwtService.ValidateToken(invalidToken);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        _configMock.Setup(c => c["Jwt:ExpireMinutes"]).Returns("-5");
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: new[] { new Claim(JwtRegisteredClaimNames.Name, "expireduser") },
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var result = _jwtService.ValidateToken(tokenString);

        Assert.That(result, Is.Null);
    }
}
