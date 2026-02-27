using dawazonBackend.Common.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.Common.Cache;

[TestFixture]
[Description("CacheService Unit Tests")]
public class CacheServiceTest
{
    private Mock<IDistributedCache> _cacheMock;
    private Mock<ILogger<CacheService>> _loggerMock;
    private CacheService _service;

    [SetUp]
    public void SetUp()
    {
        _cacheMock  = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CacheService>>();
        _service    = new CacheService(_cacheMock.Object, _loggerMock.Object);
    }

    [Test]
    [Description("GetAsync: cuando la clave no existe (cache miss) debe retornar default")]
    public async Task GetAsync_WhenCacheMiss_ShouldReturnDefault()
    {
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);

        var result = await _service.GetAsync<string>("key:missing");

        Assert.That(result, Is.Null);
    }

    [Test]
    [Description("GetAsync: cuando la clave no existe debe loguear Debug ('Cache miss')")]
    public async Task GetAsync_WhenCacheMiss_ShouldLogDebug()
    {
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);

        await _service.GetAsync<string>("key:miss");

        _loggerMock.Verify(
            l => l.Log(LogLevel.Debug, It.IsAny<EventId>(),
                       It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(),
                       It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    [Description("GetAsync: cuando la clave existe debe deserializar y retornar el valor")]
    public async Task GetAsync_WhenCacheHit_ShouldDeserializeAndReturnValue()
    {
        var json  = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes("valor cacheado");
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(json);

        var result = await _service.GetAsync<string>("key:hit");

        Assert.That(result, Is.EqualTo("valor cacheado"));
    }

    [Test]
    [Description("GetAsync: cuando hay un objeto complejo, debe deserializarlo correctamente")]
    public async Task GetAsync_WhenComplexObject_ShouldDeserializeCorrectly()
    {
        var obj  = new { Name = "Test", Value = 42 };
        var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj);
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(json);

        var result = await _service.GetAsync<System.Text.Json.JsonElement>("key:obj");

        Assert.That(result.GetProperty("Name").GetString(), Is.EqualTo("Test"));
        Assert.That(result.GetProperty("Value").GetInt32(),  Is.EqualTo(42));
    }

    [Test]
    [Description("GetAsync: cuando el cache lanza excepción debe retornar default sin propagar")]
    public async Task GetAsync_WhenCacheThrows_ShouldReturnDefaultAndLogError()
    {
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new InvalidOperationException("Redis caído"));

        var result = await _service.GetAsync<string>("key:error");

        Assert.That(result, Is.Null);

        _loggerMock.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(),
                       It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                       It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    [Description("SetAsync: debe serializar el valor y llamar a SetAsync del cache con las opciones")]
    public async Task SetAsync_ShouldSerializeAndCallCacheSet()
    {
        _cacheMock.Setup(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SetAsync("key:set", "valor");

        _cacheMock.Verify(c => c.SetAsync(
            "key:set",
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [Description("SetAsync: con expiración personalizada debe usarla en las opciones")]
    public async Task SetAsync_WithCustomExpiration_ShouldUseIt()
    {
        var customExpiry = TimeSpan.FromMinutes(30);

        _cacheMock.Setup(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SetAsync("key:expiry", 42, customExpiry);

        _cacheMock.Verify(c => c.SetAsync(
            "key:expiry",
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == customExpiry),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    [Description("SetAsync: cuando el cache lanza excepción no debe propagar (solo loguear)")]
    public async Task SetAsync_WhenCacheThrows_ShouldNotPropagate()
    {
        _cacheMock.Setup(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis caído"));

        Assert.DoesNotThrowAsync(() => _service.SetAsync("key:fail", "data"));

        _loggerMock.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(),
                       It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                       It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await Task.CompletedTask;
    }

    [Test]
    [Description("RemoveAsync: debe llamar a Remove del cache con la clave correcta")]
    public async Task RemoveAsync_ShouldCallCacheRemove()
    {
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        await _service.RemoveAsync("key:remove");

        _cacheMock.Verify(c => c.RemoveAsync("key:remove", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [Description("RemoveAsync: cuando el cache lanza excepción no debe propagar")]
    public async Task RemoveAsync_WhenCacheThrows_ShouldNotPropagate()
    {
        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new Exception("Redis caído"));

        Assert.DoesNotThrowAsync(() => _service.RemoveAsync("key:fail"));

        _loggerMock.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(),
                       It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                       It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await Task.CompletedTask;
    }

    [Test]
    [Description("RemoveByPatternAsync: debe completar sin lanzar excepciones")]
    public async Task RemoveByPatternAsync_ShouldCompleteWithoutThrowing()
    {
        Assert.DoesNotThrowAsync(() => _service.RemoveByPatternAsync("products:*"));
        await Task.CompletedTask;
    }

    [Test]
    [Description("RemoveByPatternAsync: debe loguear el patrón a eliminar")]
    public async Task RemoveByPatternAsync_ShouldLogPattern()
    {
        await _service.RemoveByPatternAsync("user:*");

        _loggerMock.Verify(
            l => l.Log(LogLevel.Debug, It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("user:*")),
                       It.IsAny<Exception?>(),
                       It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}