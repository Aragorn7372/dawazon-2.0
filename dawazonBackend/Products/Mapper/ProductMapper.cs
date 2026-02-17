using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;

namespace dawazonBackend.Products.Mapper;

public static class ProductMapper
{
    /// <summary>
    /// Convierte un modelo de base de datos Product a un ProductResponseDto.
    /// </summary>
    public static ProductResponseDto ToDto(this Product model)
    {
        return new ProductResponseDto(
            Id: model.Id ?? string.Empty,
            Name: model.Name,
            Price: model.Price,
            Stock: model.Stock,
            Category: model.Category?.Name ?? model.CategoryId, // Intenta usar el nombre, si no, usa el ID
            Description: model.Description,
            Comments: model.Comments.Select(c => c.ToDto()).ToList(),
            Images: model.Images
        );
    }

    /// <summary>
    /// Convierte un ProductRequestDto (Create/Update) a un modelo Product.
    /// Nota: Campos como Id, CreatedAt, UpdatedAt se manejan en el servicio/repositorio.
    /// </summary>
    public static Product ToModel(this ProductRequestDto dto)
    {
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock ?? 0,
            Description = dto.Description ?? string.Empty,
            // La categoría se resuelve en el servicio buscando por nombre
            CategoryId = string.Empty, 
            CreatorId = dto.CreatorId ?? 0,
            Images = dto.Images ?? [],
            IsDeleted = false,
            Comments = []
        };
    }

    /// <summary>
    /// Convierte un modelo Comment a CommentDto.
    /// </summary>
    public static CommentDto ToDto(this Comment model)
    {
        
        return new CommentDto(
            userName: model.UserId.ToString(),
            comment: model.Content,
            recommended: model.recommended,
            verified: model.verified
        );
    }
}