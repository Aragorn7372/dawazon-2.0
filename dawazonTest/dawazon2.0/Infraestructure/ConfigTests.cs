using dawazon2._0.Infraestructures;
using dawazonBackend.Cart.Repository;
using dawazonBackend.Cart.Service;
using dawazonBackend.Common.Database;
using dawazonBackend.Products.Repository.Categoria;
using dawazonBackend.Products.Repository.Productos;
using dawazonBackend.Products.Service;
using dawazonBackend.Stripe;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using dawazonBackend.Users.Service.Auth;
using dawazonBackend.Users.Service.Favs;
using dawazonBackend.Users.Service.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using dawazonBackend.Common.Mail;

namespace dawazonTest.dawazon2._0.Infraestructure;

[TestFixture]
public class ConfigTests
{
    private IServiceCollection _services;
    private IConfiguration _configuration;
    private IWebHostEnvironment _env;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
        
        var inMemorySettings = new Dictionary<string, string> {
            {"IsDevelopment", "true"},
            {"Cors:AllowedOrigins:0", "http://test.com"},
            {"Jwt:Secret", "A_Very_Long_Secret_Key_For_Testing_12345"},
            {"Jwt:Issuer", "Issuer"},
            {"Jwt:Audience", "Audience"},
            {"Jwt:ExpirationInMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(m => m.EnvironmentName).Returns(Environments.Development);
        _env = envMock.Object;
    }

    [Test]
    public void CacheConfig_AddCache_ShouldRegisterMemoryCache()
    {
        _services.AddCache(_configuration);
        Assert.That(_services.Any(s => s.ServiceType.Name.Contains("IMemoryCache")), Is.True);
    }

    [Test]
    public void CorsConfig_AddCorsPolicy_ShouldRegisterServices()
    {
        _services.AddCorsPolicy(_configuration, true);
        _services.AddCorsPolicy(_configuration, false);
        Assert.That(_services.Any(s => s.ServiceType.Name.Contains("ICorsService")), Is.True);
    }

    [Test]
    public void SessionConfig_AddSession_ShouldRegisterServices()
    {
        _services.AddSession(_configuration);
        Assert.That(_services.Any(s => s.ServiceType.Name.Contains("ISessionStore")), Is.True);
    }

    [Test]
    public void EmailConfig_AddEmail_ShouldRegisterServices()
    {
        _services.AddEmail(_env);
        Assert.That(_services.Any(s => s.ServiceType == typeof(Channel<EmailMessage>)), Is.True);
    }

    [Test]
    public void ControllerConfig_AddMvcControllers_ShouldRegisterServices()
    {
        _services.AddLogging();
        _services.AddMvcControllers();
        var provider = _services.BuildServiceProvider();

        var mvcOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.MvcOptions>>().Value;
        Assert.That(mvcOptions.RespectBrowserAcceptHeader, Is.True);
        Assert.That(mvcOptions.ReturnHttpNotAcceptable, Is.True);

        var jsonOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>().Value;
        Assert.That(jsonOptions.JsonSerializerOptions.PropertyNamingPolicy, Is.EqualTo(System.Text.Json.JsonNamingPolicy.CamelCase));
        Assert.That(jsonOptions.JsonSerializerOptions.WriteIndented, Is.True);
        Assert.That(jsonOptions.JsonSerializerOptions.PropertyNameCaseInsensitive, Is.True);

        Assert.That(_services.Count, Is.GreaterThan(0));
    }

    [Test]
    public void RateLimitConfig_AddRateLimitingPolicy_ShouldRegisterServices()
    {
        _services.AddRateLimitingPolicy();
        Assert.That(_services.Any(s => s.ServiceType.Name.Contains("IMemoryCache")), Is.True);
    }

    [Test]
    public void DbConfig_AddDatabase_ShouldRegisterServices()
    {
        _services.AddLogging(); 
        _services.AddDatabase(_configuration);
        var provider = _services.BuildServiceProvider();

        var identityOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdentityOptions>>().Value;
        Assert.That(identityOptions.Password.RequireDigit, Is.True);
        Assert.That(identityOptions.Password.RequiredLength, Is.EqualTo(6));
        Assert.That(identityOptions.Password.RequireNonAlphanumeric, Is.True);
        Assert.That(identityOptions.Password.RequireUppercase, Is.True);

        var dbContextOptions = provider.GetRequiredService<Microsoft.EntityFrameworkCore.DbContextOptions<DawazonDbContext>>();
        Assert.That(dbContextOptions, Is.Not.Null);

        Assert.That(_services.Any(s => s.ServiceType == typeof(DawazonDbContext)), Is.True);
    }

    [Test]
    public void DbConfig_AddDatabase_Production_ShouldRegisterServices()
    {
        var prodConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> {
            {"IsDevelopment", "false"},
            {"DATABASE_URL", "Host=localhost;Port=5432;Database=test;Username=user;Password=pass;"}
        }).Build();

        var prodServices = new ServiceCollection();
        prodServices.AddLogging();
        prodServices.AddDatabase(prodConfig);

        var provider = prodServices.BuildServiceProvider();

        var dbContextOptions = provider.GetRequiredService<Microsoft.EntityFrameworkCore.DbContextOptions<DawazonDbContext>>();
        Assert.That(dbContextOptions, Is.Not.Null);
        var extensions = dbContextOptions.Extensions.ToList();
        Assert.That(extensions.Count, Is.GreaterThan(0));
    }

    [Test]
    public void RepositoriesConfig_AddRepositories_ShouldRegisterServices()
    {
        _services.AddRepositories();
        Assert.Multiple(() =>
        {
            Assert.That(_services.Any(s => s.ServiceType == typeof(ICategoriaRepository)), Is.True, "ICategoriaRepository not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(IProductRepository)), Is.True, "IProductRepository not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(UserManager<User>)), Is.True, "UserManager<User> not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(ICartRepository)), Is.True, "ICartRepository not registered");
        });
    }

    [Test]
    public void ServicesConfig_AddServices_ShouldRegisterServices()
    {
        _services.AddServices();
        Assert.Multiple(() =>
        {
            Assert.That(_services.Any(s => s.ServiceType == typeof(IAuthService)), Is.True, "IAuthService not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(IJwtService)), Is.True, "IJwtService not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(IJwtTokenExtractor)), Is.True, "IJwtTokenExtractor not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(IProductService)), Is.True, "IProductService not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(IUserService)), Is.True, "IUserService not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(ICartService)), Is.True, "ICartService not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(IStripeService)), Is.True, "IStripeService not registered");
            Assert.That(_services.Any(s => s.ServiceType == typeof(IFavService)), Is.True, "IFavService not registered");
        });
    }

    [Test]
    public void StorageConfig_AddStorage_ShouldRegisterServices()
    {
        _services.AddStorage();
        Assert.That(_services.Any(s => s.ServiceType.Name.Contains("IStorage")), Is.True);
    }

    [Test]
    public void SignalRConfig_AddAppSignalR_ShouldRegisterServices()
    {
        _services.AddAppSignalR();
        Assert.That(_services.Count, Is.GreaterThan(0));
    }

    [Test]
    public void AuthenticationConfig_AddAuthentication_ShouldRegisterServices()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "A_Very_Long_Secret_Key_For_Testing_12345"},
            {"Jwt:Issuer", "Issuer"},
            {"Jwt:Audience", "Audience"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        
        _services.AddAuthentication(config);
        Assert.That(_services.Any(s => s.ServiceType.Name.Contains("IAuthenticationService")), Is.True);
    }
}