namespace dawazonBackend.Common.Mail;

/// <summary>
/// Interfaz para el servicio de envío de correos electrónicos.
/// </summary>
/// <remarks>
/// Define el contrato para enviar emails de forma síncrona o encolada.
/// Implementada por MailKitEmailService.
/// </remarks>
public interface IEmailService
{
    /// <summary>
    /// Envía un correo de forma síncrona e inmediata.
    /// </summary>
    /// <param name="message">El mensaje a enviar.</param>
    /// <returns>Task de la operación.</returns>
    Task SendEmailAsync(EmailMessage message);
    
    /// <summary>
    /// Encola un correo para envío en segundo plano.
    /// </summary>
    /// <param name="message">El mensaje a encolar.</param>
    /// <returns>Task de la operación.</returns>
    Task EnqueueEmailAsync(EmailMessage message);
}