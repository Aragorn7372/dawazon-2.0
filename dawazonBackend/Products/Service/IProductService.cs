using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;

namespace dawazonBackend.Products.Service;

/// <summary>
/// Interfaz para el servicio de gestión de productos.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Obtiene todos los productos de forma paginada y filtrada.
    /// Opcionalmente, permite filtrar solo los productos creados por un usuario específico.
    /// </summary>
    /// <param name="filter">Filtros de búsqueda.</param>
    /// <param name="creatorId">Opcional. ID del creador para filtrar (ej. para "Mis productos").</param>
    /// <returns>Paginación de productos.</returns>
    Task<PageResponseDto<ProductResponseDto>> GetAllAsync(FilterDto filter, long? creatorId = null);
    
    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    /// <param name="id">El ID del producto (GUID como string).</param>
    /// <returns>Un resultado con el DTO del producto o un error.</returns>
    Task<Result<ProductResponseDto, ProductError>> GetByIdAsync(string id);
    
    /// <summary>
    /// Obtiene el ID numérico interno de un producto a partir de su ID de cadena.
    /// </summary>
    /// <param name="id">El ID del producto.</param>
    /// <returns>El ID numérico o un error.</returns>
    Task<Result<long, ProductError>> GetUserProductIdAsync(string id);
    
    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <param name="dto">Los datos del nuevo producto.</param>
    /// <returns>El DTO del producto creado o un error.</returns>
    Task<Result<ProductResponseDto, ProductError>> CreateAsync(ProductRequestDto dto);
    
    /// <summary>
    /// Actualiza un producto existente (reemplazo completo).
    /// </summary>
    /// <param name="id">El ID del producto a actualizar.</param>
    /// <param name="dto">Los nuevos datos del producto.</param>
    /// <returns>El DTO actualizado o un error.</returns>
    Task<Result<ProductResponseDto, ProductError>> UpdateAsync(string id, ProductRequestDto dto);
    
    /// <summary>
    /// Actualiza parcialmente un producto.
    /// </summary>
    /// <param name="id">El ID del producto.</param>
    /// <param name="dto">Los campos a actualizar.</param>
    /// <returns>El DTO actualizado o un error.</returns>
    Task<Result<ProductResponseDto, ProductError>> PatchAsync(string id, ProductPatchRequestDto dto);
    
    /// <summary>
    /// Elimina un producto (borrado lógico o físico según implementación).
    /// </summary>
    /// <param name="id">El ID del producto a eliminar.</param>
    /// <returns>El DTO del producto eliminado o un error.</returns>
    Task<Result<ProductResponseDto, ProductError>> DeleteAsync(string id);

    /// <summary>
    /// Actualiza las imágenes asociadas a un producto.
    /// </summary>
    /// <param name="id">El ID del producto.</param>
    /// <param name="images">Lista de archivos de imagen.</param>
    /// <returns>El DTO actualizado o un error.</returns>
    Task<Result<ProductResponseDto, ProductError>> UpdateImageAsync(string id, List<IFormFile> images);

    /// <summary>
    /// Obtiene todas las categorías de productos disponibles.
    /// </summary>
    /// <returns>Una lista de nombres de categorías.</returns>
    Task<List<string>> GetAllCategoriesAsync();

    /// <summary>
    /// Añade un comentario a un producto.
    /// </summary>
    /// <param name="id">El ID del producto.</param>
    /// <param name="comment">El objeto comentario a añadir.</param>
    /// <returns>El DTO del producto con el nuevo comentario o un error.</returns>
    Task<Result<ProductResponseDto, ProductError>> AddCommentAsync(string id, Comment comment);
}