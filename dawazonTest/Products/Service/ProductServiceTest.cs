using CSharpFunctionalExtensions;
using dawazonBackend.Common.Cache;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Storage;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Products.Repository.Categoria;
using dawazonBackend.Products.Repository.Productos;
using dawazonBackend.Products.Service;
using dawazonBackend.Cart.Service;
using dawazonBackend.Users.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.Products.Service;

[TestFixture]
[Description("ProductService Unit Tests")]
public class ProductServiceTest
{
    private Mock<ICacheService> _cacheMock;
    private Mock<IProductRepository> _productRepoMock;
    private Mock<ICategoriaRepository> _categoryRepoMock;
    private Mock<IStorage> _storageMock;
    private Mock<ICartService> _cartServiceMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<ILogger<ProductService>> _loggerMock;

    private ProductService _service;

    private const string ProductId = "PRD000000001";
    private const string ProductName = "Producto Test";
    private const double ProductPrice = 49.99;
    private const int ProductStock = 100;
    private const string ProductDesc = "Descripción de prueba";
    private const string CategoryId = "CAT001";
    private const string CategoryName = "Electrónica";
    private const long CreatorId = 5;

    [SetUp]
    public void SetUp()
    {
        _cacheMock = new Mock<ICacheService>();
        _productRepoMock = new Mock<IProductRepository>();
        _categoryRepoMock = new Mock<ICategoriaRepository>();
        _storageMock = new Mock<IStorage>();
        _cartServiceMock = new Mock<ICartService>();
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<ProductService>>();

        _service = new ProductService(
            _cacheMock.Object,
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _storageMock.Object,
            _cartServiceMock.Object,
            _userServiceMock.Object,
            _loggerMock.Object
        );
    }

    private static Product CreateProduct(string? id = ProductId, Category? category = null) => new()
    {
        Id = id,
        Name = ProductName,
        Price = ProductPrice,
        Stock = ProductStock,
        Description = ProductDesc,
        CategoryId = CategoryId,
        Category = category ?? new Category { Id = CategoryId, Name = CategoryName },
        Comments = [],
        Images = [],
        CreatorId = CreatorId
    };

    private static Category CreateCategory() => new() { Id = CategoryId, Name = CategoryName };

    private static FilterDto DefaultFilter() => new(Nombre: null, Categoria: null);

    private static ProductRequestDto CreateRequestDto() =>
        new(null, ProductName, ProductPrice, CategoryName, ProductDesc, [], ProductStock, CreatorId);

    [Test]
    [Description("GetByIdAsync: Cache hit — debe retornar el producto cacheado sin consultar la BD")]
    public async Task GetByIdAsync_WhenCacheHit_ShouldReturnCachedProduct()
    {
        var product = CreateProduct();
        _cacheMock.Setup(c => c.GetAsync<Product>(It.IsAny<string>()))
                  .ReturnsAsync(product);
        var result = await _service.GetByIdAsync(ProductId);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(ProductId));
        Assert.That(result.Value.Name, Is.EqualTo(ProductName));

