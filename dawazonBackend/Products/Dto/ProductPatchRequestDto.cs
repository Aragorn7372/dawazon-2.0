using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Products.Models.Dto;

public record ProductPatchRequestDto(
    string? Id,
    string? Name,
    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
    double? Price,
    string? Category,
    string? Description,
    List<string>? Images,
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser inferior a 0")]
    int? Stock,
    long? CreatorId
    );
