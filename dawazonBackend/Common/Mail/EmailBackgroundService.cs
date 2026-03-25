using System.Threading.Channels;

namespace dawazonBackend.Common.Mail;
/// <summary>
/// Servicio en segundo plano que procesa emails de la cola.
/// </summary>
/// <remarks>
/// Implementa <see cref="BackgroundService"/> de ASP.NET Core.
/// Procesa mensajes de forma asíncrona desde un Channel.
///
/// <para><b>Flujo:</b></para>
/// <list type="number">
///     <item>Lee mensajes del channel</item>
///     <item>Crea un scope para obtener IEmailService</item>
///     <li>Envía el email usando MailKit</item>
///     <li>Registra éxito o error</item>
/// </list>
/// </remarks>
public class EmailBackgroundService(
    Channel<EmailMessage> emailChannel,
    IServiceProvider serviceProvider,
    ILogger<EmailBackgroundService> logger
) : BackgroundService
{
    /// <inheritdoc/>
    /// <summary>
    /// Bucle principal que procesa emails de la cola.
    /// </summary>
    /// <remarks>
    /// Se ejecuta continuamente hasta que se cancela el token.
    /// Lee mensajes del channel y los envía de forma asíncrona.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Servicio de email en segundo plano iniciado");

        await foreach (var emailMessage in emailChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                logger.LogInformation("Procesando email de la cola para: {To}", emailMessage.To);

                await emailService.SendEmailAsync(emailMessage);

                logger.LogInformation("Email procesado exitosamente para: {To}", emailMessage.To);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error procesando email para: {To}", emailMessage.To);
            }
        }

        logger.LogInformation("Servicio de email en segundo plano detenido");
    }
}