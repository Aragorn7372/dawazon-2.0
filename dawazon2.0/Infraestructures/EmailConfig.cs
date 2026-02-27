using System.Threading.Channels;
using dawazonBackend.Common.Mail;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace dawazon2._0.Infraestructures;

public static class EmailConfig
{
    /// <summary>
    /// Configura el servicio de email.
    /// Desarrollo: MemoryEmailService (no envía realmente).
    /// Producción: MailKitEmailService (envía emails reales).
    /// </summary>
    public static IServiceCollection AddEmail(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddSingleton(Channel.CreateUnbounded<EmailMessage>());

            Log.Information("Configurando servicio de email con MailKit (producción)...");
            services.TryAddScoped<IEmailService, MailKitEmailService>();
            services.AddHostedService<EmailBackgroundService>();

        return services;
    }
}