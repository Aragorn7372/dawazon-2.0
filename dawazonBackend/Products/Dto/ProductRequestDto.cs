using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Products.Models.Dto;

/// <summary>
/// DTO para crear o actualizar un producto.
/// </summary>
/// <remarks>
/// Se utiliza en las solicitudes POST y PUT de la API de productos.
/// Todos los campos son requeridos excepto Images y CreatorId.
/// </remarks>
public record ProductRequestDto(
    /// <summary>
    /// ID opcional (para actualizaciones).
    /// </summary>
    string? Id,

    /// <summary>
    /// Nombre del producto.
    /// </summary>
    [Required(ErrorMessage = "El nombre no puede estar vacío")]
    string Name,

    /// <summary>
    /// Precio del producto.
    /// </summary>
    [Required(ErrorMessage = "El precio no puede estar vacío")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
    double Price,

    /// <summary>
    /// Categoría del producto.
    /// </summary>
    [Required(ErrorMessage = "La categoría no puede estar vacía")]
    string Category,

    /// <summary>
    /// Descripción del producto.
    /// </summary>
    [Required(ErrorMessage = "La descripción no puede estar vacía")]
    string Description,

    /// <summary>
    /// Lista de URLs de imágenes del producto.
    /// </summary>
    List<string>? Images,

    /// <summary>
    /// Cantidad en stock.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser inferior a 0")]
    int Stock,

    /// <summary>
    /// ID del creador/usuario que crea el producto.
    /// </summary>
    long? CreatorId
);