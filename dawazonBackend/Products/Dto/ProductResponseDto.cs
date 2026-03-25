namespace dawazonBackend.Products.Models.Dto;

/// <summary>
/// DTO de respuesta para un producto.
/// </summary>
/// <remarks>
/// Se utiliza para devolver los datos de un producto en la API.
/// Incluye todos los campos del producto más los comentarios asociados.
/// </remarks>
public record ProductResponseDto(
    /// <summary>
    /// ID único del producto.
    /// </summary>
    string Id,

    /// <summary>
    /// Nombre del producto.
    /// </summary>
    string Name,

    /// <summary>
    /// Precio del producto.
    /// </summary>
    double Price,

    /// <summary>
    /// Cantidad en stock.
    /// </summary>
    int Stock,

    /// <summary>
    /// Nombre de la categoría.
    /// </summary>
    string Category,

    /// <summary>
    /// Descripción del producto.
    /// </summary>
    string Description,

    /// <summary>
    /// Lista de comentarios del producto.
    /// </summary>
    List<CommentDto> Comments,

    /// <summary>
    /// Lista de URLs de imágenes.
    /// </summary>
    List<string> Images,

    /// <summary>
    /// ID del creador del producto.
    /// </summary>
    long CreatorId = 0
);