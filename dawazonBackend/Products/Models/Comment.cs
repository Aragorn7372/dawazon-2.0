using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Products.Models;

/// <summary>
/// Representa un comentario en un producto.
/// </summary>
/// <remarks>
/// Entidad que almacena los comentarios/reseñas de los usuarios sobre los productos.
/// </remarks>
public class Comment
{
    /// <summary>
    /// ID del usuario que hizo el comentario.
    /// </summary>
    [Required]
    public int UserId { get; set; }
    
    /// <summary>
    /// Contenido del comentario.
    /// </summary>
    [Required]
    [MaxLength(200)]
    [MinLength(2)]
    public string Content { get; set; }= string.Empty;
    
    /// <summary>
    /// Fecha de creación del comentario.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Indica si el comentario es de un usuario verificado (con compra).
    /// </summary>
    [Required]
    public bool verified { get; set; } = false;
    
    /// <summary>
    /// Indica si el usuario recomienda el producto.
    /// </summary>
    [Required]
    public bool recommended { get; set; } = false;
}