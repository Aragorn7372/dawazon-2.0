using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Products.Models.Dto;

public record ProductRequestDto(
    string? Id,

    [Required(ErrorMessage = "El nombre no puede estar vacío")]
    string Name,

    [Required(ErrorMessage = "El precio no puede estar vacío")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
    double Price,

    [Required(ErrorMessage = "La categoría no puede estar vacía")]
    string Category,

    string? Description,

    List<string>? Images,

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser inferior a 0")]
    int? Stock,

    long? CreatorId
);