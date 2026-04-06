using dawazonBackend.Common.Hub;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración de SignalR para notificaciones en tiempo real.
/// </summary>
/// <remarks>
/// Configura el hub de SignalR para Blazor Server.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>NotificationHub: Hub del backend</item>
/// </list>
/// 
/// <para><b>Configuración:</b></para>
/// <list type="bullet">
///     <item>EnableDetailedErrors: true</item>
///     <item>MaximumReceiveMessageSize: 4KB</item>
///     <item>KeepAliveInterval: 15 segundos</item>
/// </list>
/// </remarks>
public static class SignalRConfig
{
    /// <summary>
    /// Configura SignalR.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddAppSignalR(this IServiceCollection services)
    {
        Log.Information("Configurando SignalR...");

        services.AddSignalR()
            .AddHubOptions<NotificationHub>(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = 1024 * 4;
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            });

        Log.Information("SignalR configurado");

        return services;
    }
}