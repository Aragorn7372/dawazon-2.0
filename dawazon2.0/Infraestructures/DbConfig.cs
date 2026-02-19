using dawazonBackend.Common.Database;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace dawazon2._0.Infraestructures;

public static class DbConfig
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        //BBDD de Productos y categorías
        services.AddDbContext<DawazonDbContext>(options =>
        {
            var isDevelopment = configuration.GetValue<bool?>("IsDevelopment") ?? true;
            
            if(isDevelopment) options.UseInMemoryDatabase("DawazonDatabase");
            else
            {
                var connectionString = configuration["DATABASE_URL"] 
                                       ?? configuration.GetConnectionString("DefaultConnection") 
                                       ?? "Host=localhost;Port=5432;Database=dawazon_db;Username=dawazon_user;Password=dawazon_password;";
                options.UseNpgsql(connectionString);
                options.EnableSensitiveDataLogging(); // Para producción
                options.EnableDetailedErrors(); // Para producción
            }
        });

        //BBDD de Usuarios
        services.AddIdentity<User, IdentityRole<long>>(options => 
            {
                // Configuraciones opcionales de contraseña, etc. P@ssw0rd
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