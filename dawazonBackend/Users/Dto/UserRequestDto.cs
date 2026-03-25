using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace dawazonBackend.Users.Dto;

/// <summary>
/// DTO para solicitudes de actualización de perfil de usuario.
/// </summary>
/// <remarks>
/// Se utiliza en las solicitudes PUT/PATCH del perfil de usuario.
/// Limpia automáticamente el teléfono de prefijos y caracteres especiales.
/// </remarks>
public class UserRequestDto
{
    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = default!;

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    private string? _telefono;

    /// <summary>
    /// Teléfono del usuario (se limpia automáticamente).
    /// </summary>
    /// <remarks>
    /// El setter elimina: espacios, guiones, paréntesis, prefijos (+34, 0034, 34).
    /// </remarks>
    [RegularExpression(@"^(\d{9})?$", ErrorMessage = "El teléfono debe tener 9 dígitos o estar vacío")]
    public string? Telefono
    {
        get => _telefono;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _telefono = "";
                return;
            }

            // Eliminar espacios, guiones, paréntesis, etc.
            string cleaned = Regex.Replace(value, @"[\s\-().]", "");

            // Si empieza con +34, quitarlo
            if (cleaned.StartsWith("+34"))
                cleaned = cleaned.Substring(3);
            // Si empieza con 0034, quitarlo
            else if (cleaned.StartsWith("0034"))
                cleaned = cleaned.Substring(4);
            // Si empieza con 34 y tiene más de 9 dígitos
            else if (cleaned.StartsWith("34") && cleaned.Length > 9)
                cleaned = cleaned.Substring(2);

            _telefono = cleaned;
        }
    }

    // Campos de dirección (opcionales)
    
    /// <summary>
    /// Calle de la dirección.
    /// </summary>
    [Required]
    public string Calle { get; set; } = string.Empty;
    
    /// <summary>
    /// Ciudad de residencia.
    /// </summary>
    [Required]
    public string Ciudad { get; set; }=string.Empty;
    
    /// <summary>
    /// Código postal.
    /// </summary>
    [Required]
    public string CodigoPostal { get; set; }=string.Empty;
    
    /// <summary>
    /// Provincia o región.
    /// </summary>
    [Required]
    public string Provincia { get; set; }=string.Empty;
}