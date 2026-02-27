using System;
using System.Linq;
using dawazon2._0.Infraestructures;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Infraestructure;

[TestFixture]
public class AuthenticationConfigTests
{
    private IServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
    }

    [Test]
    public void AddAuthentication_WhenJwtKeyIsMissing_ShouldThrowInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder().Build();

        var ex = Assert.Throws<InvalidOperationException>(() => _services.AddAuthentication(configuration));
        Assert.That(ex.Message, Is.EqualTo("JWT Key no configurada"));
    }

    [Test]
    public void AddAuthentication_WhenJwtKeyIsProvided_ShouldRegisterAuthenticationServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("Jwt:Key", "SuperSecretKeyWithAtLeast32Characters")
            })
            .Build();

        _services.AddAuthentication(configuration);

        var serviceProvider = _services.BuildServiceProvider();

        var schemeProvider = serviceProvider.GetService<IAuthenticationSchemeProvider>();
        Assert.That(schemeProvider, Is.Not.Null);

        var jwtOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<JwtBearerOptions>>();
        Assert.That(jwtOptionsMonitor, Is.Not.Null);

        var jwtOptions = jwtOptionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);
        Assert.That(jwtOptions, Is.Not.Null);

        var cookieOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<CookieAuthenticationOptions>>();
        Assert.That(cookieOptionsMonitor, Is.Not.Null);

        var cookieOptions = cookieOptionsMonitor.Get(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme);
        Assert.That(cookieOptions, Is.Not.Null);
        Assert.That(cookieOptions.LoginPath.Value, Is.EqualTo("/login"));
        Assert.That(cookieOptions.LogoutPath.Value, Is.EqualTo("/logout"));
    }
}
