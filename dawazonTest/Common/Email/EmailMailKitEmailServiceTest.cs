using System.Threading.Channels;
using dawazonBackend.Common.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.Common.Email;

[TestFixture]
[Description("MailKitEmailService Unit Tests")]
public class EmailMailKitEmailServiceTest
{
    private Mock<IConfiguration> _configMock;
    private Mock<ILogger<MailKitEmailService>> _loggerMock;
    private Channel<EmailMessage> _channel;
    private MailKitEmailService _service;

    [SetUp]
    public void SetUp()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<MailKitEmailService>>();
        _channel = Channel.CreateUnbounded<EmailMessage>();

        _service = new MailKitEmailService(_configMock.Object, _loggerMock.Object, _channel);
    }

    [Test]
    [Description("SendEmailAsync: cuando el host SMTP no está configurado, debe loguear warning y retornar sin lanzar")]
    public async Task SendEmailAsync_WhenSmtpHostNotConfigured_ShouldLogWarningAndReturn()
    {
        _configMock.Setup(c => c["Smtp:Host"]).Returns((string?)null);
        _configMock.Setup(c => c["Smtp:Port"]).Returns("587");
        _configMock.Setup(c => c["Smtp:Username"]).Returns("user@example.com");
        _configMock.Setup(c => c["Smtp:Password"]).Returns("secret");

        var message = new EmailMessage { To = "dest@example.com", Subject = "S", Body = "<b>ok</b>", IsHtml = true };

        Assert.DoesNotThrowAsync(() => _service.SendEmailAsync(message));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await Task.CompletedTask;
    }

    [Test]
    [Description("SendEmailAsync: cuando el usuario SMTP no está configurado, debe loguear warning y retornar sin lanzar")]
    public async Task SendEmailAsync_WhenSmtpUsernameNotConfigured_ShouldLogWarningAndReturn()
    {
        _configMock.Setup(c => c["Smtp:Host"]).Returns("smtp.example.com");
        _configMock.Setup(c => c["Smtp:Port"]).Returns("587");
        _configMock.Setup(c => c["Smtp:Username"]).Returns((string?)null);

        var message = new EmailMessage { To = "dest@example.com", Subject = "S", Body = "txt", IsHtml = false };

        Assert.DoesNotThrowAsync(() => _service.SendEmailAsync(message));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await Task.CompletedTask;
    }

    [Test]
    [Description("EnqueueEmailAsync: debe escribir el mensaje en el canal para procesado en background")]
    public async Task EnqueueEmailAsync_ShouldWriteMessageToChannel()
    {
        var message = new EmailMessage
        {
            To      = "queue@example.com",
            Subject = "Encolado",
            Body    = "<p>test</p>",
            IsHtml  = true
        };

        await _service.EnqueueEmailAsync(message);

        Assert.That(_channel.Reader.TryRead(out var dequeued), Is.True);
        Assert.That(dequeued!.To, Is.EqualTo("queue@example.com"));
    }

    [Test]
    [Description("EnqueueEmailAsync: debe loguear información al encolar correctamente")]
    public async Task EnqueueEmailAsync_ShouldLogInformationOnSuccess()
    {
        var message = new EmailMessage { To = "log@example.com", Subject = "Log", Body = "body" };

        await _service.EnqueueEmailAsync(message);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("log@example.com")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    [Description("EnqueueEmailAsync: cuando el canal está cerrado (ChannelClosedException), debe loguear el error sin propagar")]
    public async Task EnqueueEmailAsync_WhenChannelClosed_ShouldLogErrorAndNotThrow()
    {
        _channel.Writer.Complete();

        var message = new EmailMessage { To = "closed@example.com", Subject = "S", Body = "b" };
        Assert.DoesNotThrowAsync(() => _service.EnqueueEmailAsync(message));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await Task.CompletedTask;
    }

    [Test]
    [Description("SendEmailAsync: cuando SMTP está configurado pero el servidor no es alcanzable, debe loguear error y relanzar excepción (cubre la rama IsHtml=false y la ruta SMTP activa)")]
    public async Task SendEmailAsync_WhenSmtpConfiguredButUnreachable_ShouldLogErrorAndThrow()
    {
        _configMock.Setup(c => c["Smtp:Host"]).Returns("127.0.0.1");
        _configMock.Setup(c => c["Smtp:Port"]).Returns("19999"); // puerto que no escucha
        _configMock.Setup(c => c["Smtp:Username"]).Returns("user@example.com");
        _configMock.Setup(c => c["Smtp:Password"]).Returns("secret");
        _configMock.Setup(c => c["Smtp:FromEmail"]).Returns((string?)null);
        _configMock.Setup(c => c["Smtp:FromName"]).Returns((string?)null);

        var message = new EmailMessage
        {
            To      = "dest@example.com",
            Subject = "Prueba",
            Body    = "Texto plano sin HTML",
            IsHtml  = false   // ← cubre la rama TextBody
        };

        Assert.CatchAsync(async () => await _service.SendEmailAsync(message));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await Task.CompletedTask;
    }

    [Test]
    [Description("SendEmailAsync: cuando SMTP está configurado con HTML body pero el servidor no es alcanzable, debe loguear error y relanzar excepción")]
    public async Task SendEmailAsync_WhenSmtpConfiguredWithHtmlBody_AndUnreachable_ShouldLogErrorAndThrow()
    {
        _configMock.Setup(c => c["Smtp:Host"]).Returns("127.0.0.1");
        _configMock.Setup(c => c["Smtp:Port"]).Returns("19999");
        _configMock.Setup(c => c["Smtp:Username"]).Returns("user@example.com");
        _configMock.Setup(c => c["Smtp:Password"]).Returns("secret");
        _configMock.Setup(c => c["Smtp:FromEmail"]).Returns("from@example.com");
        _configMock.Setup(c => c["Smtp:FromName"]).Returns("Tienda");

        var message = new EmailMessage
        {
            To      = "dest@example.com",
            Subject = "Prueba HTML",
            Body    = "<b>HTML body</b>",
            IsHtml  = true
        };

        Assert.CatchAsync(async () => await _service.SendEmailAsync(message));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await Task.CompletedTask;
    }

    [Test]
    [Description("EmailMessage: los valores por defecto deben ser seguros (IsHtml=true, cadenas vacías)")]
    public void EmailMessage_DefaultValues_ShouldBeSafeDefaults()
    {
        var msg = new EmailMessage();

        Assert.That(msg.To,      Is.EqualTo(string.Empty));
        Assert.That(msg.Subject, Is.EqualTo(string.Empty));
        Assert.That(msg.Body,    Is.EqualTo(string.Empty));
        Assert.That(msg.IsHtml,  Is.True);
    }
}