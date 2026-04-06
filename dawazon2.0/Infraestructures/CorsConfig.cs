using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración de políticas CORS.
/// </summary>
/// <remarks>
/// Define políticas CORS según el entorno de ejecución.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>IConfiguration: Configuración de la aplicación</item>
/// </list>
/// 
/// <para><b>Políticas:</b></para>
/// <list type="bullet">
///     <item>Desarrollo (AllowAll): localhost:5000, 5001, 7000, 7001 con credenciales</item>
///     <item>Producción (ProductionPolicy): Orígenes configurados en Cors:AllowedOrigins</item>
/// </list>
/// 
/// <para><b>Nota:</b></para>
/// SignalR (Blazor) requiere AllowCredentials(), incompatible con AllowAnyOrigin().
/// </remarks>
public static class CorsConfig
{
    /// <summary>
    /// Configura la política CORS según el entorno.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Desarrollo: política "AllowAll" con orígenes localhost</item>
    ///     <item>Producción: política "ProductionPolicy" con orígenes configurables</item>
    /// </list>
    /// </remarks>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la app.</param>
    /// <param name="isDevelopment">Indica si es entorno de desarrollo.</param>
    /// <returns>IServiceCollection.</returns>
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
                        .AllowCredentials(); 
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