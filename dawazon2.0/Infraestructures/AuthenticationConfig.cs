using System.Text;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Extensiones de configuración de autenticación y autorización JWT.
/// </summary>
public static class AuthenticationConfig
{
    /// <summary>
    /// Configura autenticación JWT con tokens Bearer.
    /// </summary>
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Information("🔐 Configurando autenticación JWT...");

        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key no configurada");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "TiendaApi";
        var jwtAudience = configuration["Jwt:Audience"] ?? "TiendaApi";

        Log.Debug("🔑 JWT Issuer: {Issuer}", jwtIssuer);
        Log.Debug("🎯 JWT Audience: {Audience}", jwtAudience);

        services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "TiendaApi",
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"] ?? "TiendaApi",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}