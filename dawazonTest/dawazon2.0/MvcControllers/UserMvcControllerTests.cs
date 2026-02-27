using CSharpFunctionalExtensions;
using dawazon2._0.Models;
using dawazon2._0.MvcControllers;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using dawazonBackend.Users.Service.Favs;
using dawazonBackend.Cart.Models;
using dawazonBackend.Users.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace dawazonTest.dawazon2._0.MvcControllers;

[TestFixture]
public class UserMvcControllerTests
{
    private Mock<IUserService> _userServiceMock;
    private Mock<IFavService> _favServiceMock;
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<SignInManager<User>> _signInManagerMock;
    private UserMvcController _controller;

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [SetUp]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();
        _favServiceMock = new Mock<IFavService>();
        
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        _signInManagerMock = new Mock<SignInManager<User>>(_userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);

        _controller = new UserMvcController(_userServiceMock.Object, _favServiceMock.Object, _userManagerMock.Object, _signInManagerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext() { User = user };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Test]
    public async Task Profile_WhenUserExists_ShouldReturnViewWithModel()
    {
        var mockUser = new User
        {
            Id = 1,
            Name = "John",
            Email = "john@example.com",
            PhoneNumber = "123456789",
            Client = new Client { Address = new Address { Street = "Broadway", City = "NY", PostalCode = 10001, Country = "USA", Province = "NY" } }
        };
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(mockUser);

        var result = await _controller.Profile() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<UserProfileViewModel>());
        var model = (UserProfileViewModel)result.Model;
        Assert.That(model.Name, Is.EqualTo("John"));
        Assert.That(model.Email, Is.EqualTo("john@example.com"));
        Assert.That(model.Street, Is.EqualTo("Broadway"));
    }

    [Test]
    public async Task Profile_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((User)null);

        var result = await _controller.Profile();

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task EditProfile_Get_WhenUserExists_ShouldReturnViewWithModel()
    {
        var mockUser = new User
        {
            Id = 1,
            Name = "John",
            Email = "john@example.com",
            PhoneNumber = "123456789",
            Client = new Client { Address = new Address { Street = "Broadway" } }
        };
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(mockUser);

        var result = await _controller.EditProfile() as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<UserEditViewModel>());
        var model = (UserEditViewModel)result.Model;
        Assert.That(model.Nombre, Is.EqualTo("John"));
        Assert.That(model.Email, Is.EqualTo("john@example.com"));
        Assert.That(model.Calle, Is.EqualTo("Broadway"));
    }

    [Test]
    public async Task EditProfile_Get_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((User)null);

        var result = await _controller.EditProfile();

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task EditProfile_Post_WithInvalidModel_ShouldReturnView()
    {
        _controller.ModelState.AddModelError("Error", "Invalid data");
        var vm = new UserEditViewModel();

        var result = await _controller.EditProfile(vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.EqualTo(vm));
    }

    [Test]
    public async Task EditProfile_Post_WhenUpdateFails_ShouldReturnViewWithError()
    {
        var vm = new UserEditViewModel();
        _userServiceMock.Setup(s => s.UpdateByIdAsync(1, It.IsAny<UserRequestDto>(), null))
            .ReturnsAsync(Result.Failure<UserDto, UserError>(new UserNotFoundError("Update Failed")));

        var result = await _controller.EditProfile(vm) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
    }

    [Test]
    public async Task EditProfile_Post_WhenUpdateSucceeds_ShouldRedirectToProfile()
    {
        var vm = new UserEditViewModel { Nombre = "New Name" };
        var mockResponse = new UserDto { Id = 1, Nombre = "New Name" };
        _userServiceMock.Setup(s => s.UpdateByIdAsync(1, It.IsAny<UserRequestDto>(), null))
            .ReturnsAsync(Result.Success<UserDto, UserError>(mockResponse));

        var result = await _controller.EditProfile(vm) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Profile"));
        Assert.That(_controller.TempData["Success"], Is.EqualTo("Perfil actualizado correctamente."));
    }

    [Test]
    public async Task DeleteAccount_ShouldBanSignOutAndRedirect()
    {
        _userServiceMock.Setup(s => s.BanUserById("1")).Returns(Task.CompletedTask);
        _signInManagerMock.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

        var result = await _controller.DeleteAccount() as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        Assert.That(result.ControllerName, Is.EqualTo("ProductsMvc"));
        Assert.That(_controller.TempData["Info"], Is.EqualTo("Tu cuenta ha sido desactivada."));
    }

    [Test]
    public async Task Favs_WhenSuccessful_ShouldReturnViewWithModel()
    {
        var mockPage = new PageResponseDto<ProductResponseDto>(new List<ProductResponseDto>(), 1, 10, 10, 0, 10, "id", "asc");
        _favServiceMock.Setup(s => s.GetFavs(1, It.IsAny<FilterDto>()))
            .ReturnsAsync(Result.Success<PageResponseDto<ProductResponseDto>, DomainError>(mockPage));

        var result = await _controller.Favs(0, 12) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.TypeOf<UserFavsViewModel>());
    }

    [Test]
    public async Task Favs_WhenFails_ShouldReturn500()
    {
        _favServiceMock.Setup(s => s.GetFavs(1, It.IsAny<FilterDto>()))
            .ReturnsAsync(Result.Failure<PageResponseDto<ProductResponseDto>, DomainError>(new UserNotFoundError("Error")));

        var result = await _controller.Favs(0, 12) as StatusCodeResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task AddFav_ShouldCallServiceAndRedirect()
    {
        _favServiceMock.Setup(s => s.AddFav("PROD-1", 1)).ReturnsAsync(Result.Success<bool, DomainError>(true));

        var result = await _controller.AddFav("PROD-1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(result.ControllerName, Is.EqualTo("ProductsMvc"));
        Assert.That(result.RouteValues["id"], Is.EqualTo("PROD-1"));
    }

    [Test]
    public async Task RemoveFav_ShouldCallServiceAndRedirect()
    {
        _favServiceMock.Setup(s => s.RemoveFav("PROD-1", 1)).ReturnsAsync(Result.Success<bool, DomainError>(true));

        var result = await _controller.RemoveFav("PROD-1") as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Detail"));
        Assert.That(result.ControllerName, Is.EqualTo("ProductsMvc"));
        Assert.That(result.RouteValues["id"], Is.EqualTo("PROD-1"));
    }
}