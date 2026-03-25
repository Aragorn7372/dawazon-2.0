namespace dawazonBackend.Users.Dto;

/// <summary>
/// DTO de respuesta de autenticación con JWT.
/// </summary>
/// <remarks>
/// Devuelto tras login o registro exitoso.
/// El token JWT debe enviarse en el header Authorization:
/// Authorization: Bearer &lt;token&gt;
/// </remarks>
public record AuthResponseDto(
    /// <summary>
    /// Token JWT para autenticación en requests.
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    string Token
);
