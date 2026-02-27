using dawazon2._0.Infraestructures;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.Infraestructure;

[TestFixture]
public class CorsExtensionsTests
{
    [Test]
    public void UseCorsPolicy_ShouldNotThrow_WhenCalledWithWebApplication()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        Assert.DoesNotThrow(() => app.UseCorsPolicy());
    }
}
