using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using dawazon2._0.RestControllers;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace dawazonTest.dawazon2._0.RestController;

[TestFixture]
public class UsersControllerTests
{
    private Mock<IUserService> _userServiceMock;
    private Mock<ILogger<UsersController>> _loggerMock;
    private UsersController _controller;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_userServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk()
    {
        var users = new List<UserDto>();
        var pageResponse = new PageResponseDto<UserDto>(users, 0, 0, 10, 0, 0, "id", "asc");
        var filter = new FilterDto(null, null, 0, 10, "id", "asc");

        _userServiceMock.Setup(s => s.GetAllAsync(It.IsAny<FilterDto>()))
            .ReturnsAsync(pageResponse);

        var result = await _controller.GetAll(filter);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(pageResponse));
    }

    [Test]
    public async Task GetById_ReturnsOk_WhenUserFound()
    {
        var userDto = new UserDto { Id = 1, Nombre = "test" };

        _userServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Success<UserDto, UserError>(userDto));

        var result = await _controller.GetById("1");

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(userDto));
    }

    [Test]
    public async Task GetById_ReturnsNotFound_WhenUserNotFound()
    {
        _userServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Failure<UserDto, UserError>(new UserNotFoundError("Not found")));

        var result = await _controller.GetById("1");

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task GetById_ReturnsBadRequest_WhenOtherError()
    {
        _userServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Failure<UserDto, UserError>(new UserError("Generic error")));

        var result = await _controller.GetById("1");

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
    }

    [Test]
    public async Task UpdateById_ReturnsOk_WhenUpdateSucceeds()
    {
        var requestDto = new UserRequestDto { Nombre = "new_username" };
        var responseDto = new UserDto { Id = 1, Nombre = "new_username" };

        _userServiceMock.Setup(s => s.UpdateByIdAsync(1, requestDto, It.IsAny<IFormFile?>()))
            .ReturnsAsync(Result.Success<UserDto, UserError>(responseDto));

        var result = await _controller.UpdateById(1, requestDto);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(responseDto));
    }

    [Test]
    public async Task UpdateById_ReturnsNotFound_WhenUserNotFound()
    {
        var requestDto = new UserRequestDto { Nombre = "new_username" };

        _userServiceMock.Setup(s => s.UpdateByIdAsync(1, requestDto, It.IsAny<IFormFile?>()))
            .ReturnsAsync(Result.Failure<UserDto, UserError>(new UserNotFoundError("Not found")));

        var result = await _controller.UpdateById(1, requestDto);

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task UpdateById_ReturnsBadRequest_WhenOtherError()
    {
        var requestDto = new UserRequestDto { Nombre = "new_username" };

        _userServiceMock.Setup(s => s.UpdateByIdAsync(1, requestDto, It.IsAny<IFormFile?>()))
            .ReturnsAsync(Result.Failure<UserDto, UserError>(new UserError("Generic error")));

        var result = await _controller.UpdateById(1, requestDto);

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
    }

    [Test]
    public async Task BanUser_ReturnsNoContent_WhenUserFoundAndBanned()
    {
        var responseDto = new UserDto { Id = 1 };

        _userServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Success<UserDto, UserError>(responseDto));

        _userServiceMock.Setup(s => s.BanUserById("1"))
            .Returns(Task.CompletedTask);

        var result = await _controller.BanUser("1");

        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        _userServiceMock.Verify(s => s.BanUserById("1"), Times.Once);
    }

    [Test]
    public async Task BanUser_ReturnsNotFound_WhenUserNotFound()
    {
        _userServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Failure<UserDto, UserError>(new UserNotFoundError("Not found")));

        var result = await _controller.BanUser("1");

        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        _userServiceMock.Verify(s => s.BanUserById("1"), Times.Never);
    }

    [Test]
    public async Task BanUser_ReturnsBadRequest_WhenGetByIdThrowsOtherError()
    {
        _userServiceMock.Setup(s => s.GetByIdAsync("1"))
            .ReturnsAsync(Result.Failure<UserDto, UserError>(new UserError("Generic error")));

        var result = await _controller.BanUser("1");

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        _userServiceMock.Verify(s => s.BanUserById("1"), Times.Never);
    }
}
