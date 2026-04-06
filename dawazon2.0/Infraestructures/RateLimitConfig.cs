using AspNetCoreRateLimit;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración de Rate Limiting.
/// </summary>
/// <remarks>
/// Protege la API de ataques de fuerza bruta y abuso.
///
/// <para><b>Reglas:</b></para>
/// <list type="bullet">
///     <item>General: 100 req/15s</item>
///     <item>Auth: 10 req/min</item>
///     <item>Escritura POST: 20 req/min</item>
///     <item>GraphQL: 200 req/min</item>
/// </list>
/// 
/// <para><b>Configuración:</b></para>
/// <list type="bullet">
///     <item>EnableEndpointRateLimiting: true</item>
///     <item>HttpStatusCode: 429</item>
/// </list>
/// </remarks>
public static class RateLimitConfig
{
    /// <summary>
    /// Configura las políticas de rate limiting.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddRateLimitingPolicy(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.Configure<RateLimitOptions>(options =>
        {
            options.EnableEndpointRateLimiting = true;
            options.HttpStatusCode = 429;
            options.QuotaExceededMessage = "Demasiadas solicitudes. Por favor, intente más tarde.";
            
            options.GeneralRules = new List<RateLimitRule>
            {
                // API General: 100 requests por 15 segundos
                new RateLimitRule
                {
                    Endpoint = "*",
                    Limit = 100,
                    Period = "15s"
                },
                // Endpoints de autenticación: más estrictos (fuerza bruta)
                new RateLimitRule
                {
                    Endpoint = "*/api/v1/auth/*",
                    Limit = 10,
                    Period = "1m"
                },
                // Endpoints de escritura: más estrictos
                new RateLimitRule
                {
                    Endpoint = "POST:*",
                    Limit = 20,
                    Period = "1m"
                },
                // GraphQL: más permisivo para queries
                new RateLimitRule
                {
                    Endpoint = "POST:/graphql",
                    Limit = 200,
                    Period = "1m"
                }
            };
        });

        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        
        return services;
    }

    /// <summary>
    /// Aplica el middleware de Rate Limiting.
    /// </summary>
    /// <param name="app">Constructor de la aplicación.</param>
    /// <returns>IApplicationBuilder.</returns>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        app.UseIpRateLimiting();
        return app;
    }
}