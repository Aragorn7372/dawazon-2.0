using System.ComponentModel.DataAnnotations;
using dawazonBackend.Cart.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Models;

/// <summary>
/// Representa a un usuario en el sistema, extendiendo la funcionalidad de IdentityUser.
/// </summary>
public class User: IdentityUser<long>
{
    /// <summary>
    /// Nombre de la imagen por defecto para el avatar.
    /// </summary>
    public const string DEFAULT_IMAGE = "default.png";

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