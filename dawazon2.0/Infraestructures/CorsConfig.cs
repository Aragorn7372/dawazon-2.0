using Serilog;

namespace dawazon2._0.Infraestructures;

public static class CorsConfig
{
    /// <summary>
    /// Configura la política CORS según el entorno.
    /// Desarrollo: AllowAll (permite todo)
    /// Producción: Solo orígenes configurados en Cors:AllowedOrigins
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        Log.Information("Configurando CORS para {Environment}...", isDevelopment ? "DESARROLLO" : "PRODUCCIÓN");

        return services.AddCors(options =>
        {
            if (isDevelopment)
            {
                // SignalR (Blazor) WebSockets requieren AllowCredentials(),
                // que es incompatible con AllowAnyOrigin().
                // Por eso usamos orígenes explícitos de localhost.
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:5000",
                            "https://localhost:5001",
                            "http://localhost:7000",
                            "https://localhost:7001")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials(); // ← necesario para SignalR/Blazor
                });
                Log.Information("CORS: AllowAll (desarrollo) con credenciales para SignalR");
            }
            else
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                     ?? throw new InvalidOperationException("Cors:AllowedOrigins no configurado");

                options.AddPolicy("ProductionPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
                Log.Information("CORS: ProductionPolicy con {Count} orígenes", allowedOrigins.Length);
            }
        });
    }
}