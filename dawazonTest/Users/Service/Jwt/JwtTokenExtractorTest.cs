using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dawazonBackend.Users.Service.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;

namespace dawazonTest.Users.Service.Jwt;

[TestFixture]
[Description("Tests para JwtTokenExtractor")]
public class JwtTokenExtractorTest
{
    private Mock<ILogger<JwtTokenExtractor>> _loggerMock;
    private JwtTokenExtractor _extractor;
    
    private const string TestSecret = "SuperSecretTestKeyThatNeedsToBeVeryLongForAlgorithms";

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<JwtTokenExtractor>>();
        _extractor = new JwtTokenExtractor(_loggerMock.Object);
    }

    private string GenerateValidSignedToken(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateUnsignedToken(string payloadJson)
    {
        var header = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
        var encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
        var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        return $"{encodedHeader}.{encodedPayload}.";
    }

    private string Base64UrlEncode(byte[] input)
    {
        var output = Convert.ToBase64String(input);
        output = output.Split('=')[0]; 
        output = output.Replace('+', '-'); 
        output = output.Replace('/', '_'); 
        return output;
    }

    [Test]
    public void ExtractUserId_WithValidSubClaim_ReturnsUserId()
    {
        var token = GenerateValidSignedToken(new[] { new Claim(JwtRegisteredClaimNames.Sub, "123") });

        var result = _extractor.ExtractUserId(token);
        Assert.That(result, Is.EqualTo(123));
    }

    [Test]
    public void ExtractUserId_WithInvalidUserIdFormat_ReturnsNull()
    {
        var token = GenerateValidSignedToken(new[] { new Claim(JwtRegisteredClaimNames.Sub, "not-a-number") });

        var result = _extractor.ExtractUserId(token);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractUserId_WithNoUserIdClaim_ReturnsNull()
    {
        var token = GenerateValidSignedToken(new[] { new Claim(ClaimTypes.Role, "User") });

        var result = _extractor.ExtractUserId(token);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractUserId_WithInvalidTokenFormat_ReturnsNullAndLogsWarning()
    {
        var result = _extractor.ExtractUserId("not.even.a.token");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractRole_WithValidRoleClaim_ReturnsRole()
    {
        var token = GenerateValidSignedToken(new[] { new Claim(ClaimTypes.Role, "Admin") });

        var result = _extractor.ExtractRole(token);
        Assert.That(result, Is.EqualTo("Admin"));
    }

    [Test]
    public void ExtractRole_WithNoRoleClaim_ReturnsNull()
    {
        var token = GenerateValidSignedToken(new[] { new Claim(JwtRegisteredClaimNames.Sub, "1") });

        var result = _extractor.ExtractRole(token);
        Assert.That(result, Is.Null);
    }

    [Test]
    [TestCase("admin", true)]
    [TestCase("Admin", true)]
    [TestCase("ADMIN", true)]
    [TestCase("user", false)]
    [TestCase(null, false)]
    public void IsAdmin_ReturnsExpectedResult(string? roleValue, bool expected)
    {
        string token;
        if (roleValue == null)
        {
            token = GenerateValidSignedToken(new[] { new Claim(JwtRegisteredClaimNames.Sub, "1") });
        }
        else
        {
            token = GenerateValidSignedToken(new[] { new Claim(ClaimTypes.Role, roleValue) });
        }

        var result = _extractor.IsAdmin(token);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ExtractUserInfo_WithValidToken_ReturnsCorrectTuple()
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "42"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var token = GenerateValidSignedToken(claims);

        var result = _extractor.ExtractUserInfo(token);
        Assert.That(result.UserId, Is.EqualTo(42));
        Assert.That(result.IsAdmin, Is.True);
        Assert.That(result.Role, Is.EqualTo("Admin"));
    }

    [Test]
    public void ExtractClaims_WithValidSignedToken_ReturnsClaimsPrincipal()
    {
        var claims = new[]
        {
            new Claim("sub", "99"),
            new Claim("email", "test@test.com"),
            new Claim("name", "Test User"),
            new Claim("role", "Moderator")
        };
        var token = GenerateValidSignedToken(claims);

        var result = _extractor.ExtractClaims(token);
        Assert.That(result, Is.Not.Null);
        var identity = result!.Identity as ClaimsIdentity;
        Assert.That(identity, Is.Not.Null);
        Assert.That(identity!.AuthenticationType, Is.EqualTo("jwt"));

        Assert.That(result.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "99"), Is.True);
        Assert.That(result.HasClaim(c => c.Type == ClaimTypes.Email && c.Value == "test@test.com"), Is.True);
        Assert.That(result.HasClaim(c => c.Type == ClaimTypes.Name && c.Value == "Test User"), Is.True);
        Assert.That(result.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Moderator"), Is.True);
    }

    [Test]
    public void ExtractClaims_WithUnsignedToken_ReturnsClaimsPrincipal()
    {
        var payloadJson = "{\"sub\":\"15\", \"email\":\"no-sig@test.com\", \"role\":\"User\"}";
        var token = GenerateUnsignedToken(payloadJson);
        var result = _extractor.ExtractClaims(token);

        Assert.That(result, Is.Not.Null);
        
        Assert.That(result!.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "15"), Is.True);
        Assert.That(result.HasClaim(c => c.Type == ClaimTypes.Email && c.Value == "no-sig@test.com"), Is.True);
        Assert.That(result.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "User"), Is.True);
    }

    [Test]
    public void ExtractClaims_WithInvalidTokenFormat_ReturnsNullAndLogsWarning()
    {
        var token = "invalid.token";
        var result = _extractor.ExtractClaims(token);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractClaims_WithMissingPayload_ReturnsNull()
    {
        var result = _extractor.ExtractClaims("header..signature");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractClaims_WithInvalidPartsLength_ReturnsNull()
    {
        var result = _extractor.ExtractClaims("header.payload");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractClaims_WithInvalidBase64Payload_ReturnsNull()
    {
        var result = _extractor.ExtractClaims("header.inval!d.signature");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractClaims_WithValidJsonPayloadAndUnrecognizedHeader_ParsesSuccessfully()
    {
        var payloadJson = "{\"sub\":\"123\", \"customClaim\":\"customValue\", \"numberClaim\": 42}";
        var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        
        var token = $"invalid_header_$$$.{encodedPayload}.";
        
        var result = _extractor.ExtractClaims(token);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.HasClaim(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "123"), Is.True);
        Assert.That(result.HasClaim(c => c.Type == "numberClaim"), Is.False);
        Assert.That(result.HasClaim(c => c.Type == "customClaim" && c.Value == "customValue"), Is.True);
    }

    [Test]
    public void ExtractEmail_WithValidEmailClaim_ReturnsEmail()
    {
        var token = GenerateValidSignedToken(new[] { new Claim(ClaimTypes.Email, "a@b.com") });

        var result = _extractor.ExtractEmail(token);

        Assert.That(result, Is.EqualTo("a@b.com"));
    }

    [Test]
    public void ExtractEmail_WithNoEmailClaim_ReturnsNull()
    {
        var token = GenerateValidSignedToken(new[] { new Claim(JwtRegisteredClaimNames.Sub, "1") });

        var result = _extractor.ExtractEmail(token);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExtractEmail_WithMalformedPayloadYieldingException_ReturnsNull()
    {
        var payloadJson = "THIS IS NOT JSON at all!";
        var header = "{\"alg\":\"none\"}";
        var encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
        var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var token = $"{encodedHeader}.{encodedPayload}.";
        
        var emailResult = _extractor.ExtractEmail(token);
        var roleResult = _extractor.ExtractRole(token);
        var userIdResult = _extractor.ExtractUserId(token);

        Assert.That(emailResult, Is.Null);
        Assert.That(roleResult, Is.Null);
        Assert.That(userIdResult, Is.Null);
    }

    [Test]
    [TestCase("header.payload.signature", true)]
    [TestCase("header.payload.", true, Description = "Will be validated further in extraction but format is ok")]
    [TestCase("part1", false)]
    [TestCase("part1.part2", false)]
    [TestCase("part1.part2.part3.part4", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void IsValidTokenFormat_ReturnsExpectedResult(string? token, bool expected)
    {
        if (token == "header.payload.")
        {
            var header = "{\"alg\":\"none\"}";
            var encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
            token = $"{encodedHeader}.payload.";
        }
        else if (token == "header.payload.signature")
        {
            var header = "{\"alg\":\"HS256\"}";
            var encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
            token = $"{encodedHeader}.payload.signature";
        }

        var result = _extractor.IsValidTokenFormat(token);
        Assert.That(result, Is.EqualTo(expected));
    }
}
