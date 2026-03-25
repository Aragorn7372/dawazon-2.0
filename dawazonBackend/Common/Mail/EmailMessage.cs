namespace dawazonBackend.Common.Mail;

/// <summary>
/// Representa un mensaje de correo electrónico a enviar.
/// </summary>
/// <remarks>
/// Clase simple que contiene los datos necesarios para enviar un email.
/// Se utiliza como DTO entre las capas de la aplicación.
/// </remarks>
public class EmailMessage
{
    /// <summary>
    /// Destinatario del correo electrónico.
    /// </summary>
    /// <value>Dirección de email del destinatario.</value>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Asunto del correo electrónico.
    /// </summary>
    /// <value>Título o tema del email.</value>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Cuerpo del mensaje.
    /// </summary>
    /// <value>Contenido del email (HTML o texto plano).</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el cuerpo es HTML.
    /// </summary>
    /// <value>True si el Body contiene HTML, false si es texto plano.</value>
    public bool IsHtml { get; set; } = true;
}