using System.Text.Json;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración de controladores MVC.
/// </summary>
/// <remarks>
/// Configura los controladores con negociación de contenido.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>RespectBrowserAcceptHeader: true</item>
///     <item>ReturnHttpNotAcceptable: true</item>
///     <item>JSON: CamelCase, WriteIndented</item>
///     <item>XML: XmlSerializer, DataContractSerializer</item>
/// </list>
/// </remarks>
public static class ControllerConfig
{
    /// <summary>
    /// Configura los controladores MVC.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IMvcBuilder.</returns>
    public static IMvcBuilder AddMvcControllers(this IServiceCollection services)
    {
        Log.Information("Configurando controladores MVC...");
        return services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            })
            .AddXmlSerializerFormatters()
            .AddXmlDataContractSerializerFormatters();
    }
}