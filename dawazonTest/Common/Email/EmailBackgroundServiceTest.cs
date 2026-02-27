using System.Threading.Channels;
using dawazonBackend.Common.Mail;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.Common.Email;

[TestFixture]
[Description("EmailBackgroundService Unit Tests")]
public class EmailBackgroundServiceTest
{
    private Mock<IEmailService> _emailServiceMock;
    private Mock<ILogger<EmailBackgroundService>> _loggerMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IServiceScope> _scopeMock;
    private Mock<IServiceScopeFactory> _scopeFactoryMock;

    [SetUp]
    public void SetUp()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<EmailBackgroundService>>();

        _scopeMock = new Mock<IServiceScope>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IEmailService)))
                  .Returns(_emailServiceMock.Object);

        _scopeFactoryMock.Setup(f => f.CreateScope())
                         .Returns(_scopeMock.Object);

        _serviceProviderMock
            .Setup(p => p.GetService(typeof(IServiceScopeFactory)))
            .Returns(_scopeFactoryMock.Object);
    }

    [Test]
    [Description("ExecuteAsync: cuando hay un mensaje en el canal, debe llamar a SendEmailAsync")]
    public async Task ExecuteAsync_WhenMessageInChannel_ShouldCallSendEmailAsync()
    {
        var channel = Channel.CreateUnbounded<EmailMessage>();
        var message = new EmailMessage
        {
            To = "user@example.com",
            Subject = "Test",
            Body = "<p>Hola</p>",
            IsHtml = true
        };
        await channel.Writer.WriteAsync(message);

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<EmailMessage>()))
            .Returns(Task.CompletedTask);

        var service = new EmailBackgroundService(channel, _serviceProviderMock.Object, _loggerMock.Object);

        using var cts = new CancellationTokenSource();
        var executeTask = service.StartAsync(cts.Token);

        await Task.Delay(200);
        channel.Writer.Complete();
        await service.StopAsync(CancellationToken.None);

        _emailServiceMock.Verify(s => s.SendEmailAsync(It.Is<EmailMessage>(m => m.To == "user@example.com")),
            Times.Once);
    }

    [Test]
    [Description("ExecuteAsync: cuando SendEmailAsync lanza, debe loguear el error y NO propagar la excepción")]
    public async Task ExecuteAsync_WhenEmailServiceThrows_ShouldLogErrorAndContinue()
    {
        var channel = Channel.CreateUnbounded<EmailMessage>();
        var message = new EmailMessage { To = "fail@example.com", Subject = "Fail", Body = "Boom" };
        await channel.Writer.WriteAsync(message);

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<EmailMessage>()))
            .ThrowsAsync(new InvalidOperationException("SMTP error simulado"));

        var service = new EmailBackgroundService(channel, _serviceProviderMock.Object, _loggerMock.Object);

        using var cts = new CancellationTokenSource();
        _ = service.StartAsync(cts.Token);

        await Task.Delay(200);
        channel.Writer.Complete();
        await service.StopAsync(CancellationToken.None);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("fail@example.com")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    [Description("ExecuteAsync: con canal vacío y cancelado, no debe llamar a SendEmailAsync")]
    public async Task ExecuteAsync_WhenChannelIsEmpty_ShouldNotCallSendEmailAsync()
    {
        var channel = Channel.CreateUnbounded<EmailMessage>();
        var service = new EmailBackgroundService(channel, _serviceProviderMock.Object, _loggerMock.Object);

        using var cts = new CancellationTokenSource();
        _ = service.StartAsync(cts.Token);

        channel.Writer.Complete();
        await service.StopAsync(CancellationToken.None);

        _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Test]
    [Description("ExecuteAsync: con N mensajes en canal, debe procesar todos en orden")]
    public async Task ExecuteAsync_WithMultipleMessages_ShouldProcessAll()
    {
        const int count = 3;
        var channel = Channel.CreateUnbounded<EmailMessage>();
        for (int i = 0; i < count; i++)
            await channel.Writer.WriteAsync(new EmailMessage { To = $"user{i}@test.com", Subject = $"S{i}", Body = "b" });

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<EmailMessage>()))
            .Returns(Task.CompletedTask);

        var service = new EmailBackgroundService(channel, _serviceProviderMock.Object, _loggerMock.Object);

        using var cts = new CancellationTokenSource();
        _ = service.StartAsync(cts.Token);

        await Task.Delay(400);
        channel.Writer.Complete();
        await service.StopAsync(CancellationToken.None);

        _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<EmailMessage>()), Times.Exactly(count));
    }
}