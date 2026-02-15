namespace dawazonBackend.Common.Mail;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message);
    
    Task EnqueueEmailAsync(EmailMessage message);
}