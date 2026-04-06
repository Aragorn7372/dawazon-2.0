using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración de Serilog para logging.
/// </summary>
/// <remarks>
/// Configura el logger con salida a consola y filtros de nivel.
///
/// <para><b>Niveles:</b></para>
/// <list type="bullet">
///     <item>Default: Information</item>
///     <item>Microsoft: Warning</item>
///     <item>EF Core: Warning</item>
/// </list>
/// 
/// <para><b>Salida:</b></para>
/// <list type="bullet">
///     <item>Consola con template personalizado</item>
///     <item>Tema: AnsiConsoleTheme.Code</item>
/// </list>
/// </remarks>
public static class SerilogConfig
{
    /// <summary>
    /// Configura Serilog.
    /// </summary>
    /// <returns>LoggerConfiguration.</returns>
    public static LoggerConfiguration Configure()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code);
    }
}