using dawazon2._0.Infraestructures;
using dawazonBackend.Common.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace dawazonTest.dawazon2._0.Infraestructure;

[TestFixture]
public class DatabaseInitializationTests
{
    [Test]
    public async Task InitializeDatabaseAsync_ShouldNotThrow_WhenDependenciesAreRegistered()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddDbContext<DawazonDbContext>(options =>
            options.UseInMemoryDatabase("TestDatabase"));
            
        builder.Services.AddLogging(configure => configure.AddConsole());

        var app = builder.Build();

        Assert.DoesNotThrowAsync(async () => await app.InitializeDatabaseAsync());
    }
}
