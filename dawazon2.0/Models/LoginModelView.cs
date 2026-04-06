using System.ComponentModel.DataAnnotations;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para el formulario de inicio de sesión.
/// </summary>
/// <remarks>
/// Recoge las credenciales del usuario para autenticar.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>DataAnnotations: Validaciones</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Validaciones: Required</item>
///     <item>Soporta username o email</item>
/// </list>
/// </remarks>
public class LoginModelView
{
    /// <summary>
    /// Nombre de usuario o correo electrónico.
    /// </summary>
    /// <remarks>
    /// Identifica al usuario en el sistema.
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    /// </list>
    /// </remarks>
    /// <example>juanperez</example>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    public string UsernameOrEmail { get; init; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    /// <remarks>
    /// Verificada contra el hash almacenado en la base de datos.
    /// <list type="bullet">
    ///     <item>Required: Obligatoria</item>
    /// </list>
    /// </remarks>
    /// <example>Contraseña123!</example>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; init; } = string.Empty;
    
}