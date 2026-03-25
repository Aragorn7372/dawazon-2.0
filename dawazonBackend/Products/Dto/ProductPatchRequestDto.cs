using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Products.Models.Dto;

/// <summary>
/// DTO para actualización parcial de un producto.
/// </summary>
/// <remarks>
/// Se utiliza en solicitudes PATCH. Todos los campos son opcionales,
/// permitiendo actualizar solo los campos que se deseen modificar.
/// </remarks>
public record ProductPatchRequestDto(
    /// <summary>
    /// ID del producto a actualizar.
    /// </summary>
    string? Id,

    /// <summary>
    /// Nuevo nombre (opcional).
    /// </summary>
    string? Name,

    /// <summary>
    /// Nuevo precio (opcional).
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
    double? Price,

    /// <summary>
    /// Nueva categoría (opcional).
    /// </summary>
    string? Category,

    /// <summary>
    /// Nueva descripción (opcional).
    /// </summary>
    string? Description,

    /// <summary>
    /// Nuevas imágenes (opcional).
    /// </summary>
    List<string>? Images,

    /// <summary>
    /// Nuevo stock (opcional).
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser inferior a 0")]
    int? Stock,

    /// <summary>
    /// ID del creador (opcional).
    /// </summary>
    long? CreatorId
    );
