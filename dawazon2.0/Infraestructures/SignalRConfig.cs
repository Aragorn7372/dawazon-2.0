using dawazonBackend.Common.Hub;
using Serilog;

namespace dawazon2._0.Infraestructures;

public static class SignalRConfig
{
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