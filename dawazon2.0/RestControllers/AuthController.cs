using CSharpFunctionalExtensions;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Service.Auth;
using Microsoft.AspNetCore.Mvc;

namespace dawazon2._0.RestControllers;

/// <summary>
/// Controlador API REST para autenticación.
/// </summary>
/// <remarks>
/// Proporciona endpoints para login y registro de usuarios.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>ILogger: Logging</item>
///     <item>IAuthService: Servicio de autenticación</item>
/// </list>
/// 
/// <para><b>Endpoints:</b></para>
/// <list type="bullet">
///     <item>POST /api/auth/login - Inicio de sesión</item>
///     <item>POST /api/auth/register - Registro de nuevo usuario</item>
/// </list>
/// </remarks>
[ApiController]
[Route("api/[controller]/[action]")]
[Produces("application/json")]
public class AuthController(ILogger<AuthController> logger, IAuthService service) : ControllerBase
{
    /// <summary>
    /// Inicia sesión de un usuario.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Verifica credenciales</item>
    ///     <item>Retorna token JWT si es exitoso</item>
    /// </list>
    /// </remarks>
    /// <param name="dto">Credenciales (username/email + password).</param>
    /// <response code="200">Login exitoso con token.</response>
    /// <response code="401">Credenciales inválidas.</response>
    /// <response code="404">Usuario no encontrado.</response>
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

    /// <summary>
    /// Registra un nuevo usuario.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Crea nuevo usuario en el sistema</item>
    ///     <item>Asigna rol USER por defecto</item>
    /// </list>
    /// </remarks>
    /// <param name="dto">Datos de registro.</param>
    /// <response code="200">Registro exitoso.</response>
    /// <response code="409">Conflicto (usuario/email ya existe).</response>
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