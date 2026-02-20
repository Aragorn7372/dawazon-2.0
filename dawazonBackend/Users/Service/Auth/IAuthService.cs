using CSharpFunctionalExtensions;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;

namespace dawazonBackend.Users.Service.Auth;

/// <summary>
/// Contrato del servicio de autenticación.
/// </summary>
public interface IAuthService
{
    /// <summary>Registra un nuevo usuario.</summary>
    /// <param name="dto">Datos de registro.</param>
    /// <returns>Resultado con respuesta de autenticación.</returns>
    Task<Result<AuthResponseDto, UserError>> SignUpAsync(RegisterDto dto);

    /// <summary>Inicia sesión con credenciales.</summary>
    /// <param name="dto">Credenciales de acceso.</param>
    /// <returns>Resultado con respuesta de autenticación.</returns>
    Task<Result<AuthResponseDto, UserError>> SignInAsync(LoginDto dto);
}