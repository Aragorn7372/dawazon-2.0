using dawazonBackend.Common.Storage;
using Serilog;

namespace dawazon2._0.Infraestructures;

public static class StorageConfig
{
    /// <summary>
    /// Configura el servicio de almacenamiento de archivos locales.
    /// </summary>
    public static IServiceCollection AddStorage(this IServiceCollection services)
    {
        Log.Information("🖼️ Configurando servicio de almacenamiento...");
        return services.AddScoped<IStorage, Storage>();
    }
}