using dawazon2._0.Infraestructures;
using dawazonBackend.Common.Database;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace dawazonTest.dawazon2._0.Infraestructure;

[TestFixture]
public class IdentitySeederTests
{
    [Test]
    public async Task SeedIdentityAsync_ShouldCreateRolesAndDefaultUsers()
    {
        var builder = WebApplication.CreateBuilder();

        var dbName = "TestIdentityDatabase_" + System.Guid.NewGuid().ToString();
        builder.Services.AddDbContext<DawazonDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        builder.Services.AddIdentity<User, IdentityRole<long>>()
            .AddEntityFrameworkStores<DawazonDbContext>();

        builder.Services.AddLogging(configure => configure.AddConsole());

        var app = builder.Build();

        await app.SeedIdentityAsync();

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DawazonDbContext>();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var rolesCount = roleManager.Roles.Count();
        Assert.That(rolesCount, Is.GreaterThanOrEqualTo(3), "Should have seeded Admin, Manager, and User roles");

        var usersCount = userManager.Users.Count();
        Assert.That(usersCount, Is.GreaterThanOrEqualTo(3), "Should have seeded admin, manager, and user accounts");

        Assert.That(await roleManager.RoleExistsAsync("Admin"), Is.True);
        Assert.That(await roleManager.RoleExistsAsync("Manager"), Is.True);
        Assert.That(await roleManager.RoleExistsAsync("User"), Is.True);

        Assert.That(await userManager.FindByEmailAsync("admin@admin.com"), Is.Not.Null);
        Assert.That(await userManager.FindByEmailAsync("manager@manager.com"), Is.Not.Null);
        Assert.That(await userManager.FindByEmailAsync("user@user.com"), Is.Not.Null);
    }
}
