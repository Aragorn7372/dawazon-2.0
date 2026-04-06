using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dawazon2._0.RestControllers;

/// <summary>
/// Controlador API REST para gestión de usuarios desde el panel de Admin.
/// </summary>
/// <remarks>
/// Proporciona endpoints para listar, ver, actualizar y banning usuarios.
/// Solo accesible para usuarios con rol ADMIN.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>IUserService: Lógica de negocio de usuarios</item>
///     <item>ILogger: Logging</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Listado paginado de usuarios</item>
///     <item>Detalle de usuario por ID</item>
///     <item>Actualización de datos de usuario</item>
///     <item>Baneo de usuarios</item>
/// </list>
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.ADMIN)]
public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los usuarios con paginación.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Requiere rol ADMIN</item>
    ///     <item>Recibe filtros de paginación</item>
    /// </list>
    /// </remarks>
    /// <param name="filters">Filtros de paginación.</param>
    /// <response code="200">Lista de usuarios.</response>
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

    /// <summary>
    /// Obtiene un usuario específico por ID.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <response code="200">Usuario encontrado.</response>
    /// <response code="404">Usuario no encontrado.</response>
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

    /// <summary>
    /// Actualiza los datos de un usuario.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="userRequestDto">Nuevos datos del usuario.</param>
    /// <response code="200">Usuario actualizado.</response>
    /// <response code="404">Usuario no encontrado.</response>
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

    /// <summary>
    /// Banea (elimina lógicamente) un usuario.
    /// </summary>
    /// <param name="id">ID del usuario a banear.</param>
    /// <response code="204">Usuario baneado exitosamente.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BanUser(string id)
    {
        logger.LogInformation($"Endpoint llamado: DELETE api/users/{id}");
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