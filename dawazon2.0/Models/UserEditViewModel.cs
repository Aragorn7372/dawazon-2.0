using System.ComponentModel.DataAnnotations;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para el formulario de edición del perfil del usuario.
/// </summary>
/// <remarks>
/// Permite modificar datos personales, dirección y avatar.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>DataAnnotations: Validaciones</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Validaciones: Required, MaxLength, EmailAddress, RegularExpression</item>
///     <item>Dirección completa</item>
///     <item>Soporte para cambio de avatar</item>
/// </list>
/// </remarks>
public class UserEditViewModel
{
    /// <summary>Nombre completo del usuario.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    ///     <item>MaxLength: Máximo 50 caracteres</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50)]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Correo electrónico del usuario.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    ///     <item>EmailAddress: Formato válido</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email no válido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Número de teléfono de contacto.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>RegularExpression: 9 dígitos o vacío</item>
    /// </list>
    /// </remarks>
    [RegularExpression(@"^(\d{9})?$", ErrorMessage = "El teléfono debe tener 9 dígitos o estar vacío")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    /// <summary>Dirección - Calle y número.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "La calle es obligatoria")]
    [Display(Name = "Calle y número")]
    public string Calle { get; set; } = string.Empty;

    /// <summary>Dirección - Ciudad.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatoria</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "La ciudad es obligatoria")]
    [Display(Name = "Ciudad")]
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>Dirección - Código postal.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El código postal es obligatorio")]
    [Display(Name = "Código Postal")]
    public string CodigoPostal { get; set; } = string.Empty;

    /// <summary>Dirección - Provincia.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatoria</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "La provincia es obligatoria")]
    [Display(Name = "Provincia")]
    public string Provincia { get; set; } = string.Empty;
    
    /// <summary>Archivo de avatar a subir.</summary>
    /// <remarks>
    /// Opcional. Si se proporciona, reemplaza el avatar actual.
    /// </remarks>
    [Display(Name = "Avatar")]
    public IFormFile? Avatar { get; set; }

    /// <summary>Rol del usuario (solo lectura, para admins).</summary>
    [Display(Name = "Rol del usuario")]
    public string? Rol { get; set; }
}
