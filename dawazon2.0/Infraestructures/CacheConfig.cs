using dawazonBackend.Common.Cache;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace dawazon2._0.Infraestructures;

public static class CacheConfig
{
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