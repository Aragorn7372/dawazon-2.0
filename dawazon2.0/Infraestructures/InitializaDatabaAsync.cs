using dawazonBackend.Common.Database;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Inicialización de la base de datos.
/// </summary>
/// <remarks>
/// Asegura que las tablas existan en PostgreSQL.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Usa EnsureCreatedAsync</item>
///     <item>No elimina datos existentes</item>
/// </list>
/// </remarks>
public static class InitializaDatabaAsync
{
    /// <summary>
    /// Inicializa la base de datos.
    /// </summary>
    /// <param name="app">Aplicación web.</param>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        Log.Information("Inicializando base de datos...");

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DawazonDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Base de datos verificada (tablas creadas si no existían)");
        
    }
}