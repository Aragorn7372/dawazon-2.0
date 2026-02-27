using System;
using System.Linq;
using dawazon2._0.Infraestructures;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Infraestructure;

[TestFixture]
public class CorsConfigTests
{
    private IServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
    }

    [Test]
    public void AddCorsPolicy_WhenDevelopment_ShouldRegisterAllowAllPolicy()
    {
        var configuration = new ConfigurationBuilder().Build();

        _services.AddCorsPolicy(configuration, isDevelopment: true);

        var serviceProvider = _services.BuildServiceProvider();
        var corsOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<CorsOptions>>();
        Assert.That(corsOptionsMonitor, Is.Not.Null);

        var corsOptions = corsOptionsMonitor.Get(Options.DefaultName);
        var policy = corsOptions.GetPolicy("AllowAll");

        Assert.That(policy, Is.Not.Null);
        Assert.That(policy.AllowAnyMethod, Is.True);
        Assert.That(policy.AllowAnyHeader, Is.True);
        Assert.That(policy.SupportsCredentials, Is.True);
        Assert.That(policy.Origins.Contains("http://localhost:5000"), Is.True);
        Assert.That(policy.Origins.Contains("https://localhost:5001"), Is.True);
        Assert.That(policy.AllowAnyOrigin, Is.False);
    }

    [Test]
    public void AddCorsPolicy_WhenProductionAndAllowedOriginsMissing_ShouldThrowInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder().Build();

        _services.AddCorsPolicy(configuration, isDevelopment: false);
        var serviceProvider = _services.BuildServiceProvider();
        var corsOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<CorsOptions>>();

        var ex = Assert.Throws<InvalidOperationException>(() => corsOptionsMonitor.Get(Options.DefaultName));
        Assert.That(ex.Message, Is.EqualTo("Cors:AllowedOrigins no configurado"));
    }

    [Test]
    public void AddCorsPolicy_WhenProductionAndAllowedOriginsProvided_ShouldRegisterProductionPolicy()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("Cors:AllowedOrigins:0", "https://example.com"),
                new System.Collections.Generic.KeyValuePair<string, string>("Cors:AllowedOrigins:1", "https://another-domain.com")
            })
            .Build();

        _services.AddCorsPolicy(configuration, isDevelopment: false);

        var serviceProvider = _services.BuildServiceProvider();
        var corsOptionsMonitor = serviceProvider.GetService<IOptionsMonitor<CorsOptions>>();
        Assert.That(corsOptionsMonitor, Is.Not.Null);

        var corsOptions = corsOptionsMonitor.Get(Options.DefaultName);
        var policy = corsOptions.GetPolicy("ProductionPolicy");

        Assert.That(policy, Is.Not.Null);
        Assert.That(policy.AllowAnyMethod, Is.True);
        Assert.That(policy.AllowAnyHeader, Is.True);
        Assert.That(policy.SupportsCredentials, Is.True);
        Assert.That(policy.Origins.Contains("https://example.com"), Is.True);
        Assert.That(policy.Origins.Contains("https://another-domain.com"), Is.True);
    }
}
