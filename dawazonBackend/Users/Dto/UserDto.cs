using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace dawazonBackend.Users.Dto;

/// <summary>
/// DTO que representa la información de un usuario en el sistema.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "El id debe ser mayor que 0")]
    public long? Id { get; set; }

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; }=string.Empty;

    /// <summary>
    /// Dirección de correo electrónico.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }=string.Empty;

    /// <summary>
    /// Número de teléfono (limpio de caracteres especiales).
    /// </summary>
    [RegularExpression(@"^(\d{9})?$", ErrorMessage = "El teléfono debe tener 9 dígitos o estar vacío")]
    public string? Telefono { get; set; }

    /// <summary>
    /// Roles asignados al usuario.
    /// </summary>
    public HashSet<string> Roles { get; set; } = new();

    /// <summary>
    /// Calle de la dirección.
    /// </summary>
    public string? Calle { get; set; }

    /// <summary>
    /// Ciudad de residencia.
    /// </summary>
    public string? Ciudad { get; set; }

    /// <summary>
    /// Código postal.
    /// </summary>
    public string? CodigoPostal { get; set; }

    /// <summary>
    /// Provincia o región.
    /// </summary>
    public string? Provincia { get; set; }
    
    
    public string Avatar { get; set; } = string.Empty;

    /// <summary>
    /// Limpia y establece el número de teléfono.
    /// </summary>
    /// <param name="telefono">El número de teléfono a procesar.</param>
    public void SetTelefono(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            Telefono = "";
            return;
        }

        // Eliminar espacios, guiones, paréntesis, etc.
        string cleaned = Regex.Replace(telefono, @"[\s\-().]", "");

        // Si empieza con +34, quitarlo
        if (cleaned.StartsWith("+34"))
        {
            cleaned = cleaned.Substring(3);
        }
        // Si empieza con 0034, quitarlo
        else if (cleaned.StartsWith("0034"))
        {
            cleaned = cleaned.Substring(4);
        }
        // Si empieza con 34 y tiene más de 9 dígitos
        else if (cleaned.StartsWith("34") && cleaned.Length > 9)
        {
            cleaned = cleaned.Substring(2);
        }

        Telefono = cleaned;
    }
}

