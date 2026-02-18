using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace dawazonBackend.Users.Dto;

public class UserDto
{
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "El id debe ser mayor que 0")]
    public long? Id { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; }=string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; }=string.Empty;

    [RegularExpression(@"^(\d{9})?$", ErrorMessage = "El teléfono debe tener 9 dígitos o estar vacío")]
    public string? Telefono { get; set; }

    public HashSet<string> Roles { get; set; } = new();

    // Campos de dirección del cliente
    public string? Calle { get; set; }
    public string? Ciudad { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Provincia { get; set; }

    // Setter personalizado equivalente
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

