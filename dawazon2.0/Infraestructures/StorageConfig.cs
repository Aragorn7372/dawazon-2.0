using dawazonBackend.Common.Storage;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración del servicio de almacenamiento de archivos.
/// </summary>
/// <remarks>
/// Registra el servicio IStorage para manejar subida de imágenes.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>IStorage: Interfaz del servicio (implementación: Storage)</item>
/// </list>
/// </remarks>
public static class StorageConfig
{
    /// <summary>
    /// Configura el servicio de almacenamiento de archivos locales.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Registra IStorage como scoped</item>
    ///     <item>Usa implementación del proyecto backend</item>
    /// </list>
    /// </remarks>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddStorage(this IServiceCollection services)
    {
        Log.Information("Configurando servicio de almacenamiento...");
        return services.AddScoped<IStorage, Storage>();
    }
}