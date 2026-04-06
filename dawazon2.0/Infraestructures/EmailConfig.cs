using System.Threading.Channels;
using dawazonBackend.Common.Mail;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración del servicio de email.
/// </summary>
/// <remarks>
/// Configura el servicio de envío de emails.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>IWebHostEnvironment: Entorno de ejecución</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Usa MailKit para producción</item>
///     <item>Background service para envío asíncrono</item>
/// </list>
/// </remarks>
public static class EmailConfig
{
    /// <summary>
    /// Configura el servicio de email.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="environment">Entorno de la aplicación.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddEmail(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddSingleton(Channel.CreateUnbounded<EmailMessage>());

            Log.Information("Configurando servicio de email con MailKit (producción)...");
            services.TryAddScoped<IEmailService, MailKitEmailService>();
            services.AddHostedService<EmailBackgroundService>();

        return services;
    }
}