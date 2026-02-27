using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using dawazon2._0.RestControllers;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Products.Service;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.RestController;

[TestFixture]
public class ProductsControllerTests
{
    private Mock<IProductService> _productServiceMock;
    private ProductsController _controller;

    [SetUp]
    public void SetUp()
    {
        _productServiceMock = new Mock<IProductService>();
        _controller = new ProductsController(_productServiceMock.Object);
    }

    private void SetUpUserContext(string userId = "1", string role = UserRoles.MANAGER)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Test]
    public async Task GetAsync_All_ReturnsOk()
    {
        var expectedProducts = new List<ProductResponseDto>();
        var pageResponse = new PageResponseDto<ProductResponseDto>(expectedProducts, 0, 0, 10, 0, 0, "id", "asc");
        _productServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<FilterDto>()))
            .ReturnsAsync(pageResponse);

        var result = await _controller.GetAsync("nombre", "categoria", "id", 0, 10, "asc");

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(pageResponse));
    }

    [Test]
    public async Task GetAsync_ById_ReturnsOk_WhenFound()
    {
        var dto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string>());
        _productServiceMock
            .Setup(s => s.GetByIdAsync("prod1"))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(dto));

        var result = await _controller.GetAsync("prod1");

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetAsync_ById_ReturnsNotFound_WhenNotFound()
    {
        _productServiceMock
            .Setup(s => s.GetByIdAsync("prod1"))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductNotFoundError("Not found")));

        var result = await _controller.GetAsync("prod1");

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task GetAsync_ById_ReturnsServerError_OnOtherErrors()
    {
        _productServiceMock
            .Setup(s => s.GetByIdAsync("prod1"))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductError("Server error")));

        var result = await _controller.GetAsync("prod1");

        var statusResult = result as ObjectResult;
        Assert.That(statusResult, Is.Not.Null);
        Assert.That(statusResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task PostAsync_ReturnsCreated_WhenValid()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "New Product", 10.0, "Cat", "", null, 10, 1);
        var responseDto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string>());

        _productServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));
            
        _productServiceMock
            .Setup(s => s.UpdateImageAsync("prod1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));

        var result = await _controller.PostAsync(requestDto, new List<IFormFile>());

        var createdResult = result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.Value, Is.EqualTo(responseDto));
    }

    [Test]
    public async Task PostAsync_ReturnsConflict_WhenCreateFails()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "New Product", 10.0, "Cat", "", null, 10, 1);

        _productServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductError("Conflict")));

        var result = await _controller.PostAsync(requestDto, new List<IFormFile>());

        var conflictResult = result as ConflictObjectResult;
        Assert.That(conflictResult, Is.Not.Null);
    }

    [Test]
    public async Task PostAsync_ReturnsNotFound_WhenUpdateImageFailsWithProductNotFound()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "New Product", 10.0, "Cat", "", null, 10, 1);
        var responseDto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string>());

        _productServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));
            
        _productServiceMock
            .Setup(s => s.UpdateImageAsync("prod1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductNotFoundError("Not found")));

        var result = await _controller.PostAsync(requestDto, new List<IFormFile>());

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task PostAsync_ReturnsBadRequest_WhenUpdateImageFails()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "New Product", 10.0, "Cat", "", null, 10, 1);
        var responseDto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string>());

        _productServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));
            
        _productServiceMock
            .Setup(s => s.UpdateImageAsync("prod1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductError("Invalid image")));

        var result = await _controller.PostAsync(requestDto, new List<IFormFile>());

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
    }

    [Test]
    public async Task PutAsync_ReturnsOk_WhenValid_AndManagerIsCreator()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "Updated Product", 10.0, "Cat", "", null, 10, 123);
        var responseDto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string> { "img.png" });

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Success<long, ProductError>(123));

        _productServiceMock.Setup(s => s.UpdateImageAsync("prod1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));

        _productServiceMock.Setup(s => s.UpdateAsync("prod1", It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));

        var result = await _controller.PutAsync("prod1", requestDto, new List<IFormFile>());

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(responseDto));
    }

    [Test]
    public async Task PutAsync_ReturnsNotFound_WhenCreatorIdFetchFails()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "Updated Product", 10.0, "Cat", "", null, 10, 123);

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Failure<long, ProductError>(new ProductError("Not found")));

        var result = await _controller.PutAsync("prod1", requestDto, new List<IFormFile>());

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task PutAsync_ReturnsForbid_WhenManagerIsNotCreator()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "Updated Product", 10.0, "Cat", "", null, 10, 123);

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Success<long, ProductError>(999)); // Different user

        var result = await _controller.PutAsync("prod1", requestDto, new List<IFormFile>());

        var forbidResult = result as ForbidResult;
        Assert.That(forbidResult, Is.Not.Null);
    }

    [Test]
    public async Task PutAsync_ReturnsBadRequest_WhenUpdateImageFails()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "Updated Product", 10.0, "Cat", "", null, 10, 123);

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Success<long, ProductError>(123));

        _productServiceMock.Setup(s => s.UpdateImageAsync("prod1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductError("Bad image")));

        var result = await _controller.PutAsync("prod1", requestDto, new List<IFormFile>());

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
    }

    [Test]
    public async Task PutAsync_ReturnsNotFound_WhenUpdateImageFailsWithProductNotFound()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "Updated Product", 10.0, "Cat", "", null, 10, 123);

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Success<long, ProductError>(123));

        _productServiceMock.Setup(s => s.UpdateImageAsync("prod1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductNotFoundError("Not found")));

        var result = await _controller.PutAsync("prod1", requestDto, new List<IFormFile>());

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task PutAsync_ReturnsBadRequest_WhenUpdateThrowsValidationError()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "Updated Product", 10.0, "Cat", "", null, 10, 123);
        var responseDto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string>());

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Success<long, ProductError>(123));

        _productServiceMock.Setup(s => s.UpdateImageAsync("prod1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));

        _productServiceMock.Setup(s => s.UpdateAsync("prod1", It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductValidationError("Validation message")));

        var result = await _controller.PutAsync("prod1", requestDto, new List<IFormFile>());

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
    }
    
    [Test]
    public async Task PutAsync_ReturnsNotFound_WhenUpdateThrowsNotFoundError()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "Updated Product", 10.0, "Cat", "", null, 10, 123);
        var responseDto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string>());

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Success<long, ProductError>(123));

        _productServiceMock.Setup(s => s.UpdateImageAsync("prod1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));

        _productServiceMock.Setup(s => s.UpdateAsync("prod1", It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductNotFoundError("Not found message")));

        var result = await _controller.PutAsync("prod1", requestDto, new List<IFormFile>());

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }
    
    [Test]
    public async Task PutAsync_ReturnsServerError_WhenUpdateThrowsOtherError()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var requestDto = new ProductRequestDto(null, "Updated Product", 10.0, "Cat", "", null, 10, 123);
        var responseDto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string>());

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Success<long, ProductError>(123));

        _productServiceMock.Setup(s => s.UpdateImageAsync("prod1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));

        _productServiceMock.Setup(s => s.UpdateAsync("prod1", It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductError("Other error")));

        var result = await _controller.PutAsync("prod1", requestDto, new List<IFormFile>());

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task DeleteAsync_ReturnsOk_WhenValid_AndManagerIsCreator()
    {
        SetUpUserContext("123", UserRoles.MANAGER);
        var responseDto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string>());

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Success<long, ProductError>(123));

        _productServiceMock.Setup(s => s.DeleteAsync("prod1"))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));

        var result = await _controller.DeleteAsync("prod1");

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(responseDto));
    }

    [Test]
    public async Task DeleteAsync_ReturnsNotFound_WhenCreatorIdFetchFailsForManager()
    {
        SetUpUserContext("123", UserRoles.MANAGER);

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Failure<long, ProductError>(new ProductError("Not found")));

        var result = await _controller.DeleteAsync("prod1");

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_ReturnsForbid_WhenManagerIsNotCreator()
    {
        SetUpUserContext("123", UserRoles.MANAGER);

        _productServiceMock.Setup(s => s.GetUserProductIdAsync("prod1"))
            .ReturnsAsync(Result.Success<long, ProductError>(999));

        var result = await _controller.DeleteAsync("prod1");

        var forbidResult = result as ForbidResult;
        Assert.That(forbidResult, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_ReturnsOk_ForAdmin_BypassingCreatorCheck()
    {
        SetUpUserContext("123", UserRoles.ADMIN);
        var responseDto = new ProductResponseDto("prod1", "Name", 10.0, 10, "Category", "Desc", new List<CommentDto>(), new List<string>());

        _productServiceMock.Setup(s => s.DeleteAsync("prod1"))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(responseDto));

        var result = await _controller.DeleteAsync("prod1");

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        _productServiceMock.Verify(s => s.GetUserProductIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_ReturnsNotFound_WhenDeleteThrowsNotFoundError()
    {
        SetUpUserContext("123", UserRoles.ADMIN); 

        _productServiceMock.Setup(s => s.DeleteAsync("prod1"))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductNotFoundError("Not found")));

        var result = await _controller.DeleteAsync("prod1");

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }
    
    [Test]
    public async Task DeleteAsync_ReturnsServerError_WhenDeleteThrowsOtherError()
    {
        SetUpUserContext("123", UserRoles.ADMIN); 

        _productServiceMock.Setup(s => s.DeleteAsync("prod1"))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductError("Other")));

        var result = await _controller.DeleteAsync("prod1");

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }
}
