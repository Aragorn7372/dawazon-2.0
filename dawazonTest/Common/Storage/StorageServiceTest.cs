using dawazonBackend.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.Common.Storage;

[TestFixture]
[Description("Storage Unit Tests")]
public class StorageServiceTest
{
    private Mock<ILogger<dawazonBackend.Common.Storage.Storage>> _loggerMock;
    private Mock<IWebHostEnvironment> _envMock;
    private string _tempDir;
    private dawazonBackend.Common.Storage.Storage _storage;

    private static IConfiguration BuildConfig(long maxFileSizeBytes = 5 * 1024 * 1024)
    {
        var dict = new Dictionary<string, string?>
        {
            ["Storage:UploadPath"]    = "uploads",
            ["Storage:MaxFileSize"]   = maxFileSizeBytes.ToString(),
            ["Storage:AllowedExtensions:0"]   = ".jpg",
            ["Storage:AllowedExtensions:1"]   = ".jpeg",
            ["Storage:AllowedExtensions:2"]   = ".png",
            ["Storage:AllowedExtensions:3"]   = ".gif",
            ["Storage:AllowedContentTypes:0"] = "image/jpeg",
            ["Storage:AllowedContentTypes:1"] = "image/png",
            ["Storage:AllowedContentTypes:2"] = "image/gif",
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"storage_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _loggerMock = new Mock<ILogger<dawazonBackend.Common.Storage.Storage>>();
        _envMock    = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_tempDir);

        _storage = new dawazonBackend.Common.Storage.Storage(
            BuildConfig(),
            _loggerMock.Object,
            _envMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Test]
    [Description("GetRelativePath debe retornar /{uploadPath}/{folder}/{filename}")]
    public void GetRelativePath_ShouldReturnCorrectRelativeUrl()
    {
        var result = _storage.GetRelativePath("image.jpg", "products");

        Assert.That(result, Is.EqualTo("/uploads/products/image.jpg"));
    }

    [Test]
    [Description("GetRelativePath sin carpeta explícita usa el default 'products' (tal como define Storage.cs)")]
    public void GetRelativePath_WithDefaultFolder_UsesProductsDefault()
    {
        var result = _storage.GetRelativePath("photo.png");

        Assert.That(result, Is.EqualTo("/uploads/products/photo.png"));
    }

    [Test]
    [Description("GetFullPath: cuando se pasa una ruta absoluta, la devuelve sin modificar")]
    public void GetFullPath_WhenAbsolutePath_ShouldReturnItAsIs()
    {
        var absPath = Path.Combine(_tempDir, "uploads", "test.jpg");
        var result  = _storage.GetFullPath(absPath);

        Assert.That(result, Is.EqualTo(absPath));
    }

    [Test]
    [Description("GetFullPath: en Windows, rutas que empiezan con '/' son 'rooted' y se devuelven sin modificar")]
    public void GetFullPath_WhenStorageSlashPrefix_OnWindows_ReturnsAsIs()
    {
        var input  = "/storage/products/img.jpg";
        var result = _storage.GetFullPath(input);

        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    [Description("GetFullPath: en Windows, /uploads/... también es 'rooted' y se devuelve sin modificar")]
    public void GetFullPath_WhenUploadPathPrefix_OnWindows_ReturnsAsIs()
    {
        var input  = "/uploads/products/img.jpg";
        var result = _storage.GetFullPath(input);

        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    [Description("GetFullPath: cuando filename es un nombre simple, combina con _rootPath")]
    public void GetFullPath_WhenSimpleFilename_ShouldCombineWithRootPath()
    {
        var result   = _storage.GetFullPath("simple.jpg");
        var expected = Path.Combine(_tempDir, "uploads", "simple.jpg");

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [Description("FileExists: con filename vacío debe retornar false")]
    public void FileExists_WhenFilenameIsEmpty_ShouldReturnFalse()
    {
        Assert.That(_storage.FileExists(string.Empty), Is.False);
    }

    [Test]
    [Description("FileExists: cuando el archivo no existe en disco debe retornar false")]
    public void FileExists_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        Assert.That(_storage.FileExists("nonexistent_file.jpg"), Is.False);
    }

    [Test]
    [Description("FileExists: cuando el archivo existe en disco debe retornar true")]
    public void FileExists_WhenFileExists_ShouldReturnTrue()
    {
        var uploadDir = Path.Combine(_tempDir, "uploads");
        Directory.CreateDirectory(uploadDir);
        var filename = "real_file.jpg";
        File.WriteAllText(Path.Combine(uploadDir, filename), "dummy");

        Assert.That(_storage.FileExists(filename), Is.True);
    }

    [Test]
    [Description("SaveFileAsync: con IFormFile null debe retornar Failure (archivo vacío)")]
    public async Task SaveFileAsync_WhenFileIsNull_ShouldReturnFailure()
    {
        var result = await _storage.SaveFileAsync(null!, "products");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("vacío").IgnoreCase);
    }

    [Test]
    [Description("SaveFileAsync: con tamaño 0 debe retornar Failure")]
    public async Task SaveFileAsync_WhenFileSizeIsZero_ShouldReturnFailure()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        var result = await _storage.SaveFileAsync(fileMock.Object, "products");

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    [Description("SaveFileAsync: con archivo mayor al límite debe retornar Failure")]
    public async Task SaveFileAsync_WhenFileTooLarge_ShouldReturnFailure()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(10 * 1024 * 1024);
        fileMock.Setup(f => f.FileName).Returns("big.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        var result = await _storage.SaveFileAsync(fileMock.Object, "products");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("grande").IgnoreCase);
    }

    [Test]
    [Description("SaveFileAsync: con extensión no permitida (.exe) debe retornar Failure")]
    public async Task SaveFileAsync_WhenExtensionNotAllowed_ShouldReturnFailure()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("virus.exe");
        fileMock.Setup(f => f.ContentType).Returns("application/octet-stream");

        var result = await _storage.SaveFileAsync(fileMock.Object, "products");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("Extensión").IgnoreCase);
    }

