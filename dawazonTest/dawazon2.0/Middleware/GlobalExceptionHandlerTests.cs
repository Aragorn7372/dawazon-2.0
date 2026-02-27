using dawazon2._0.Middleware;
using dawazonBackend.Cart.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace dawazonTest.dawazon2._0.Middleware;

[TestFixture]
public class GlobalExceptionHandlerTests
{
    private Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private DefaultHttpContext _httpContext;
    private MemoryStream _responseBodyStream;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _httpContext = new DefaultHttpContext();
        _responseBodyStream = new MemoryStream();
        _httpContext.Response.Body = _responseBodyStream;
    }

    private async Task<JsonDocument> GetResponseJsonAsync()
    {
        _responseBodyStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_responseBodyStream);
        var bodyText = await reader.ReadToEndAsync();
        return JsonDocument.Parse(bodyText);
    }

    [Test]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        var nextCalled = false;
        var handler = new GlobalExceptionHandler(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, _loggerMock.Object);

        await handler.InvokeAsync(_httpContext);

        Assert.That(nextCalled, Is.True);
        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task InvokeAsync_WhenCartNotFoundException_ShouldReturn404()
    {
        var exception = new CartNotFoundException("Cart not found");
        var handler = new GlobalExceptionHandler(ctx => throw exception, _loggerMock.Object);

        await handler.InvokeAsync(_httpContext);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(404));
        var json = await GetResponseJsonAsync();
        Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("Cart not found"));
        Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("NotFoundError"));
    }

    [Test]
    public async Task InvokeAsync_WhenUnauthorizedAccessException_ShouldReturn401()
    {
        var handler = new GlobalExceptionHandler(ctx => throw new UnauthorizedAccessException(), _loggerMock.Object);

        await handler.InvokeAsync(_httpContext);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(401));
        var json = await GetResponseJsonAsync();
        Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("UnauthorizedError"));
    }

    [Test]
    public async Task InvokeAsync_WhenArgumentException_ShouldReturn400()
    {
        var handler = new GlobalExceptionHandler(ctx => throw new ArgumentException("Bad argument"), _loggerMock.Object);

        await handler.InvokeAsync(_httpContext);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(400));
        var json = await GetResponseJsonAsync();
        Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo("Bad argument"));
        Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("ValidationError"));
    }

    [Test]
    public async Task InvokeAsync_WhenDbUpdateException_ShouldReturn409()
    {
        var handler = new GlobalExceptionHandler(ctx => throw new DbUpdateException(), _loggerMock.Object);

        await handler.InvokeAsync(_httpContext);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(409));
        var json = await GetResponseJsonAsync();
        Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("ConflictError"));
    }

    [Test]
    public async Task InvokeAsync_WhenTimeoutException_ShouldReturn408()
    {
        var handler = new GlobalExceptionHandler(ctx => throw new TimeoutException(), _loggerMock.Object);

        await handler.InvokeAsync(_httpContext);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(408));
        var json = await GetResponseJsonAsync();
        Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("InternalError"));
    }

    [Test]
    public async Task InvokeAsync_WhenGenericException_ShouldReturn500()
    {
        var handler = new GlobalExceptionHandler(ctx => throw new Exception("Unknown error"), _loggerMock.Object);

        await handler.InvokeAsync(_httpContext);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(500));
        var json = await GetResponseJsonAsync();
        Assert.That(json.RootElement.GetProperty("errorType").GetString(), Is.EqualTo("InternalError"));
    }

    [Test]
    public void UseGlobalExceptionHandler_ShouldRegisterMiddleware()
    {
        var appBuilderMock = new Mock<Microsoft.AspNetCore.Builder.IApplicationBuilder>();
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        appBuilderMock.Setup(a => a.ApplicationServices).Returns(serviceProviderMock.Object);
        
        // When Use is called, return the app builder itself to simulate fluent pattern
        appBuilderMock.Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(appBuilderMock.Object);

        var result = GlobalExceptionHandlerExtensions.UseGlobalExceptionHandler(appBuilderMock.Object);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(appBuilderMock.Object));
    }
}