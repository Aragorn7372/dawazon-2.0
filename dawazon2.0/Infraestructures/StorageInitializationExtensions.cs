using Serilog;
using Path = System.IO.Path;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Inicialización del directorio de almacenamiento de archivos.
/// </summary>
/// <remarks>
/// Crea el directorio uploads en wwwroot si no existe.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Ubicación: {WebRootPath}/uploads</item>
///     <item>Solo crea, no modifica contenido existente</item>
/// </list>
/// </remarks>
public static class StorageInitializationExtensions
{
    /// <summary>
    /// Inicializa el directorio de almacenamiento.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Construye ruta: {WebRootPath}/uploads</item>
    ///     <item>Verifica si existe</li>
    ///     <item>Lo crea si no existe</item>
    /// </list>
    /// </remarks>
    /// <param name="app">Aplicación web.</param>
    public static void InitializeStorage(this WebApplication app)
    {
        var storagePath = Path.Combine(app.Environment.WebRootPath, "uploads");
        var storageDirectory = new DirectoryInfo(storagePath);
        
        Log.Information("[PRODUCCIÓN] Verificando directorio de almacenamiento: {Path}", storagePath);
        try
        {
            if (!storageDirectory.Exists)
            {
                storageDirectory.Create();
                Log.Information("Directorio de almacenamiento creado");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al verificar directorio de almacenamiento");
        }
        
    }
}