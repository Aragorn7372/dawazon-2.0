using dawazon2._0.Infraestructures;
using dawazonBackend.Cart.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace dawazonTest.dawazon2._0.Infraestructure;

[TestFixture]
public class CartCleanupBackgroundServiceTests
{
    private Mock<IServiceScopeFactory> _scopeFactoryMock;
    private Mock<IServiceScope> _scopeMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<ICartService> _cartServiceMock;
    private Mock<ILogger<CartCleanupBackgroundService>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _scopeFactoryMock    = new Mock<IServiceScopeFactory>();
        _scopeMock           = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _cartServiceMock     = new Mock<ICartService>();
        _loggerMock          = new Mock<ILogger<CartCleanupBackgroundService>>();

        _scopeFactoryMock.Setup(s => s.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock
            .Setup(s => s.GetService(typeof(ICartService)))
            .Returns(_cartServiceMock.Object);
    }

    private CartCleanupBackgroundService BuildService()
        => new(_scopeFactoryMock.Object, _loggerMock.Object);

    private static async Task StartAndCancelAsync(
        CartCleanupBackgroundService svc,
        CancellationTokenSource cts,
        int startupWaitMs = 150,
        int stopWaitMs    = 150)
    {
        await svc.StartAsync(cts.Token);
        await Task.Delay(startupWaitMs);
        cts.Cancel();
        await Task.Delay(stopWaitMs);
        await svc.StopAsync(CancellationToken.None);
    }

    [Test]
    public void Constructor_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            using var svc = BuildService();
        });
    }

    [Test]
    public async Task StartAsync_ShouldLogInitiatedMessage()
    {
        using var cts = new CancellationTokenSource();
        using var svc = BuildService();

        await StartAndCancelAsync(svc, cts);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("iniciado")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task StopAsync_ShouldCompleteWithoutException()
    {
        using var cts = new CancellationTokenSource();
        using var svc = BuildService();

        await svc.StartAsync(cts.Token);
        await Task.Delay(150);
        cts.Cancel();

        Assert.DoesNotThrowAsync(async () => await svc.StopAsync(CancellationToken.None));
    }

    [Test]
    public async Task ExecuteAsync_ShouldNotCallCleanup_WhenCancelledDuringDelay()
    {
        using var cts = new CancellationTokenSource();
        using var svc = BuildService();

        await StartAndCancelAsync(svc, cts);

        _cartServiceMock.Verify(
            c => c.CleanupExpiredCheckoutsAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Test]
    public async Task StopAsync_IsIdempotent()
    {
        using var cts = new CancellationTokenSource();
        using var svc = BuildService();

        await svc.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();
        await Task.Delay(100);

        Assert.DoesNotThrowAsync(async () => await svc.StopAsync(CancellationToken.None));
        Assert.DoesNotThrowAsync(async () => await svc.StopAsync(CancellationToken.None));
    }

    [Test]
    public void Dispose_ShouldNotThrow()
    {
        var svc = BuildService();
        Assert.DoesNotThrow(() => svc.Dispose());
    }
}