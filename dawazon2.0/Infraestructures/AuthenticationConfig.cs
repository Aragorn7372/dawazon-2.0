using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
                options.DefaultScheme = PolicyScheme;
                options.DefaultAuthenticateScheme = PolicyScheme;
                options.DefaultChallengeScheme = PolicyScheme;
                // Necesario para que rol incorrecto devuelva 403 y no 401
                options.DefaultForbidScheme = PolicyScheme;
            })
            .AddPolicyScheme(PolicyScheme, "JWT o Cookie según ruta", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    // Las rutas /api/* usan JWT Bearer
                    if (context.Request.Path.StartsWithSegments("/api"))
                        return JwtBearerDefaults.AuthenticationScheme;

                    // El resto usa cookie de Identity (MVC)
                    return IdentityConstants.ApplicationScheme;
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
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Warning("[JWT] Token inválido: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Log.Warning("[JWT] Challenge 401 — Error: {Error} | Descripción: {Desc}",
                            context.Error ?? "none",
                            context.ErrorDescription ?? "none");
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                };
            });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        return services;
    }
}
