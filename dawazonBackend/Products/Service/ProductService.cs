using CSharpFunctionalExtensions;
using dawazonBackend.Common.Cache;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Storage;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Mapper;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Products.Repository.Categoria;
using dawazonBackend.Products.Repository.Productos;

namespace dawazonBackend.Products.Service;

/// <summary>
/// Implementación del servicio de gestión de productos con soporte para caché y almacenamiento de imágenes.
/// </summary>
public class ProductService(
    ICacheService cache,
    IProductRepository repository,
    ICategoriaRepository categoryRepository,
    IStorage storageService,
    ILogger<ProductService> logger)
    : IProductService
{
    private const string CacheKeyPrefix = "Product_";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);


    /// <inheritdoc/>
    public async Task<Result<ProductResponseDto, ProductError>> GetByIdAsync(string id)
    {
        logger.LogDebug("Buscando Product con id: {Id}", id);
        var cacheKey = CacheKeyPrefix + id;

        if (await cache.GetAsync<Product>(cacheKey) is { }  cachedProduct)
        {
                logger.LogDebug("Product con id {Id} encontrado en caché", id);
                return cachedProduct.ToDto();
        }

        var product = await repository.GetProductAsync(id);
        if (product == null)
        {
            logger.LogWarning("Product con id {Id} no encontrado en la base de datos", id);
            return Result.Failure<ProductResponseDto, ProductError>(
                new ProductNotFoundError($"No se encontró el Product con id: {id}."));
        }

        await cache.SetAsync(cacheKey, product, _cacheDuration);
        return product.ToDto();
    }

    /// <inheritdoc/>
    public async Task<Result<long, ProductError>> GetUserProductIdAsync(string id)
    {
        logger.LogDebug("Obteniendo ID del creador para Product con id: {Id}", id);

        var product = await repository.GetProductAsync(id);
    
        if (product == null)
        {
            return Result.Failure<long, ProductError>(
                new ProductNotFoundError($"No se encontró el Product con id: {id}."));
        }

        return Result.Success<long, ProductError>(product.CreatorId); 
    }

    /// <inheritdoc/>
    public async Task<PageResponseDto<ProductResponseDto>> GetAllAsync(FilterDto filter)
    {
        logger.LogDebug("Obteniendo listado de Products con filtros");

        var (products, totalCount) = await repository.GetAllAsync(filter);

        var response = products.Select(it => it.ToDto()).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.Size);

        var page = new PageResponseDto<ProductResponseDto>(
            Content: response,
            TotalPages: totalPages,
            TotalElements: totalCount,
            PageSize: filter.Size,
            PageNumber: filter.Page,
            TotalPageElements: response.Count,
            SortBy: filter.SortBy,
            Direction: filter.Direction
        );

        return page;
    }
    
    /// <inheritdoc/>
    public async Task<List<string>> GetAllCategoriesAsync()
    {
        logger.LogDebug("Obteniendo todas las categorías");
        return await categoryRepository.GetCategoriesAsync(); 
    }
    
    /// <inheritdoc/>
    public async Task<Result<ProductResponseDto, ProductError>> CreateAsync(ProductRequestDto dto)
    {
        logger.LogInformation("Creando nuevo Product: {Name}", dto.Name);

        var foundCategory = await categoryRepository.GetByNameAsync(dto.Category);
        if (foundCategory == null)
        {
            return Result.Failure<ProductResponseDto, ProductError>(
                new ProductConflictError($"La categoría: {dto.Category} no existe."));
        }

        var productModel = dto.ToModel();
        productModel.CategoryId = foundCategory.Id;
        productModel.Category = foundCategory;

        var savedProduct = await repository.CreateProductAsync(productModel);
        
        return savedProduct!.ToDto();
    }

    /// <inheritdoc/>
    public async Task<Result<ProductResponseDto, ProductError>> UpdateAsync(string id, ProductRequestDto dto)
    {
        logger.LogInformation("Actualizando Product con id: {Id}", id);

        var foundCategory = await categoryRepository.GetByNameAsync(dto.Category);
        if (foundCategory == null)
        {
            return Result.Failure<ProductResponseDto, ProductError>(
                new ProductConflictError($"La categoría: {dto.Category} no existe."));
        }

        var productToUpdate = dto.ToModel();
        productToUpdate.Id = id;
        productToUpdate.CategoryId = foundCategory.Id;
        productToUpdate.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await repository.UpdateProductAsync(productToUpdate, id);

        if (updatedProduct == null)
        {
            return Result.Failure<ProductResponseDto, ProductError>(
                new ProductNotFoundError($"No se encontró el Product con id: {id}."));
        }

        await cache.RemoveAsync(CacheKeyPrefix + id);
        return updatedProduct.ToDto();
    }

    /// <inheritdoc/>
    public async Task<Result<ProductResponseDto, ProductError>> PatchAsync(string id, ProductPatchRequestDto dto)
    {
        // Mantenemos tu Patch original
        logger.LogInformation("Aplicando PATCH a Product id: {Id}", id);
        var foundProduct = await repository.GetProductAsync(id);
        
        if (foundProduct == null) 
            return Result.Failure<ProductResponseDto, ProductError>(new ProductNotFoundError($"Product {id} no encontrado"));

        if (dto.Name != null) foundProduct.Name = dto.Name;
        if (dto.Price != null) foundProduct.Price = (double)dto.Price;
        if (dto.Images != null) foundProduct.Images = dto.Images;
        
        if (dto.Category != null)
        {
            var foundCategory = await categoryRepository.GetByNameAsync(dto.Category);
            if (foundCategory == null)
                return Result.Failure<ProductResponseDto, ProductError>(new ProductConflictError($"Categ {dto.Category} no existe"));
            
            foundProduct.Category = foundCategory;
            foundProduct.CategoryId = foundCategory.Id;
        }

        await repository.UpdateProductAsync(foundProduct, id);
        await cache.RemoveAsync(CacheKeyPrefix + id);
        return foundProduct.ToDto();
    }

    /// <inheritdoc/>
    public async Task<Result<ProductResponseDto, ProductError>> UpdateImageAsync(string id, List<IFormFile> images)
    {
        logger.LogInformation("SERVICE: Actualizando imagen de producto por id: {Id}", id);

        var foundProduct = await repository.GetProductAsync(id);
        if (foundProduct == null)
        {
            return Result.Failure<ProductResponseDto, ProductError>(
                new ProductNotFoundError($"Producto no encontrado con id: {id}"));
        }

        // Filtrar archivos vacíos
        var validImages = images.Where(file => file is { Length: > 0 }).ToList();

        // 3. Si no hay archivos válidos, retornar sin cambios
        if (validImages.Count == 0)
        {
            logger.LogInformation("SERVICE: No se subieron imágenes nuevas, manteniendo las existentes");
            return foundProduct.ToDto();
        }

        // Eliminar imágenes antiguas del almacenamiento
        if (foundProduct.Images.Count > 0)
        {
            foreach (var imagePath in foundProduct.Images)
            {
                await storageService.DeleteFileAsync(imagePath);
            }
            foundProduct.Images.Clear();
        }

        // Guardar las nuevas imágenes
        var savedPaths = new List<string>();
        foreach (var file in validImages)
        {
            var result = await storageService.SaveFileAsync(file, "products");
            
            if (result.IsSuccess) savedPaths.Add(result.Value); // Value contiene la ruta relativa o nombre guardado
            
            else logger.LogWarning("Error guardando una imagen: {Error}", result.Error);
        }

        // Actualizar el modelo con las nuevas rutas
        foundProduct.Images = savedPaths;

        // Persistir cambios en BD
        var updatedProduct = await repository.UpdateProductAsync(foundProduct, id);

        if (updatedProduct == null)
        {
            return Result.Failure<ProductResponseDto, ProductError>(
                new ProductNotFoundError($"Error al actualizar producto {id} tras guardar imágenes"));
        }

        // Devolver DTO actualizado
        return updatedProduct.ToDto();
    }

    /// <inheritdoc/>
    public async Task<Result<ProductResponseDto, ProductError>> AddCommentAsync(string id, Comment comment)
    {
        logger.LogInformation("Agregando comentario a Product id: {Id}", id);

        var foundProduct = await repository.GetProductAsync(id);
        if (foundProduct == null)
        {
            return Result.Failure<ProductResponseDto, ProductError>(
                new ProductNotFoundError($"No se encontró el Product con id: {id}."));
        }

        foundProduct.Comments.Add(comment);

        await repository.UpdateProductAsync(foundProduct, id);

        await cache.RemoveAsync(CacheKeyPrefix + id);
        return foundProduct.ToDto();
    }

    /// <inheritdoc/>
    public async Task<Result<ProductResponseDto, ProductError>> DeleteAsync(string id)
    {
        logger.LogInformation("Eliminando Product con id: {Id}", id);

        // BUSCAR ANTES DE BORRAR
        var productToDelete = await repository.GetProductAsync(id);

        if (productToDelete == null)
        {
            return Result.Failure<ProductResponseDto, ProductError>(
                new ProductNotFoundError($"No se encontró el Product con id: {id}."));
        }

        // BORRAR EN BD
        try
        {
            await repository.DeleteByIdAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError("Error eliminando producto en BD: {Message}", ex.Message);
            return Result.Failure<ProductResponseDto, ProductError>(
                new ProductNotFoundError($"Error interno al eliminar: {ex.Message}"));
        }
    
        // BORRAR IMÁGENES
        if (productToDelete.Images.Count > 0)
        {
            foreach (var imagePath in productToDelete.Images)
            {
                await storageService.DeleteFileAsync(imagePath);
            }
        }

        await cache.RemoveAsync(CacheKeyPrefix + id);
    
        return productToDelete.ToDto();
    }
}