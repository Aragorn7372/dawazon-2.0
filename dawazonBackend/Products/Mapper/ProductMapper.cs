using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;

namespace dawazonBackend.Products.Mapper;

using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;

/// <summary>
/// Clase de utilidad estática para mapear entre modelos de dominio y DTOs de productos.
/// </summary>
/// <remarks>
/// Proporciona métodos de extensión para convertir entre:
/// <list type="bullet">
///     <item>Product ↔ ProductResponseDto</item>
///     <item>ProductRequestDto ↔ Product</item>
///     <item>Comment ↔ CommentDto</item>
/// </list>
/// 
/// <para><b>Patrón:</b></b>
/// Métodos de extensión estáticos (ToDto, ToModel).
/// </remarks>
public static class ProductMapper
{
    /// <summary>
    /// Convierte un modelo Product a ProductResponseDto.
    /// </summary>
    /// <param name="model">Modelo de base de datos.</param>
    /// <returns>DTO de respuesta.</returns>
    /// <remarks>
    /// Mapea Category: usa Name si está cargado, sino usa CategoryId.
    /// </remarks>
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
            Images: model.Images,
            CreatorId: model.CreatorId
        );
    }

    /// <summary>
    /// Convierte un ProductRequestDto a modelo Product.
    /// </summary>
    /// <param name="dto">DTO de solicitud.</param>
    /// <returns>Modelo de producto.</returns>
    /// <remarks>
    /// Id, CategoryId, CreatedAt, UpdatedAt se manejan en servicio/repositorio.
    /// </remarks>
    public static Product ToModel(this ProductRequestDto dto)
    {
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            Description = dto.Description,
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
    /// <param name="model">Modelo de comentario.</param>
    /// <returns>DTO de comentario.</returns>
    /// <remarks>
    /// El userName se mapea desde UserId como string.
    /// </remarks>
    public static CommentDto ToDto(this Comment model)
    {
        
        return new CommentDto(
            userName: model.UserId.ToString(),
            comment: model.Content,
            recommended: model.recommended,
            verified: model.verified
        );
    }

    /// <summary>
    /// Crea una copia de ProductRequestDto permitiendo sobrescribir campos.
    /// </summary>
    /// <param name="original">DTO original.</param>
    /// <param name="Id">Nuevo ID (opcional).</param>
    /// <param name="Name">Nuevo nombre.</param>
    /// <param name="Price">Nuevo precio.</param>
    /// <param name="Category">Nueva categoría.</param>
    /// <param name="Description">Nueva descripción.</param>
    /// <param name="Images">Nuevas imágenes.</param>
    /// <param name="Stock">Nuevo stock.</param>
    /// <param name="CreatorId">Nuevo CreatorId.</param>
    /// <returns>Nuevo DTO con los valores sobrescritos.</returns>
    /// <remarks>
    /// Útil para tests o transformaciones rápidas. Null mantiene el valor original.
    /// </remarks>
    public static ProductRequestDto Copy(
        this ProductRequestDto original,
        string? Id = null,
        string? Name = null,
        double? Price = null,
        string? Category = null,
        string? Description = null,
        List<string>? Images = null,
        int? Stock = null,
        long? CreatorId = null)
    {
        return new ProductRequestDto(
            Id: Id ?? original.Id,
            Name: Name ?? original.Name,
            Price: Price ?? original.Price,
            Category: Category ?? original.Category,
            Description: Description ?? original.Description,
            Images: Images ?? original.Images, // Nota: Si envías null, mantendrá la lista original
            Stock: Stock ?? original.Stock,
            CreatorId: CreatorId ?? original.CreatorId
        );
    }
}