using System.Text;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Extensiones de configuración de autenticación y autorización JWT + Cookie.
/// </summary>
public static class AuthenticationConfig
{
    private const string PolicyScheme = "PolicyScheme";

    /// <summary>
    /// Configura autenticación JWT (para /api/*) y Cookie (para rutas MVC).
    /// </summary>
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Information("🔐 Configurando autenticación JWT...");

        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key no configurada");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "TiendaApi";
        var jwtAudience = configuration["Jwt:Audience"] ?? "TiendaApi";

        services.AddAuthentication(options =>
            {
                // Esquema selector: elige JWT o Cookie según la ruta
                options.DefaultScheme = PolicyScheme;
                options.DefaultChallengeScheme = PolicyScheme;
            })
            .AddPolicyScheme(PolicyScheme, "JWT o Cookie según ruta", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    // Las rutas /api/* usan JWT Bearer
                    if (context.Request.Path.StartsWithSegments("/api"))
                        return JwtBearerDefaults.AuthenticationScheme;

                    // El resto usa cookie (MVC)
                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });

        return services;
    }
}
