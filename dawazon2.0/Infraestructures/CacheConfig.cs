using dawazonBackend.Common.Cache;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace dawazon2._0.Infraestructures;

public static class CacheConfig
{
   public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
   {
      var isDevelopment = configuration.GetValue<bool?>("IsDevelopment") ?? true;
      if (isDevelopment)
      {
         services.AddMemoryCache();
         
      }
      else
      {
         Log.Information("💾 Configurando caché Redis (producción)...");
         services.AddStackExchangeRedisCache(options =>
         {
            options.Configuration = "redis:6379,password=redispass123";
            options.InstanceName = "FunkoApi:";
         });
      }
      services.TryAddScoped<ICacheService, CacheService>();
      return services;
   }
}