using System.Text;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
                // Esquema selector: elige JWT o Cookie según la ruta
                options.DefaultScheme = PolicyScheme;
                // Necesario para que UseAuthentication() asigne el User correctamente
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
                    // Diagnóstico: muestra el error exacto de validación del token
                    OnAuthenticationFailed = context =>
                    {
                        Log.Warning("[JWT] Token inválido: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    // Diagnóstico: muestra por qué se emite un 401
                    OnChallenge = context =>
                    {
                        Log.Warning("[JWT] Challenge 401 — Error: {Error} | Descripción: {Desc}",
                            context.Error ?? "none",
                            context.ErrorDescription ?? "none");
                        return Task.CompletedTask;
                    },
                    // Cuando usuario autenticado NO tiene el rol requerido → 403
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                };
            });

        // Configurar la cookie que Identity usa internamente
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            // En desarrollo (HTTP) evita que el navegador rechace la cookie por falta de HTTPS
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        return services;
    }
}