    [Test]
    [Description("SaveFileAsync: con ContentType no permitido debe retornar Failure")]
    public async Task SaveFileAsync_WhenContentTypeNotAllowed_ShouldReturnFailure()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("file.jpg");
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        var result = await _storage.SaveFileAsync(fileMock.Object, "products");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("contenido").IgnoreCase);
    }

    [Test]
    [Description("SaveFileAsync: filename con '..' literal (después de Path.GetFileName) debe retornar Failure")]
    public async Task SaveFileAsync_WhenPathTraversalInFilename_ShouldReturnFailure()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("file..jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        var result = await _storage.SaveFileAsync(fileMock.Object, "products");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Message, Does.Contain("válido").IgnoreCase);
    }

    [Test]
    [Description("SaveFileAsync: con archivo válido debe guardar en disco y retornar la ruta relativa")]
    public async Task SaveFileAsync_WhenFileIsValid_ShouldSaveAndReturnRelativePath()
    {
        var content  = "fake image content"u8.ToArray();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.FileName).Returns("photo.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.CopyTo(It.IsAny<Stream>()))
                .Callback<Stream>(s => s.Write(content, 0, content.Length));

        var result = await _storage.SaveFileAsync(fileMock.Object, "products");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Does.StartWith("/uploads/products/"));
        Assert.That(result.Value, Does.EndWith(".jpg"));
    }

    // ──────────────────────────────────────────
    //  DeleteFileAsync
    // ──────────────────────────────────────────

    [Test]
    [Description("DeleteFileAsync: con filename vacío debe retornar Success(true) sin tocar disco")]
    public async Task DeleteFileAsync_WhenFilenameIsEmpty_ShouldReturnSuccess()
    {
        var result = await _storage.DeleteFileAsync(string.Empty);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value,     Is.True);
    }

    [Test]
    [Description("DeleteFileAsync: cuando el archivo no existe en disco debe retornar Success(true)")]
    public async Task DeleteFileAsync_WhenFileDoesNotExist_ShouldReturnSuccess()
    {
        var result = await _storage.DeleteFileAsync("nonexistent.jpg");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value,     Is.True);
    }

    [Test]
    [Description("DeleteFileAsync: cuando el archivo existe debe eliminarlo y retornar Success(true)")]
    public async Task DeleteFileAsync_WhenFileExists_ShouldDeleteFileAndReturnSuccess()
    {
        // Arrange — creamos el archivo en el directorio de uploads
        var uploadDir = Path.Combine(_tempDir, "uploads");
        Directory.CreateDirectory(uploadDir);
        var filename  = "to_delete.jpg";
        var fullPath  = Path.Combine(uploadDir, filename);
        await File.WriteAllTextAsync(fullPath, "dummy");

        Assert.That(File.Exists(fullPath), Is.True, "Precondición: el archivo debe existir antes del test");

        // Act
        var result = await _storage.DeleteFileAsync(filename);

        // Assert
        Assert.That(result.IsSuccess,      Is.True);
        Assert.That(result.Value,          Is.True);
        Assert.That(File.Exists(fullPath), Is.False, "El archivo debe haber sido eliminado del disco");
    }

    [Test]
    [Description("DeleteFileAsync: con null como filename debe retornar Success(true)")]
    public async Task DeleteFileAsync_WhenFilenameIsNull_ShouldReturnSuccess()
    {
        var result = await _storage.DeleteFileAsync(null!);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    [Description("Constructor: debe crear la carpeta de uploads si no existía previamente")]
    public void Constructor_WhenUploadDirectoryDoesNotExist_ShouldCreateIt()
    {
        // Arrange — usamos un subdirectorio que aún no existe
        var freshRoot = Path.Combine(Path.GetTempPath(), $"fresh_{Guid.NewGuid():N}");
        Directory.CreateDirectory(freshRoot);

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(freshRoot);

        try
        {
            // Act
            var s = new dawazonBackend.Common.Storage.Storage(BuildConfig(), _loggerMock.Object, envMock.Object);

            // Assert
            Assert.That(Directory.Exists(Path.Combine(freshRoot, "uploads")), Is.True);
        }
        finally
        {
            Directory.Delete(freshRoot, recursive: true);
        }
    }
}