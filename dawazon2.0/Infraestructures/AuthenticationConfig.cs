using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración de autenticación híbrida JWT + Cookie.
/// </summary>
/// <remarks>
/// Implementa autenticación dual para API (JWT) y MVC (Cookie).
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>IConfiguration: Configuración (Jwt:Key, Issuer, Audience)</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>JWT para rutas /api/*</item>
///     <item>Cookie para rutas MVC</li>
///     <item>Expiración de cookie: 8 horas</li>
/// </list>
/// 
/// <para><b>Configuración JWT:</b></para>
/// <list type="bullet">
///     <item>Validación de firma con clave simétrica</item>
///     <item>Validación de issuer y audience</item>
///     <item>Validación de lifetime</item>
/// </list>
/// </remarks>
public static class AuthenticationConfig
{
    private const string PolicyScheme = "PolicyScheme";

    /// <summary>
    /// Configura autenticación JWT (API) y Cookie (MVC).
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>JWT Bearer para /api/*</item>
    ///     <item>Identity Cookie para el resto</item>
    ///     <item>Cookie: 8 horas con sliding expiration</item>
    /// </list>
    /// </remarks>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la app.</param>
    /// <returns>IServiceCollection.</returns>
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
