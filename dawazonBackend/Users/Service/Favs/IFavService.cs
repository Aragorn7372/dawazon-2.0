using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Products.Models.Dto;

namespace dawazonBackend.Users.Service.Favs;

/// <summary>
/// Interfaz que define el contrato para el servicio de gestión de productos favoritos.
/// </summary>
/// <remarks>
/// Proporciona métodos para administrar los productos favoritos de los usuarios.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Añadir productos a favoritos</item>
///     <item>Eliminar productos de favoritos</item>
///     <item>Obtener lista de favoritos con paginación</item>
/// </list>
///
/// <para><b>Uso típico:</b></para>
/// <code>
/// IFavService favService = httpContext.RequestServices.GetRequiredService&lt;IFavService&gt;();
///
/// // Añadir a favoritos
/// var addResult = await favService.AddFav("product123", userId);
///
/// // Obtener favoritos paginados
/// var filters = new FilterDto { Page = 0, Size = 10, SortBy = "name" };
/// var favsResult = await favService.GetFavs(userId, filters);
/// </code>
/// </remarks>
public interface IFavService
{
    /// <summary>
    /// Añade un producto a la lista de favoritos del usuario.
    /// </summary>
    /// <param name="productId">El identificador único del producto a añadir.</param>
    /// <param name="userId">El identificador único del usuario.</param>
    /// <returns>
    /// Un <see cref="Result{bool, DomainError}"/> que puede ser:
    /// <list type="bullet">
    ///     <item>Exitoso: true si se añadió correctamente</item>
    ///     <item>Fallido: un <see cref="DomainError"/> indicando el error</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para><b>Errores posibles:</b></para>
    /// <list type="bullet">
    ///     <item><see cref="UserNotFoundError"/>: Si el usuario no existe</item>
    ///     <item><see cref="UserHasThatProductError"/>: Si el producto ya está en favoritos</item>
    ///     <item><see cref="UserError"/>: Si falla la actualización del usuario</item>
    /// </list>
    /// 
    /// <para><b>Nota:</b> No se permiten productos duplicados en la lista de favoritos.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await favService.AddFav("PROD-001", 123);
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine("Producto añadido a favoritos");
    /// }
    /// </code>
    /// </example>
    Task<Result<bool, DomainError>> AddFav(string productId, long userId);

    /// <summary>
    /// Elimina un producto de la lista de favoritos del usuario.
    /// </summary>
    /// <param name="productId">El identificador único del producto a eliminar.</param>
    /// <param name="userId">El identificador único del usuario.</param>
    /// <returns>
    /// Un <see cref="Result{bool, DomainError}"/> que puede ser:
    /// <list type="bullet">
    ///     <item>Exitoso: true si se eliminó correctamente</item>
    ///     <item>Fallido: un <see cref="DomainError"/> indicando el error</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para><b>Errores posibles:</b></para>
    /// <list type="bullet">
    ///     <item><see cref="UserNotFoundError"/>: Si el usuario no existe</item>
    ///     <item><see cref="UserHasThatProductError"/>: Si el producto no está en favoritos</item>
    ///     <item><see cref="UserError"/>: Si falla la actualización del usuario</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await favService.RemoveFav("PROD-001", 123);
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine("Producto eliminado de favoritos");
    /// }
    /// </code>
    /// </example>
    Task<Result<bool, DomainError>> RemoveFav(string productId, long userId);

    /// <summary>
    /// Obtiene la lista de productos favoritos del usuario con paginación.
    /// </summary>
    /// <param name="userId">El identificador único del usuario.</param>
    /// <param name="pageable">Objeto <see cref="FilterDto"/> con los filtros de paginación y ordenamiento.</param>
    /// <returns>
    /// Un <see cref="Result{PageResponseDto{ProductResponseDto}, DomainError}"/> que puede ser:
    /// <list type="bullet">
    ///     <item>Exitoso: <see cref="PageResponseDto{ProductResponseDto}"/> con los productos favoritos</item>
    ///     <item>Fallido: un <see cref="DomainError"/> indicando el error</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para><b>Campos de ordenamiento soportados:</b></para>
    /// <list type="bullet">
    ///     <item>"name" - Ordenar por nombre del producto</item>
    ///     <item>"price" - Ordenar por precio</item>
    ///     <item>"stock" - Ordenar por cantidad en stock</item>
    ///     <item>cualquier otro valor - Ordenar por ID (comportamiento por defecto)</item>
    /// </list>
    /// 
    /// <para><b>Valores por defecto:</b></para>
    /// <list type="bullet">
    ///     <item>Si Page &lt; 0: se usa 0</item>
    ///     <item>Si Size &lt;= 0: se usa 10</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var filters = new FilterDto
    /// {
    ///     Page = 0,
    ///     Size = 20,
    ///     SortBy = "price",
    ///     Direction = "asc"
    /// };
    ///
    /// var result = await favService.GetFavs(userId, filters);
    /// if (result.IsSuccess)
    /// {
    ///     foreach (var product in result.Value.Content)
    ///     {
    ///         Console.WriteLine($"- {product.Name}: {product.Price}€");
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<Result<PageResponseDto<ProductResponseDto>, DomainError>> GetFavs(long userId, FilterDto pageable);
}
