using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Users.Dto;

public record LoginDto
{
    /// <summary>
    /// Nombre de usuario o correo electrónico.
    /// Identifica al usuario en el sistema.
    /// </summary>
    /// <example>juanperez</example>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    public string UsernameOrEmail { get; init; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario.
    /// Verificada contra el hash almacenado.
    /// </summary>
    /// <example>Contraseña123!</example>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; init; } = string.Empty;
}