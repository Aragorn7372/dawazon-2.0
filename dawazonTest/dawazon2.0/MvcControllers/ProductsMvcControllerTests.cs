using CSharpFunctionalExtensions;
using dawazon2._0.Models;
using dawazon2._0.MvcControllers;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Errors;
using dawazonBackend.Cart.Service;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Products.Service;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace dawazonTest.dawazon2._0.MvcControllers;

[TestFixture]
public class ProductsMvcControllerTests
{
    private Mock<IProductService> _productServiceMock;
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<ICartService> _cartServiceMock;
    private ProductsMvcController _controller;

    [SetUp]
    public void Setup()
    {
        _productServiceMock = new Mock<IProductService>();
        
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        
        _cartServiceMock = new Mock<ICartService>();

        _controller = new ProductsMvcController(_productServiceMock.Object, _userManagerMock.Object, _cartServiceMock.Object);

        SetupControllerUser(UserRoles.MANAGER, "1");
    }

    private void SetupControllerUser(string role, string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        }, "mock"));

        var httpContext = new DefaultHttpContext() { User = user };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public async Task Index_ShouldReturnViewWithModel()
    {
        var mockProducts = new List<ProductResponseDto>
        {
            new ProductResponseDto("1", "Product 1", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>())
        };
        var mockPage = new PageResponseDto<ProductResponseDto>(mockProducts, 1, 10, 10, 0, 10, "id", "asc");

        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), It.IsAny<long?>()))
            .ReturnsAsync(mockPage);

        var result = await _controller.Index(null, null) as ViewResult;

        Assert.That(result, Is.Not.Null);
        dynamic model = result.Model;
        Assert.That(model, Is.Not.Null);
        Assert.That(_controller.ViewBag.MisProductos, Is.False);
    }

    [Test]
    public async Task Index_WithMisProductosAndManagerRole_ShouldFilterByCreatorId()
    {
        var mockPage = new PageResponseDto<ProductResponseDto>(new List<ProductResponseDto>(), 1, 10, 10, 0, 10, "id", "asc");

        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), 1)) 
            .ReturnsAsync(mockPage);
            
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), null)) 
            .ReturnsAsync(mockPage);

        var result = await _controller.Index(null, null, misProductos: true) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ViewBag.MisProductos, Is.True);
        _productServiceMock.Verify(s => s.GetAllAsync(It.IsAny<FilterDto>(), 1), Times.Once);
    }

    [Test]
    public async Task Detail_WhenProductExists_AndUserIsRoleUser_ShouldReturnViewWithFavAndCartSet()
    {
        SetupControllerUser(UserRoles.USER, "1");
        var mockProduct = new ProductResponseDto("1", "Product 1", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>());
        _productServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockProduct));
        
        var mockUser = new User { Id = 1, ProductsFavs = new List<string> { "1" } };
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(mockUser);
        
        var mockClient = new ClientDto { Name = "Test", Email = "test@test.com", Phone = "123" };
        var mockCart = new CartResponseDto("cart1", 1, false, mockClient, new List<SaleLineDto> { new SaleLineDto { ProductId = "1", Quantity = 1 } }, 1, 10);
        _cartServiceMock.Setup(s => s.GetCartByUserIdAsync(1))
            .ReturnsAsync(Result.Success<CartResponseDto, DomainError>(mockCart));

        var result = await _controller.Detail("1") as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ViewBag.IsFav, Is.True);
        Assert.That(_controller.ViewBag.IsInCart, Is.True);
        Assert.That(_controller.ViewBag.CartId, Is.EqualTo("cart1"));
    }

    [Test]
    public async Task Detail_WhenProductNotFound_ShouldReturnNotFound()
    {
        _productServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductNotFoundError("Not found")));

        var result = await _controller.Detail("1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Detail_WhenOtherError_ShouldReturn500()
    {
        _productServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductStorageError("DB error")));

        var result = await _controller.Detail("1") as StatusCodeResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task Create_Get_ShouldReturnFormView()
    {
        var mockPage = new PageResponseDto<ProductResponseDto>(new List<ProductResponseDto>(), 1, 10, 10, 0, 10, "id", "asc");
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), null))
            .ReturnsAsync(mockPage);

        var result = await _controller.Create() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Form"));
        Assert.That(result.Model, Is.TypeOf<ProductFormViewModel>());
    }

    [Test]
    public async Task Create_Post_WhenModelStateIsInvalid_ShouldReturnView()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var vm = new ProductFormViewModel { Price = 10, Stock = 5, Category = "Cat1" };
        var mockPage = new PageResponseDto<ProductResponseDto>(new List<ProductResponseDto>(), 1, 10, 10, 0, 10, "id", "asc");
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), null)).ReturnsAsync(mockPage);

        var result = await _controller.Create(vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Form"));
    }

    [Test]
    public async Task Create_Post_WhenCreateFails_ShouldReturnViewWithError()
    {
        var vm = new ProductFormViewModel { Name = "New Product", Price = 10, Stock = 5, Category = "Cat1" };
        var mockPage = new PageResponseDto<ProductResponseDto>(new List<ProductResponseDto>(), 1, 10, 10, 0, 10, "id", "asc");
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), null)).ReturnsAsync(mockPage);
        
        _productServiceMock.Setup(s => s.CreateAsync(It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductStorageError("Failed")));

        var result = await _controller.Create(vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Form"));
        Assert.That(_controller.ModelState.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task Create_Post_WhenValidAndHasImages_ShouldCreateAndUpdateImages()
    {
        var mockPage = new PageResponseDto<ProductResponseDto>(new List<ProductResponseDto>(), 1, 10, 10, 0, 10, "id", "asc");
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), null))
            .ReturnsAsync(mockPage);

        var fileMock = new Mock<IFormFile>();
        var vm = new ProductFormViewModel { Name = "New Product", Price = 10, Stock = 5, Category = "Cat1", Images = new List<IFormFile> { fileMock.Object } };
        var mockResponse = new ProductResponseDto("1", "New Product", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>());
        
        _productServiceMock.Setup(s => s.CreateAsync(It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockResponse));

        _productServiceMock.Setup(s => s.UpdateImageAsync("1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockResponse));

        var result = await _controller.Create(vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _productServiceMock.Verify(s => s.UpdateImageAsync("1", It.IsAny<List<IFormFile>>()), Times.Once);
    }

    [Test]
    public async Task Edit_Get_WhenProductExistsAndUserIsCreator_ShouldReturnFormView()
    {
        var mockProduct = new ProductResponseDto("1", "Product 1", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>());
        _productServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockProduct));
            
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1"))
            .ReturnsAsync(Result.Success<long, ProductError>(1));

        var mockPage = new PageResponseDto<ProductResponseDto>(new List<ProductResponseDto>(), 1, 10, 10, 0, 10, "id", "asc");
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), null))
            .ReturnsAsync(mockPage);

        var result = await _controller.Edit("1") as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Form"));
        Assert.That(result.Model, Is.TypeOf<ProductFormViewModel>());
        var model = result.Model as ProductFormViewModel;
        Assert.That(model.Name, Is.EqualTo("Product 1"));
    }

    [Test]
    public async Task Edit_Get_WhenProductNotFound_ShouldReturnNotFound()
    {
        _productServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductNotFoundError("Not found")));

        var result = await _controller.Edit("1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Get_WhenGenericError_ShouldReturn500()
    {
        _productServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductStorageError("DB error")));

        var result = await _controller.Edit("1") as StatusCodeResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task Edit_Get_WhenCreatorNotFound_ShouldReturnNotFound()
    {
        var mockProduct = new ProductResponseDto("1", "Product 1", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>());
        _productServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockProduct));
            
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1"))
            .ReturnsAsync(Result.Failure<long, ProductError>(new ProductNotFoundError("Not found"))); 

        var result = await _controller.Edit("1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Get_WhenUserIsNotCreator_ShouldReturnForbid()
    {
        var mockProduct = new ProductResponseDto("1", "Product 1", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>());
        _productServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockProduct));
            
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1"))
            .ReturnsAsync(Result.Success<long, ProductError>(2)); 

        var result = await _controller.Edit("1");

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    [Test]
    public async Task Edit_Post_WhenModelStateIsInvalid_ShouldReturnView()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var vm = new ProductFormViewModel { Name = "" };
        
        var mockPage = new PageResponseDto<ProductResponseDto>(new List<ProductResponseDto>(), 1, 10, 10, 0, 10, "id", "asc");
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), null)).ReturnsAsync(mockPage);

        var result = await _controller.Edit("1", vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Form"));
    }

    [Test]
    public async Task Edit_Post_WhenCreatorNotFound_ShouldReturnNotFound()
    {
        var vm = new ProductFormViewModel { Name = "Updated Product" };
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1"))
            .ReturnsAsync(Result.Failure<long, ProductError>(new ProductNotFoundError("Not found")));

        var result = await _controller.Edit("1", vm);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Post_WhenUserIsNotCreator_ShouldReturnForbid()
    {
        var vm = new ProductFormViewModel { Name = "Updated Product" };
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1"))
            .ReturnsAsync(Result.Success<long, ProductError>(2)); // Creator 2 != User 1

        var result = await _controller.Edit("1", vm);

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    [Test]
    public async Task Edit_Post_WhenUpdateFails_ShouldReturnViewWithError()
    {
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1")).ReturnsAsync(Result.Success<long, ProductError>(1));

        var vm = new ProductFormViewModel { Name = "Updated Product" };
        _productServiceMock.Setup(s => s.UpdateAsync("1", It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductStorageError("Update failed")));

        var mockPage = new PageResponseDto<ProductResponseDto>(new List<ProductResponseDto>(), 1, 10, 10, 0, 10, "id", "asc");
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>(), null)).ReturnsAsync(mockPage);

        var result = await _controller.Edit("1", vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Form"));
        Assert.That(_controller.ModelState.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task Edit_Post_WhenValidAndUserIsCreatorAndHasImages_ShouldUpdateImagesAndRedirectToDetail()
    {
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1"))
            .ReturnsAsync(Result.Success<long, ProductError>(1));

        var fileMock = new Mock<IFormFile>();
        var vm = new ProductFormViewModel { Name = "Updated Product", Images = new List<IFormFile> { fileMock.Object } };
        var mockResponse = new ProductResponseDto("1", "Updated Product", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>());
        
        _productServiceMock.Setup(s => s.UpdateImageAsync("1", It.IsAny<List<IFormFile>>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockResponse));

        _productServiceMock.Setup(s => s.UpdateAsync("1", It.IsAny<ProductRequestDto>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockResponse));

        var result = await _controller.Edit("1", vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(result.RouteValues["id"], Is.EqualTo("1"));
        _productServiceMock.Verify(s => s.UpdateImageAsync("1", It.IsAny<List<IFormFile>>()), Times.Once);
    }

    [Test]
    public async Task Delete_WhenCreator_ShouldReturnRedirect()
    {
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1"))
            .ReturnsAsync(Result.Success<long, ProductError>(1));
            
        var mockResponse = new ProductResponseDto("1", "Product 1", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>());
        _productServiceMock.Setup(s => s.DeleteAsync("1"))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockResponse));

        var result = await _controller.Delete("1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public async Task Delete_WhenProductNotFound_ShouldReturnNotFound()
    {
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1")).ReturnsAsync(Result.Failure<long, ProductError>(new ProductNotFoundError("Not found")));

        var result = await _controller.Delete("1");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_WhenUserIsNotCreator_ShouldReturnForbid()
    {
        _productServiceMock.Setup(s => s.GetUserProductIdAsync("1")).ReturnsAsync(Result.Success<long, ProductError>(2));

        var result = await _controller.Delete("1");

        Assert.That(result, Is.TypeOf<ForbidResult>());
    }

    [Test]
    public async Task Delete_WhenAdmin_ShouldBypassCreatorCheckAndRedirect()
    {
        SetupControllerUser(UserRoles.ADMIN, "2"); 
        
        var mockResponse = new ProductResponseDto("1", "Product 1", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>());
        _productServiceMock.Setup(s => s.DeleteAsync("1"))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(mockResponse));

        var result = await _controller.Delete("1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _productServiceMock.Verify(s => s.GetUserProductIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Delete_WhenDeleteFailsWithGenericError_ShouldReturn500()
    {
        SetupControllerUser(UserRoles.ADMIN, "2"); 
        
        _productServiceMock.Setup(s => s.DeleteAsync("1"))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductStorageError("DB Error")));

        var result = await _controller.Delete("1") as StatusCodeResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task AddComment_WhenValid_ShouldRedirectToDetail()
    {
        SetupControllerUser(UserRoles.USER, "1");
        var vm = new AddCommentViewModel { CommentText = "Great product", Recommended = true };
        
        _productServiceMock.Setup(s => s.AddCommentAsync("1", It.IsAny<dawazonBackend.Products.Models.Comment>()))
            .ReturnsAsync(Result.Success<ProductResponseDto, ProductError>(new ProductResponseDto("1", "Product 1", 10, 5, "Cat1", "Brand", new List<CommentDto>(), new List<string>())));

        var result = await _controller.AddComment("1", vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(result.RouteValues["id"], Is.EqualTo("1"));
        Assert.That(_controller.TempData["Success"], Is.Not.Null);
    }

    [Test]
    public async Task AddComment_WhenInvalidModelState_ShouldRedirectToDetailWithError()
    {
        SetupControllerUser(UserRoles.USER, "1");
        _controller.ModelState.AddModelError("CommentText", "Required");
        var vm = new AddCommentViewModel { CommentText = "" }; 

        var result = await _controller.AddComment("1", vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }

    [Test]
    public async Task AddComment_WhenAddCommentFails_ShouldRedirectToDetailWithError()
    {
        SetupControllerUser(UserRoles.USER, "1");
        var vm = new AddCommentViewModel { CommentText = "Great product", Recommended = true };
        
        _productServiceMock.Setup(s => s.AddCommentAsync("1", It.IsAny<dawazonBackend.Products.Models.Comment>()))
            .ReturnsAsync(Result.Failure<ProductResponseDto, ProductError>(new ProductStorageError("Failed")));

        var result = await _controller.AddComment("1", vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }

    [Test]
    public async Task AddComment_WhenUserIdParseFails_ShouldRedirectToDetailWithError()
    {
        SetupControllerUser(UserRoles.USER, "invalid-id");
        var vm = new AddCommentViewModel { CommentText = "Great product", Recommended = true };

        var result = await _controller.AddComment("1", vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(_controller.TempData["Error"], Is.Not.Null);
    }
}
