using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Users.Dto;

public record RegisterDto
{
    /// <summary>
    /// Nombre de usuario único.
    /// Identificador público del usuario en el sistema.
    /// </summary>
    /// <remarks>
    /// Restricciones:
    /// - Solo letras, números y guiones bajos
    /// - Sin espacios ni caracteres especiales
    /// - Debe ser único en el sistema
    /// </remarks>
    /// <example>juanperez</example>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [MinLength(3, ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres")]
    [MaxLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Solo se permiten letras, números y guiones bajos")]
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario.
    /// Utilizado para notificaciones y recuperación de contraseña.
    /// </summary>
    /// <example>juan@example.com</example>
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Debe ser un correo electrónico válido")]
    [MaxLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario.
    /// Almacenada de forma hasheada por seguridad.
    /// </summary>
    /// <example>Contraseña123!</example>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [MaxLength(100, ErrorMessage = "La contraseña no puede exceder 100 caracteres")]
    public string Password { get; init; } = string.Empty;
}