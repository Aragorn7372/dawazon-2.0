using dawazonBackend.Common.Database;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Siembra de datos iniciales de Identity.
/// </summary>
/// <remarks>
/// Crea roles y usuarios por defecto si no existen.
///
/// <para><b>Roles creados:</b></para>
/// <list type="bullet">
///     <item>Admin</item>
///     <item>User</item>
///     <item>Manager</item>
/// </list>
/// 
/// <para><b>Usuarios por defecto:</b></para>
/// <list type="bullet">
///     <item>admin@admin.com / Admin123! (rol Admin)</item>
///     <item>user@user.com / User123! (rol User)</item>
///     <item>manager@manager.com / Manager123! (rol Manager)</item>
/// </list>
/// </remarks>
public static class IdentitySeeder
{
    /// <summary>
    /// Siembra roles y usuarios iniciales.
    /// </summary>
    /// <param name="app">Aplicación web.</param>
    public static async Task SeedIdentityAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DawazonDbContext>();
            context.Database.EnsureCreated();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Inicializando roles y usuarios...");

            // Seed de roles y usuario admin
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole<long>("Admin"));
                logger.LogInformation("Rol 'Admin' creado");
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole<long>("User"));
                logger.LogInformation("Rol 'User' creado");
            }

            if (!await roleManager.RoleExistsAsync("Manager"))
            {
                await roleManager.CreateAsync(new IdentityRole<long>("Manager"));
                logger.LogInformation("Rol 'Manager' creado");
            }

            var adminUser = await userManager.FindByEmailAsync("admin@admin.com");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Name = "Administrador Principal",
                    UserName = "admin",
                    Email = "admin@admin.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Usuario admin creado correctamente");
                }
                else
                {
                    logger.LogError("Error al crear usuario admin: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            var normalUser = await userManager.FindByEmailAsync("user@user.com");
            if (normalUser == null)
            {
                normalUser = new User
                {
                    Name = "Usuario Normal",
                    UserName = "user",
                    Email = "user@user.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(normalUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                    logger.LogInformation("Usuario normal creado correctamente");
                }
                else
                {
                    logger.LogError("Error al crear usuario normal: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                var managerUser = await userManager.FindByEmailAsync("manager@manager.com");
                if (managerUser == null)
                {
                    managerUser = new User
                    {
                        Name = "Manager Principal",
                        UserName = "manager",
                        Email = "manager@manager.com",
                        EmailConfirmed = true
                    };
                    var resultado = await userManager.CreateAsync(managerUser, "Manager123!");
                    if (resultado.Succeeded)
                    {
                        await userManager.AddToRoleAsync(managerUser, "Manager");
                        logger.LogInformation("Usuario manager creado correctamente");
                    }
                    else
                    {
                        logger.LogError("Error al crear usuario manager: {Errors}",
                            string.Join(", ", resultado.Errors.Select(e => e.Description)));
                    }
                }

                logger.LogInformation("Roles y usuarios inicializados correctamente");
            }
        }
    }
}