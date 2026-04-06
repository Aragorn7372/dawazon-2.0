using dawazonBackend.Common.Cache;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración del sistema de caché.
/// </summary>
/// <remarks>
/// Configura el proveedor de caché según el entorno.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>IConfiguration: Configuración (Development, Redis:Host/Port/Password)</item>
/// </list>
/// 
/// <para><b>Proveedores:</b></para>
/// <list type="bullet">
///     <item>Desarrollo: MemoryCache</li>
///     <item>Producción: Redis (StackExchange)</li>
/// </list>
/// 
/// <para><b>Configuración Redis:</b></li>
/// <list type="bullet">
///     <item>Host: configurable (default: redis)</item>
///     <item>Port: configurable (default: 6379)</item>
///     <item>InstanceName: Dawazon2.0:</item>
/// </list>
/// </remarks>
public static class CacheConfig
{
    /// <summary>
    /// Configura el servicio de caché.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la app.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
   {
      var isDevelopment = configuration.GetValue<bool?>("Development") ?? true;
      if (isDevelopment)
      {
         services.AddMemoryCache();
         
      }
      else
      {
         Log.Information("Configurando caché Redis (producción)...");
         services.AddStackExchangeRedisCache(options =>
         {
            var host = configuration.GetValue<string>("Redis:Host") ?? "redis";
            var port = configuration.GetValue<string>("Redis:Port") ?? "6379";
            var password = configuration.GetValue<string>("Redis:Password") ?? "redispass123";
            
            options.Configuration = $"{host}:{port},password={password}";
            options.InstanceName = "Dawazon2.0:";
         });
      }
      services.TryAddScoped<ICacheService, CacheService>();
      return services;
   }
}