using System.ComponentModel.DataAnnotations;
using dawazonBackend.Common.Attribute;

namespace dawazonBackend.Products.Models;

/// <summary>
/// Modelo de dominio que representa una categoría de productos.
/// </summary>
/// <remarks>
/// Las categorías organizan los productos en grupos (Figuras, Comics, Ropa).
/// Se utiliza para filtrar y buscar productos.
/// </remarks>
public class Category
{
    /// <summary>
    /// Identificador único de la categoría.
    /// </summary>
    [Key]
    [GenerateCustomIdAtribute]
    public string Id { get; set; }= string.Empty;

    /// <summary>
    /// Nombre de la categoría.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Fecha de última actualización.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}