        _productRepoMock.Verify(r => r.GetProductAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("GetByIdAsync: Cache miss + encontrado en BD — debe guardar en caché y retornar DTO")]
    public async Task GetByIdAsync_WhenCacheMissAndFoundInDb_ShouldCacheAndReturn()
    {
        var product = CreateProduct();
        _cacheMock.Setup(c => c.GetAsync<Product>(It.IsAny<string>()))
                  .ReturnsAsync((Product?)null);
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId))
                        .ReturnsAsync(product);
        var result = await _service.GetByIdAsync(ProductId);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(ProductId));

        _cacheMock.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains(ProductId)),
            It.IsAny<Product>(),
            It.IsAny<TimeSpan>()), Times.Once);
    }

    [Test]
    [Description("GetByIdAsync: Cache miss + no encontrado en BD — debe retornar ProductNotFoundError")]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNotFoundError()
    {
        _cacheMock.Setup(c => c.GetAsync<Product>(It.IsAny<string>()))
                  .ReturnsAsync((Product?)null);
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId))
                        .ReturnsAsync((Product?)null);
        var result = await _service.GetByIdAsync(ProductId);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("GetUserProductIdAsync: Cuando el producto existe debe retornar el CreatorId")]
    public async Task GetUserProductIdAsync_WhenProductExists_ShouldReturnCreatorId()
    {
        var product = CreateProduct();
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var result = await _service.GetUserProductIdAsync(ProductId);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(CreatorId));
    }

    [Test]
    [Description("GetUserProductIdAsync: Cuando no existe debe retornar ProductNotFoundError")]
    public async Task GetUserProductIdAsync_WhenNotFound_ShouldReturnNotFoundError()
    {
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync((Product?)null);

        var result = await _service.GetUserProductIdAsync(ProductId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("GetAllAsync: Sin creatorId debe llamar a repository.GetAllAsync")]
    public async Task GetAllAsync_WithoutCreatorId_ShouldCallGetAllAsync()
    {
        var filter = DefaultFilter();
        var products = new List<Product> { CreateProduct() };
        _productRepoMock.Setup(r => r.GetAllAsync(filter))
                        .ReturnsAsync((products.AsEnumerable(), 1));
        var result = await _service.GetAllAsync(filter);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        Assert.That(result.TotalElements, Is.EqualTo(1));
        _productRepoMock.Verify(r => r.GetAllAsync(filter), Times.Once);
        _productRepoMock.Verify(r => r.FindAllByCreatorId(It.IsAny<long>(), It.IsAny<FilterDto>()), Times.Never);
    }

    [Test]
    [Description("GetAllAsync: Con creatorId debe llamar a repository.FindAllByCreatorId")]
    public async Task GetAllAsync_WithCreatorId_ShouldCallFindAllByCreatorId()
    {
        var filter = DefaultFilter();
        var products = new List<Product> { CreateProduct() };
        _productRepoMock.Setup(r => r.FindAllByCreatorId(CreatorId, filter))
                        .ReturnsAsync((products.AsEnumerable(), 1));
        var result = await _service.GetAllAsync(filter, CreatorId);
        Assert.That(result.Content, Has.Count.EqualTo(1));
        _productRepoMock.Verify(r => r.FindAllByCreatorId(CreatorId, filter), Times.Once);
        _productRepoMock.Verify(r => r.GetAllAsync(It.IsAny<FilterDto>()), Times.Never);
    }

    [Test]
    [Description("GetAllAsync: Debe calcular TotalPages correctamente")]
    public async Task GetAllAsync_ShouldCalculateTotalPagesCorrectly()
    {
        var filter = new FilterDto(null, null, Page: 0, Size: 3);
        var products = new List<Product> { CreateProduct(), CreateProduct("PRD2") };
        _productRepoMock.Setup(r => r.GetAllAsync(filter))
                        .ReturnsAsync((products.AsEnumerable(), 7));

        var result = await _service.GetAllAsync(filter);

        Assert.That(result.TotalPages, Is.EqualTo(3)); // ceil(7/3) = 3
        Assert.That(result.TotalElements, Is.EqualTo(7));
        Assert.That(result.PageSize, Is.EqualTo(3));
        Assert.That(result.PageNumber, Is.EqualTo(0));
        Assert.That(result.TotalPageElements, Is.EqualTo(2));
        Assert.That(result.SortBy, Is.EqualTo("id"));
        Assert.That(result.Direction, Is.EqualTo("asc"));
    }

    [Test]
    [Description("GetAllAsync: Cuando no hay productos debe retornar lista vacía")]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyPage()
    {
        var filter = DefaultFilter();
        _productRepoMock.Setup(r => r.GetAllAsync(filter))
                        .ReturnsAsync((Enumerable.Empty<Product>(), 0));

        var result = await _service.GetAllAsync(filter);

        Assert.That(result.Content, Is.Empty);
        Assert.That(result.TotalElements, Is.EqualTo(0));
        Assert.That(result.Empty, Is.True);
    }

    [Test]
    [Description("GetAllCategoriesAsync: Debe delegar a categoryRepository.GetCategoriesAsync")]
    public async Task GetAllCategoriesAsync_ShouldDelegateToCategoryRepository()
    {
        var categories = new List<string> { "Electrónica", "Ropa", "Hogar" };
        _categoryRepoMock.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(categories);

        var result = await _service.GetAllCategoriesAsync();

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result, Contains.Item("Electrónica"));
        _categoryRepoMock.Verify(r => r.GetCategoriesAsync(), Times.Once);
    }

    [Test]
    [Description("CreateAsync: Con categoría válida debe crear el producto y retornar DTO")]
    public async Task CreateAsync_WhenCategoryExists_ShouldCreateAndReturnDto()
    {
        var category = CreateCategory();
        var dto = CreateRequestDto();
        var savedProduct = CreateProduct();

        _categoryRepoMock.Setup(r => r.GetByNameAsync(CategoryName)).ReturnsAsync(category);
        _productRepoMock.Setup(r => r.CreateProductAsync(It.IsAny<Product>())).ReturnsAsync(savedProduct);

        var result = await _service.CreateAsync(dto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo(ProductName));
        _productRepoMock.Verify(r => r.CreateProductAsync(It.IsAny<Product>()), Times.Once);
    }

    [Test]
    [Description("CreateAsync: Con categoría inexistente debe retornar ProductConflictError")]
    public async Task CreateAsync_WhenCategoryNotFound_ShouldReturnConflictError()
    {
        _categoryRepoMock.Setup(r => r.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((Category?)null);

        var result = await _service.CreateAsync(CreateRequestDto());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductConflictError>());
    }

    [Test]
    [Description("CreateAsync: Debe generar un ID cuando el DTO no tiene Id")]
    public async Task CreateAsync_WhenDtoHasNoId_ShouldGenerateId()
    {
        var category = CreateCategory();
        _categoryRepoMock.Setup(r => r.GetByNameAsync(CategoryName)).ReturnsAsync(category);
        _productRepoMock.Setup(r => r.CreateProductAsync(It.IsAny<Product>()))
                        .ReturnsAsync((Product p) => p);
        var result = await _service.CreateAsync(CreateRequestDto());
        Assert.That(result.IsSuccess, Is.True);
        _productRepoMock.Verify(r => r.CreateProductAsync(
            It.Is<Product>(p => p.Id != null && p.Id.StartsWith("PRD"))), Times.Once);
    }

    [Test]
    [Description("CreateAsync: Debe asignar CategoryId y Category del repositorio al modelo")]
    public async Task CreateAsync_ShouldAssignCategoryFromRepository()
    {
        var category = CreateCategory();
        _categoryRepoMock.Setup(r => r.GetByNameAsync(CategoryName)).ReturnsAsync(category);
        _productRepoMock.Setup(r => r.CreateProductAsync(It.IsAny<Product>()))
                        .ReturnsAsync((Product p) => p);

        await _service.CreateAsync(CreateRequestDto());

        _productRepoMock.Verify(r => r.CreateProductAsync(
            It.Is<Product>(p => p.CategoryId == CategoryId && p.Category == category)), Times.Once);
    }

    [Test]
    [Description("UpdateAsync: Con categoría y producto existentes debe actualizar, invalidar caché y retornar DTO")]
    public async Task UpdateAsync_WhenAllValid_ShouldUpdateAndInvalidateCache()
    {
        var category = CreateCategory();
        var existingProduct = CreateProduct();
        var dto = CreateRequestDto();

        _categoryRepoMock.Setup(r => r.GetByNameAsync(CategoryName)).ReturnsAsync(category);
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync(existingProduct);

        var result = await _service.UpdateAsync(ProductId, dto);

        Assert.That(result.IsSuccess, Is.True);
        _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k.Contains(ProductId))), Times.Once);
    }

    [Test]
    [Description("UpdateAsync: Con categoría inexistente debe retornar ProductConflictError")]
    public async Task UpdateAsync_WhenCategoryNotFound_ShouldReturnConflictError()
    {
        _categoryRepoMock.Setup(r => r.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((Category?)null);

        var result = await _service.UpdateAsync(ProductId, CreateRequestDto());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductConflictError>());
    }

    [Test]
    [Description("UpdateAsync: Cuando el producto no existe debe retornar ProductNotFoundError")]
    public async Task UpdateAsync_WhenProductNotFound_ShouldReturnNotFoundError()
    {
        var category = CreateCategory();
        _categoryRepoMock.Setup(r => r.GetByNameAsync(CategoryName)).ReturnsAsync(category);
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync((Product?)null);

        var result = await _service.UpdateAsync(ProductId, CreateRequestDto());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("PatchAsync: Producto no encontrado debe retornar ProductNotFoundError")]
    public async Task PatchAsync_WhenProductNotFound_ShouldReturnNotFoundError()
    {
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync((Product?)null);
        var patchDto = new ProductPatchRequestDto(null, "Nuevo", null, null, null, null, null, null);

        var result = await _service.PatchAsync(ProductId, patchDto);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("PatchAsync: Debe actualizar solo Name cuando se envía")]
    public async Task PatchAsync_WithNameOnly_ShouldUpdateOnlyName()
    {
        var product = CreateProduct();
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync(product);

        var patchDto = new ProductPatchRequestDto(null, "Nombre Nuevo", null, null, null, null, null, null);

        var result = await _service.PatchAsync(ProductId, patchDto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(product.Name, Is.EqualTo("Nombre Nuevo"));
        _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k.Contains(ProductId))), Times.Once);
    }

    [Test]
    [Description("PatchAsync: Debe actualizar Price cuando se envía")]
    public async Task PatchAsync_WithPrice_ShouldUpdatePrice()
    {
        var product = CreateProduct();
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync(product);

        var patchDto = new ProductPatchRequestDto(null, null, 199.99, null, null, null, null, null);

        await _service.PatchAsync(ProductId, patchDto);

        Assert.That(product.Price, Is.EqualTo(199.99));
    }

    [Test]
    [Description("PatchAsync: Debe actualizar Images cuando se envía")]
    public async Task PatchAsync_WithImages_ShouldUpdateImages()
    {
        var product = CreateProduct();
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync(product);

        var newImages = new List<string> { "nueva1.jpg", "nueva2.jpg" };
        var patchDto = new ProductPatchRequestDto(null, null, null, null, null, newImages, null, null);

        await _service.PatchAsync(ProductId, patchDto);

        Assert.That(product.Images, Is.EqualTo(newImages));
    }

    [Test]
    [Description("PatchAsync: Con categoría válida debe actualizar Category y CategoryId")]
    public async Task PatchAsync_WithValidCategory_ShouldUpdateCategory()
    {
        var product = CreateProduct();
        var newCategory = new Category { Id = "CAT002", Name = "Ropa" };

        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _categoryRepoMock.Setup(r => r.GetByNameAsync("Ropa")).ReturnsAsync(newCategory);
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync(product);

        var patchDto = new ProductPatchRequestDto(null, null, null, "Ropa", null, null, null, null);

        var result = await _service.PatchAsync(ProductId, patchDto);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(product.CategoryId, Is.EqualTo("CAT002"));
        Assert.That(product.Category, Is.EqualTo(newCategory));
    }

    [Test]
    [Description("PatchAsync: Con categoría inexistente debe retornar ProductConflictError")]
    public async Task PatchAsync_WithInvalidCategory_ShouldReturnConflictError()
    {
        var product = CreateProduct();
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _categoryRepoMock.Setup(r => r.GetByNameAsync("Inexistente")).ReturnsAsync((Category?)null);

        var patchDto = new ProductPatchRequestDto(null, null, null, "Inexistente", null, null, null, null);

        var result = await _service.PatchAsync(ProductId, patchDto);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductConflictError>());
    }

    [Test]
    [Description("UpdateImageAsync: Producto no encontrado debe retornar ProductNotFoundError")]
    public async Task UpdateImageAsync_WhenProductNotFound_ShouldReturnNotFoundError()
    {
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync((Product?)null);

        var result = await _service.UpdateImageAsync(ProductId, []);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("UpdateImageAsync: Sin imágenes válidas (Length 0) debe retornar producto sin cambios")]
    public async Task UpdateImageAsync_WhenNoValidImages_ShouldReturnUnchanged()
    {
        var product = CreateProduct();
        product.Images = ["existing.jpg"];
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var emptyFile = new Mock<IFormFile>();
        emptyFile.Setup(f => f.Length).Returns(0);

        var result = await _service.UpdateImageAsync(ProductId, [emptyFile.Object]);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Images, Contains.Item("existing.jpg"));
        _storageMock.Verify(s => s.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("UpdateImageAsync: Con imágenes válidas debe eliminar las existentes y guardar las nuevas")]
    public async Task UpdateImageAsync_WithValidImages_ShouldDeleteOldAndSaveNew()
    {
        var product = CreateProduct();
        product.Images = ["old1.jpg", "old2.jpg"];
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var validFile = new Mock<IFormFile>();
        validFile.Setup(f => f.Length).Returns(1024);

        _storageMock.Setup(s => s.DeleteFileAsync(It.IsAny<string>()))
                    .ReturnsAsync(Result.Success<bool, ProductError>(true));
        _storageMock.Setup(s => s.SaveFileAsync(validFile.Object, "products"))
                    .ReturnsAsync(Result.Success<string, ProductError>("new_image.jpg"));

        var updatedProduct = CreateProduct();
        updatedProduct.Images = ["new_image.jpg"];
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync(updatedProduct);

        var result = await _service.UpdateImageAsync(ProductId, [validFile.Object]);

        Assert.That(result.IsSuccess, Is.True);
        _storageMock.Verify(s => s.DeleteFileAsync("old1.jpg"), Times.Once);
        _storageMock.Verify(s => s.DeleteFileAsync("old2.jpg"), Times.Once);
        _storageMock.Verify(s => s.SaveFileAsync(validFile.Object, "products"), Times.Once);
    }

    [Test]
    [Description("UpdateImageAsync: Sin imágenes existentes no debe llamar a DeleteFileAsync")]
    public async Task UpdateImageAsync_WhenNoExistingImages_ShouldNotCallDelete()
    {
        var product = CreateProduct();
        product.Images = [];
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var validFile = new Mock<IFormFile>();
        validFile.Setup(f => f.Length).Returns(512);

        _storageMock.Setup(s => s.SaveFileAsync(validFile.Object, "products"))
                    .ReturnsAsync(Result.Success<string, ProductError>("img.jpg"));

        var updatedProduct = CreateProduct();
        updatedProduct.Images = ["img.jpg"];
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync(updatedProduct);

        await _service.UpdateImageAsync(ProductId, [validFile.Object]);

        _storageMock.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("UpdateImageAsync: Cuando SaveFileAsync falla no debe añadir la ruta a Images")]
    public async Task UpdateImageAsync_WhenSaveFails_ShouldNotAddPath()
    {
        var product = CreateProduct();
        product.Images = [];
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var validFile = new Mock<IFormFile>();
        validFile.Setup(f => f.Length).Returns(512);

        _storageMock.Setup(s => s.SaveFileAsync(validFile.Object, "products"))
                    .ReturnsAsync(Result.Failure<string, ProductError>(new ProductStorageError("Error de storage")));

        var updatedProduct = CreateProduct();
        updatedProduct.Images = [];
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync(updatedProduct);

        var result = await _service.UpdateImageAsync(ProductId, [validFile.Object]);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Images, Is.Empty);
    }

    [Test]
    [Description("UpdateImageAsync: Cuando UpdateProductAsync retorna null debe retornar ProductNotFoundError")]
    public async Task UpdateImageAsync_WhenUpdateReturnsNull_ShouldReturnNotFoundError()
    {
        var product = CreateProduct();
        product.Images = [];
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);

        var validFile = new Mock<IFormFile>();
        validFile.Setup(f => f.Length).Returns(512);

        _storageMock.Setup(s => s.SaveFileAsync(validFile.Object, "products"))
                    .ReturnsAsync(Result.Success<string, ProductError>("img.jpg"));
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync((Product?)null);

        var result = await _service.UpdateImageAsync(ProductId, [validFile.Object]);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("AddCommentAsync: Producto encontrado debe añadir el comentario, invalidar caché y retornar DTO")]
    public async Task AddCommentAsync_WhenProductFound_ShouldAddCommentAndInvalidateCache()
    {
        var product = CreateProduct();
        var comment = new Comment { UserId = 10, Content = "Gran producto", recommended = true, verified = false };

        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>(), ProductId))
                        .ReturnsAsync(product);

        var result = await _service.AddCommentAsync(ProductId, comment);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(product.Comments, Has.Count.EqualTo(1));
        Assert.That(product.Comments[0].Content, Is.EqualTo("Gran producto"));
        _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k.Contains(ProductId))), Times.Once);
    }

    [Test]
    [Description("AddCommentAsync: Producto no encontrado debe retornar ProductNotFoundError")]
    public async Task AddCommentAsync_WhenProductNotFound_ShouldReturnNotFoundError()
    {
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync((Product?)null);
        var comment = new Comment { UserId = 1, Content = "test" };

        var result = await _service.AddCommentAsync(ProductId, comment);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("DeleteAsync: Producto encontrado debe eliminar, borrar imágenes, invalidar caché y retornar DTO")]
    public async Task DeleteAsync_WhenProductFound_ShouldDeleteAndCleanup()
    {
        var product = CreateProduct();
        product.Images = ["img1.jpg", "img2.jpg"];

        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.DeleteByIdAsync(ProductId)).Returns(Task.CompletedTask);
        _storageMock.Setup(s => s.DeleteFileAsync(It.IsAny<string>()))
                    .ReturnsAsync(Result.Success<bool, ProductError>(true));

        var result = await _service.DeleteAsync(ProductId);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(ProductId));
        _productRepoMock.Verify(r => r.DeleteByIdAsync(ProductId), Times.Once);
        _storageMock.Verify(s => s.DeleteFileAsync("img1.jpg"), Times.Once);
        _storageMock.Verify(s => s.DeleteFileAsync("img2.jpg"), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k.Contains(ProductId))), Times.Once);
    }

    [Test]
    [Description("DeleteAsync: Sin imágenes no debe llamar a DeleteFileAsync")]
    public async Task DeleteAsync_WhenNoImages_ShouldNotCallDeleteFile()
    {
        var product = CreateProduct();
        product.Images = [];

        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.DeleteByIdAsync(ProductId)).Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(ProductId);

        Assert.That(result.IsSuccess, Is.True);
        _storageMock.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("DeleteAsync: Producto no encontrado debe retornar ProductNotFoundError")]
    public async Task DeleteAsync_WhenProductNotFound_ShouldReturnNotFoundError()
    {
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync((Product?)null);

        var result = await _service.DeleteAsync(ProductId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
    }

    [Test]
    [Description("DeleteAsync: Cuando el repositorio lanza excepción debe retornar error")]
    public async Task DeleteAsync_WhenRepositoryThrows_ShouldReturnError()
    {
        var product = CreateProduct();
        _productRepoMock.Setup(r => r.GetProductAsync(ProductId)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.DeleteByIdAsync(ProductId))
                        .ThrowsAsync(new Exception("Error de BD"));

        var result = await _service.DeleteAsync(ProductId);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.InstanceOf<ProductNotFoundError>());
        Assert.That(result.Error.Message, Does.Contain("Error de BD"));
    }

    [Test]
    [Description("GetStatsAsync: Debería añadir las stats de todos los subservicios")]
    public async Task GetStatsAsync_ShouldReturnAggregatedStats()
    {
        var productsByCategory = new Dictionary<string, int> { { "Figuras", 10 }, { "Ropa", 5 } };
        _productRepoMock
            .Setup(r => r.GetStatsAsync(It.IsAny<bool>()))
            .ReturnsAsync((TotalProducts: 15, OutOfStockCount: 2, ProductsByCategory: productsByCategory));

        _cartServiceMock
            .Setup(c => c.CalculateTotalEarningsAsync(null, true))
            .ReturnsAsync(999.99);

        _userServiceMock
            .Setup(u => u.GetTotalUsersCountAsync())
            .ReturnsAsync(42);

        _cartServiceMock
            .Setup(c => c.GetTotalSalesCountAsync())
            .ReturnsAsync(7);

        var result = await _service.GetStatsAsync();

        Assert.That(result.TotalProducts, Is.EqualTo(15));
        Assert.That(result.OutOfStockCount, Is.EqualTo(2));
        Assert.That(result.TotalUsers, Is.EqualTo(42));
        Assert.That(result.TotalSales, Is.EqualTo(7));
        Assert.That(result.TotalEarnings, Is.EqualTo(999.99));
        Assert.That(result.ProductsByCategory, Is.EqualTo(productsByCategory));
    }
}
