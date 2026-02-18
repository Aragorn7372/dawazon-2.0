using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace dawazonBackend.Users.Dto;

public class UserRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    private string? _telefono;

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
    [Required]
    public string Calle { get; set; } = string.Empty;
    [Required]
    public string Ciudad { get; set; }=string.Empty;
    [Required]
    public string CodigoPostal { get; set; }=string.Empty;
    [Required]
    public string Provincia { get; set; }=string.Empty;
}