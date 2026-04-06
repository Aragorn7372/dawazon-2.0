using dawazonBackend.Common.Database;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración de bases de datos.
/// </summary>
/// <remarks>
/// Configura los DbContext y Identity para usuarios.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>IConfiguration: Configuración de la app</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Productos: InMemory (dev) o PostgreSQL (prod)</li>
///     <item>Usuarios: Entity Framework Core con Identity</item>
/// </list>
/// 
/// <para><b>Configuración de contraseña:</b></para>
/// <list type="bullet">
///     <item>RequireDigit: true</item>
///     <item>RequiredLength: 6</item>
///     <item>RequireNonAlphanumeric: true</item>
///     <item>RequireUppercase: true</item>
/// </list>
/// </remarks>
public static class DbConfig
{
    /// <summary>
    /// Configura los servicios de base de datos.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Desarrollo: UseInMemoryDatabase</item>
    ///     <item>Producción: UseNpgsql (PostgreSQL)</item>
    ///     <item>Configura Identity para usuarios</item>
    /// </list>
    /// </remarks>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la app.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        //BBDD de Productos y categorías
        services.AddDbContext<DawazonDbContext>(options =>
        {
            var isDevelopment = configuration.GetValue<bool?>("Development") ?? true;
            
            if(isDevelopment) options.UseInMemoryDatabase("DawazonDatabase");
            else
            {
                Log.Information("modo produccion activado conectando a base de datos");
                var connectionString = configuration["DATABASE_URL"] 
                                       ?? configuration.GetConnectionString("DefaultConnection") 
                                       ?? "Host=localhost;Port=5432;Database=dawazon_db;Username=dawazon_user;Password=dawazon_password;";
                options.UseNpgsql(connectionString);
                options.EnableSensitiveDataLogging(); 
                options.EnableDetailedErrors(); 
            }
        });

        //BBDD de Usuarios
        services.AddIdentity<User, IdentityRole<long>>(options => 
            {
                // Configuraciones opcionales de contraseña, etc. 
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<DawazonDbContext>()
            .AddDefaultTokenProviders();
        
        return services;
        
        
    }
}