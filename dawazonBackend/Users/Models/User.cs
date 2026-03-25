using System.ComponentModel.DataAnnotations;
using dawazonBackend.Cart.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Models;

/// <summary>
/// Entidad de usuario que extiende IdentityUser.
/// </summary>
/// <remarks>
/// Hereda de IdentityUser<long> para integración con ASP.NET Core Identity.
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Borrado lógico: IsDeleted</item>
///     <item>Productos favoritos: ProductsFavs</item>
///     <item>Datos de cliente: Client (dirección)</item>
///     <li>Avatar: Imagen de perfil</item>
/// </list>
/// </remarks>
public class User: IdentityUser<long>
{
    /// <summary>
    /// Nombre de la imagen por defecto para el avatar.
    /// </summary>
    public const string DEFAULT_IMAGE = "/uploads/users/default.png";

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el usuario ha sido borrado (borrado lógico).
    /// </summary>
    [Required]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Productos Favoritos del Usuario
    /// </summary>
    public List<string> ProductsFavs { get; set; } = new();
    /// <summary>
    /// Datos del cliente asociados al usuario (dirección, etc.).
    /// </summary>
    public Client Client { get; set; } = new Client();

    /// <summary>
    /// Ruta o nombre del archivo de avatar del usuario.
    /// </summary>
    public string Avatar { get; set; } =DEFAULT_IMAGE;
   
}