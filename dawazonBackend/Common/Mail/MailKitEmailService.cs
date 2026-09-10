using System.Threading.Channels;
using MailKit.Net.Smtp;
using MimeKit;

namespace dawazonBackend.Common.Mail;

/// <summary>
/// Implementación del servicio de correo usando MailKit.
/// </summary>
/// <remarks>
/// Proporciona envío inmediato y encolado de emails.
/// Usa SMTP configurado en appsettings.json.
/// </remarks>
public class MailKitEmailService(
    IConfiguration configuration,
    ILogger<MailKitEmailService> logger,
    Channel<EmailMessage> emailChannel
) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<MailKitEmailService> _logger = logger;
    private readonly Channel<EmailMessage> _emailChannel = emailChannel;
    
    /// <inheritdoc/>
    /// <summary>
    /// Envía un email de forma síncrona usando SMTP.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Lee configuración SMTP de appsettings.json</item>
    ///     <item>Crea mensaje MimeKit</item>
    ///     <item>Conecta a SMTP con StartTLS</item>
    ///     <li>Autentica y envía</item>
    /// </list>
    /// </remarks>
    public async Task SendEmailAsync(EmailMessage message)
    {
        try
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPassword = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"] ?? smtpUser;
            var fromName = _configuration["Smtp:FromName"] ?? "TiendaApi";

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
            {
                _logger.LogWarning("SMTP no configurado, omitiendo envío de email");
                return;
            }

            var mimeMessage = new MimeMessage();
            if (fromEmail != null) mimeMessage.From.Add(new MailboxAddress(fromName, fromEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(message.To));
            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder();
            if (message.IsHtml)
            {
                bodyBuilder.HtmlBody = message.Body;
            }
            else
            {
                bodyBuilder.TextBody = message.Body;
            }
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            if (smtpPassword != null) await client.AuthenticateAsync(smtpUser, smtpPassword);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email enviado exitosamente a: {To}", message.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a: {To}", message.To);
            throw;
        }
    }


    /// <inheritdoc/>
    /// <summary>
    /// Encola un email para ser procesado por el BackgroundService.
    /// </summary>
    /// <remarks>
    /// Escribe el mensaje en un Channel para procesamiento asíncrono.
    /// No bloquea el hilo actual.
    /// </remarks>
    public async Task EnqueueEmailAsync(EmailMessage message)
    {
        try
        {
            await _emailChannel.Writer.WriteAsync(message);
            _logger.LogInformation("Email encolado para procesamiento en segundo plano a: {To}", message.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al encolar email para: {To}", message.To);
        }
    }
}