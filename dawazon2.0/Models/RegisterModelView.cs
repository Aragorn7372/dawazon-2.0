using System.ComponentModel.DataAnnotations;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para el formulario de registro de nuevo usuario.
/// </summary>
/// <remarks>
/// Recoge los datos para crear una nueva cuenta de usuario.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>DataAnnotations: Validaciones</item>
/// </list>
/// 
/// <para><b>Validaciones:</b></para>
/// <list type="bullet">
///     <item>Username: Required, MinLength(3), Regex (alphanumeric + underscore)</item>
///     <item>Email: Required, EmailAddress</item>
///     <item>Password: Required, MinLength(6)</item>
///     <item>ConfirmPassword: Required, Compare</item>
/// </list>
/// </remarks>
public class RegisterModelView
{
    /// <summary>Nombre de usuario único.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    ///     <item>MinLength: Mínimo 3 caracteres</item>
    ///     <item>Regex: Solo letras, números y guiones bajos</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [MinLength(3, ErrorMessage = "Mínimo 3 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Solo letras, números y guiones bajos")]
    public string Username { get; set; } = string.Empty;

    /// <summary>Correo electrónico único.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    ///     <item>EmailAddress: Formato válido de email</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Contraseña del usuario.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatoria</item>
    ///     <item>MinLength: Mínimo 6 caracteres</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Confirmación de contraseña.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatoria</item>
    ///     <item>Compare: Debe coincidir con Password</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "Debes confirmar la contraseña")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;
}