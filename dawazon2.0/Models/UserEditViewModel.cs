using System.ComponentModel.DataAnnotations;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para el formulario de edición del perfil del usuario.
/// </summary>
public class UserEditViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50)]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email no válido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^(\d{9})?$", ErrorMessage = "El teléfono debe tener 9 dígitos o estar vacío")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "La calle es obligatoria")]
    [Display(Name = "Calle y número")]
    public string Calle { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria")]
    [Display(Name = "Ciudad")]
    public string Ciudad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código postal es obligatorio")]
    [Display(Name = "Código Postal")]
    public string CodigoPostal { get; set; } = string.Empty;

    [Required(ErrorMessage = "La provincia es obligatoria")]
    [Display(Name = "Provincia")]
    public string Provincia { get; set; } = string.Empty;
    
    [Display(Name = "Avatar")]
    public IFormFile? Avatar { get; set; }
}
