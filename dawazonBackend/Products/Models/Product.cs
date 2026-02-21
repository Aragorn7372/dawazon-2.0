using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using dawazonBackend.Common.Attribute;

namespace dawazonBackend.Products.Models;

/// <summary>
/// Representa un producto en el catálogo de Dawazon.
/// </summary>
public class Product
{
    /// <summary>
    /// Identificador único del producto (Custom ID).
    /// </summary>
    [Key]
    [GenerateCustomIdAtribute]
    [StringLength(12)]
    public string? Id { get; set; }
    /// <summary>
    /// Nombre del producto.
    /// </summary>
    [Required]
    [StringLength(200,MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Precio del producto en euros.
    /// </summary>
    [Required]
    [Range(0,9999.99)]
    public double Price { get; set; }

    /// <summary>
    /// Cantidad de stock disponible.
    /// </summary>
    [Required]
    [Range(0,9999)]
    public int Stock { get; set; }

    /// <summary>
    /// Descripción detallada del producto.
    /// </summary>
    [Required]
    [StringLength(300,MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario que creó el producto.
    /// </summary>
    [Required]
    public long CreatorId { get; set; }

    /// <summary>
    /// ID de la categoría a la que pertenece.
    /// </summary>
    [Required]
    public string CategoryId { get; set; }= string.Empty;

    /// <summary>
    /// Lista de comentarios asociados al producto.
    /// </summary>
    public List<Comment> Comments { get; set; } = [];
    /// <summary>
    /// Propiedad de navegación a la categoría relacionada.
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    /// <summary>
    /// Lista de URLs de imágenes del producto.
    /// </summary>
    public List<string> Images { get; set; } = [];

    /// <summary>
    /// Indica si el producto ha sido eliminado (borrado lógico).
    /// </summary>
    [Required]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Fecha de creación del registro.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }= DateTime.UtcNow;

    /// <summary>
    /// Fecha de la última actualización del registro.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Versión del registro para control de concurrencia optimista.
    /// </summary>
    [ConcurrencyCheck]
    public long Version { get; set; }

}