using dawazon2._0.Infraestructures;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Moq;
using NUnit.Framework;
using System.IO;

namespace dawazonTest.dawazon2._0.Infraestructure;

[TestFixture]
public class StorageInitializationTests
{
    private string _tempDirectory;
    private Mock<IWebHostEnvironment> _envMock;
    private WebApplicationBuilder _builder;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "dawazon_test_storage_" + Path.GetRandomFileName());
        
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_tempDirectory);

        _builder = WebApplication.CreateBuilder();
        _builder.Environment.WebRootPath = _tempDirectory;
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Test]
    public void InitializeStorage_ShouldCreateUploadsDirectory_WhenItDoesNotExist()
    {
        var app = _builder.Build();
        var uploadsPath = Path.Combine(_tempDirectory, "uploads");

        Assert.That(Directory.Exists(uploadsPath), Is.False);
        app.InitializeStorage();

        Assert.That(Directory.Exists(uploadsPath), Is.True);
    }

    [Test]
    public void InitializeStorage_ShouldNotThrow_WhenDirectoryAlreadyExists()
    {
        var app = _builder.Build();
        var uploadsPath = Path.Combine(_tempDirectory, "uploads");

        Directory.CreateDirectory(uploadsPath);
        Assert.That(Directory.Exists(uploadsPath), Is.True);

        Assert.DoesNotThrow(() => app.InitializeStorage());
    }
}
