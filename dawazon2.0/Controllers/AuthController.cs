using CSharpFunctionalExtensions;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Service.Auth;
using Microsoft.AspNetCore.Mvc;

namespace dawazon2._0.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Produces("application/json")]
public class AuthController(ILogger<AuthController> logger, IAuthService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        logger.LogInformation($"Intento de inicio de sesión de: {dto.UsernameOrEmail}");
        return await service.SignInAsync(dto).Match(
            onSuccess: IActionResult (result) => Ok(result),  
            onFailure: error => error switch
            {
                UserNotFoundError => NotFound(new { message = error.Message }),
                UserConflictError => Conflict(new { message = error.Message }),
                UnauthorizedError => Unauthorized(new { message = error.Message }),
                _ => BadRequest(new { message = error.Message })
            }
        );
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        logger.LogInformation("registro");
        return await service.SignUpAsync(dto).Match(
            onSuccess: IActionResult (result) => Ok(result),
            onFailure: error => error switch
            {
                UserConflictError => Conflict(new { message = error.Message }),
                UnauthorizedError => Unauthorized(new {message=error.Message}),
                _ => BadRequest(new {message=error.Message})
            });
    }
}