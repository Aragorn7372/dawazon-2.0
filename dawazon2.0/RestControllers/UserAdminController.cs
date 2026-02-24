using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dawazon2._0.RestControllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.ADMIN)]
public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] FilterDto filters)
    {
        logger.LogInformation("Endpoint llamado: GET api/users");
        var result = await userService.GetAllAsync(filters);
        
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(string id)
    {
        logger.LogInformation($"Endpoint llamado: GET api/users/{id}");
        var result = await userService.GetByIdAsync(id);
        
        return result.Match(
            onSuccess: IActionResult(u) => Ok(u),
            onFailure: error => error switch
            {
                UserNotFoundError => NotFound(new { message = error.Message }),
                _ => BadRequest(new { message =  error.Message})
            }
        );
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateById(long id, [FromBody] UserRequestDto userRequestDto)
    {
        logger.LogInformation($"Endpoint llamado: PUT api/users/{id}");
        var result = await userService.UpdateByIdAsync(id, userRequestDto, null);
        
        return result.Match(
            onSuccess: IActionResult(u) => Ok(u),
            onFailure: error => error switch
            {
                UserNotFoundError => NotFound(new { message = error.Message }),
                _ => BadRequest(new { message =  error.Message})
            }
        );
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BanUser(string id)
    {
        logger.LogInformation($"Endpoint called: DELETE api/users/{id}");
        var found = await userService.GetByIdAsync(id);
        if (found.IsFailure)
            return found.Error switch
            {
                UserNotFoundError => NotFound(new { message = found.Error.Message }),
                _ => BadRequest(new { message = found.Error.Message })
            };
        await userService.BanUserById(id);
        return NoContent();
    }
}