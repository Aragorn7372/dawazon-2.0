using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Extensiones para aplicar la política CORS.
/// </summary>
/// <remarks>
/// Aplica la política CORS configurada en CorsConfig según el entorno.
/// 
/// <para><b>Políticas:</b></para>
/// <list type="bullet">
///     <item>Desarrollo: "AllowAll"</item>
///     <item>Producción: "ProductionPolicy"</item>
/// </list>
/// </remarks>
public static class CorsExtensions
{
    /// <summary>
    /// Aplica la política CORS según el entorno.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Detecta el entorno de ejecución</item>
    ///     <item>Selecciona la política apropiada</item>
    ///     <item>Aplica el middleware CORS</item>
    /// </list>
    /// </remarks>
    /// <param name="app">Constructor de la aplicación.</param>
    /// <returns>IApplicationBuilder.</returns>
    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
    {
        var env = ((WebApplication)app).Environment;

        var policyName = env.IsDevelopment() ? "AllowAll" : "ProductionPolicy";

        Log.Information("Aplicando política CORS: {PolicyName}", policyName);
        return app.UseCors(policyName);
    }
}