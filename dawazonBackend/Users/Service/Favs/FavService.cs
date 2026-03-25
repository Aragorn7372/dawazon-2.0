using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Products.Mapper;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Products.Repository.Productos;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Service.Favs;

/// <summary>
/// Implementación del servicio de gestión de productos favoritos.
/// </summary>
/// <remarks>
/// Proporciona la lógica de negocio para administrar los productos favoritos de los usuarios.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item><see cref="ILogger{FavService}"/>: Para logging de operaciones</item>
///     <item><see cref="UserManager{User}"/>: Para operaciones de usuario en Identity</item>
///     <item><see cref="IProductRepository"/>: Para obtener datos de productos</item>
/// </list>
///
/// <para><b>Patrones utilizados:</b></para>
/// <list type="bullet">
///     <item>Result Pattern: Para manejo de errores con CSharpFunctionalExtensions</item>
///     <item>Repository Pattern: A través de IProductRepository</item>
/// </list>
/// </remarks>
public class FavService(ILogger<FavService> logger,UserManager<User> manager, IProductRepository products): IFavService
{
    /// <inheritdoc/>
    /// <summary>
    /// Añade un producto a la lista de favoritos del usuario.
    /// </summary>
    /// <param name="productId">El identificador del producto.</param>
    /// <param name="userId">El identificador del usuario.</param>
    /// <returns>
    /// <see cref="Result{bool, DomainError}"/> con true si fue exitoso.
    /// </returns>
    /// <remarks>
    /// <para><b>Proceso:</b></para>
    /// <list type="number">
    ///     <item>Busca el usuario por ID</item>
    ///     <item>Verifica que el producto no esté ya en favoritos</item>
    ///     <item>Añade el producto a la lista</item>
    ///     <item>Persiste los cambios</item>
    /// </list>
    /// </remarks>
    public async Task<Result<bool,DomainError>> AddFav(string productId, long userId)
    {
        logger.LogInformation("Añadiendo a favoritos producto con id: {ProductId} para usuario: {UserId}", productId, userId);
        
        var user = await manager.FindByIdAsync(userId.ToString());
        if(user== null) 
        {
            logger.LogWarning("Usuario no encontrado: {UserId}", userId);
            return Result.Failure<bool,DomainError>(new UserNotFoundError("no se encontro usuario con ese id"));
        }
        
        if (user.ProductsFavs.Contains(productId)) 
        {
            logger.LogWarning("Producto {ProductId} ya está en favoritos del usuario {UserId}", productId, userId);
            return Result.Failure<bool,DomainError>(new UserHasThatProductError("Ese usuario tenía ya ese producto guardado"));
        }
        
        user.ProductsFavs.Add(productId);
        logger.LogDebug("Producto añadido a la lista de favoritos");
        
        var updated=await manager.UpdateAsync(user);
        if (!updated.Succeeded) 
        {
            logger.LogError("Error al actualizar favoritos: {Errors}", string.Join(", ", updated.Errors.Select(x => x.Description)));
            return Result.Failure<bool,DomainError>(new UserError(string.Join(", ", updated.Errors.Select(x => x.Description))));
        }
        
        logger.LogInformation("Producto {ProductId} añadido a favoritos del usuario {UserId}", productId, userId);
        return Result.Success<bool,DomainError>(updated.Succeeded);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Elimina un producto de la lista de favoritos del usuario.
    /// </summary>
    /// <param name="productId">El identificador del producto.</param>
    /// <param name="userId">El identificador del usuario.</param>
    /// <returns>
    /// <see cref="Result{bool, DomainError}"/> con true si fue exitoso.
    /// </returns>
    /// <remarks>
    /// <para><b>Proceso:</b></para>
    /// <list type="number">
    ///     <item>Busca el usuario por ID</item>
    ///     <item>Verifica que el producto esté en favoritos</item>
    ///     <item>Elimina el producto de la lista</item>
    ///     <item>Persiste los cambios</item>
    /// </list>
    /// </remarks>
    public async Task<Result<bool,DomainError>> RemoveFav(string productId, long userId)
    {
        logger.LogInformation("Quitando de favoritos producto con id {ProductId} para usuario: {UserId}", productId, userId);
        
        var user = await manager.FindByIdAsync(userId.ToString());
        if(user== null) 
        {
            logger.LogWarning("Usuario no encontrado: {UserId}", userId);
            return Result.Failure<bool,DomainError>(new UserNotFoundError("no se encontro usuario con ese id"));
        }
        
        if (!user.ProductsFavs.Contains(productId)) 
        {
            logger.LogWarning("Producto {ProductId} no está en favoritos del usuario {UserId}", productId, userId);
            return Result.Failure<bool,DomainError>(new UserHasThatProductError("Ese usuario no tiene ese producto guardado"));
        }
        
        user.ProductsFavs.Remove(productId);
        logger.LogDebug("Producto eliminado de la lista de favoritos");
        
        var updated=await manager.UpdateAsync(user);
        if (!updated.Succeeded) 
        {
            logger.LogError("Error al actualizar favoritos: {Errors}", string.Join(", ", updated.Errors.Select(x => x.Description)));
            return Result.Failure<bool,DomainError>(new UserError(string.Join(", ", updated.Errors.Select(x => x.Description))));
        }
        
        logger.LogInformation("Producto {ProductId} eliminado de favoritos del usuario {UserId}", productId, userId);
        return Result.Success<bool,DomainError>(updated.Succeeded);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Obtiene la lista de productos favoritos del usuario con paginación.
    /// </summary>
    /// <param name="userId">El identificador del usuario.</param>
    /// <param name="pageable">Filtros de paginación y ordenamiento.</param>
    /// <returns>
    /// <see cref="Result{PageResponseDto{ProductResponseDto}, DomainError}"/> con la lista paginada.
    /// </returns>
    /// <remarks>
    /// <para><b>Proceso:</b></para>
    /// <list type="number">
    ///     <item>Busca el usuario por ID</item>
    ///     <item>Obtiene los productos de la lista de favoritos</item>
    ///     <li>Convierte a DTOs</item>
    ///     <li>Aplica ordenamiento</item>
    ///     <item>Aplica paginación</item>
    /// </list>
    /// 
    /// <para><b>Nota:</b> Los productos que no se encuentran en la base de datos son excluidos.</para>
    /// </remarks>
    public async Task<Result<PageResponseDto<ProductResponseDto>,DomainError>> GetFavs(long userId, FilterDto pageable)
    {
        logger.LogInformation("Buscando productos favoritos para usuario: {UserId}", userId);
        
        var user = await manager.FindByIdAsync(userId.ToString());
        if(user== null) 
        {
            logger.LogWarning("Usuario no encontrado: {UserId}", userId);
            return Result.Failure<PageResponseDto<ProductResponseDto>,DomainError>(new UserNotFoundError("no se encontro usuario con ese id"));
        }
        
        logger.LogDebug("Usuario tiene {Count} productos en favoritos", user.ProductsFavs.Count);
        
        var productsList = (await Task.WhenAll(
                user.ProductsFavs.Select(it => products.GetProductAsync(it))
            ))
            .OfType<Product>()
            .Select(it=>it.ToDto())
            .ToList();
        
        productsList = ApplySorting(productsList, pageable.SortBy, pageable.Direction);
        
        var page = pageable.Page < 0 ? 0 : pageable.Page;
        var size = pageable.Size <= 0 ? 10 : pageable.Size;

        var response = productsList
            .Skip(page * size)
            .Take(size)
            .ToList();
            
        var totalCount = productsList.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / pageable.Size);
        
        logger.LogInformation("Se encontraron {Count} productos favoritos (página {Page}/{TotalPages})", 
            response.Count, page + 1, totalPages);
        
        return Result.Success<PageResponseDto<ProductResponseDto>,DomainError>(new PageResponseDto<ProductResponseDto>(
            Content: response,
            TotalPages: totalPages,
            TotalElements: totalCount,
            PageSize: pageable.Size,
            PageNumber: pageable.Page,
            TotalPageElements: response.Count,
            SortBy: pageable.SortBy,
            Direction: pageable.Direction));
    }

    /// <summary>
    /// Aplica el ordenamiento a la lista de productos favoritos.
    /// </summary>
    /// <param name="product">Lista de productos a ordenar.</param>
    /// <param name="sortBy">Campo de ordenamiento (name, price, stock).</param>
    /// <param name="direction">Dirección del ordenamiento (asc, desc).</param>
    /// <returns>Lista ordenada de productos.</returns>
    /// <remarks>
    /// <para><b>Campos soportados:</b></para>
    /// <list type="bullet">
    ///     <item>"name" - Ordenar por nombre</item>
    ///     <item>"price" - Ordenar por precio</item>
    ///     <item>"stock" - Ordenar por stock</item>
    ///     <item>otro - Ordenar por ID (defecto)</item>
    /// </list>
    /// </remarks>
    private List<ProductResponseDto> ApplySorting(List<ProductResponseDto> product, string? sortBy, string? direction)
    {
        bool desc = direction?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "name" => desc 
                ? product.OrderByDescending(p => p.Name).ToList()
                : product.OrderBy(p => p.Name).ToList(),

            "price" => desc
                ? product.OrderByDescending(p => p.Price).ToList()
                : product.OrderBy(p => p.Price).ToList(),

            "stock" => desc
                ? product.OrderByDescending(p => p.Stock).ToList()
                : product.OrderBy(p => p.Stock).ToList(),

            _ => product.OrderBy(p => p.Id).ToList() 
        };
    }
}
