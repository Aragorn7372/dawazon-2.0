using System.Security.Claims;
using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Mapper;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Products.Service;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dawazon2._0.RestControllers;

/// <summary>
/// Controlador API REST para la gestión de productos.
/// </summary>
/// <remarks>
/// Maneja todas las operaciones CRUD (Crear, Leer, Actualizar, Eliminar) para productos.
/// Soporta operaciones con archivos mediante multipart/form-data para imágenes.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>IProductService: Lógica de negocio de productos</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>CRUD completo de productos</item>
///     <item>Gestión de imágenes (multipart/form-data)</item>
///     <item>Paginación y filtros</item>
///     <item>Autorización: Solo Managers pueden crear/editar</item>
/// </list>
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController(IProductService service) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los productos con paginación y filtros.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Recibe filtros: nombre, categoría, sortBy, page, size, direction</item>
    ///     <item>Retorna PageResponseDto con lista de productos</item>
    /// </list>
    /// </remarks>
    /// <param name="nombre">Filtro opcional por nombre.</param>
    /// <param name="categoria">Filtro opcional por categoría.</param>
    /// <param name="sortBy">Campo de ordenación (default: id).</param>
    /// <param name="page">Número de página (default: 0).</param>
    /// <param name="size">Tamaño de página (default: 10).</param>
    /// <param name="direction">Dirección de ordenación (default: asc).</param>
    /// <response code="200">Lista de productos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PageResponseDto<ProductResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] string? nombre,
        [FromQuery] string? categoria,
        [FromQuery] string sortBy = "id",
        [FromQuery] int page = 0, 
        [FromQuery] int size = 10,
        [FromQuery] string direction = "asc" )
    {
        var filter= new FilterDto(nombre, categoria,page, size, sortBy,direction);
        return Ok(await service.GetAllAsync(filter));
    }
    
    /// <summary>
    /// Obtiene un producto específico por su ID.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Busca el producto por ID</item>
    ///     <item>Retorna 404 si no existe</item>
    /// </list>
    /// </remarks>
    /// <param name="id">ID del producto.</param>
    /// <response code="200">Producto encontrado.</response>
    /// <response code="404">Producto no encontrado.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAsync(string id)
    {
        return await service.GetByIdAsync(id).Match(
            onSuccess: IActionResult(response) => Ok(response),
            onFailure: error => error switch
            {
                ProductNotFoundError => NotFound(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            });
    }

    /// <summary>
    /// Crea un nuevo producto con imagen opcional.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Requiere rol MANAGER</item>
    ///     <item>Acepta multipart/form-data</item>
    ///     <item>Asigna CreatorId desde el token JWT</item>
    /// </list>
    /// </remarks>
    /// <param name="request">Datos del producto.</param>
    /// <param name="file">Archivos de imagen (opcional).</param>
    /// <response code="201">Producto creado.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="409">Conflicto (categoría no existe).</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = UserRoles.MANAGER)]
    public async Task<IActionResult> PostAsync(
        [FromForm] ProductRequestDto request,
        [FromForm] List<IFormFile> file)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var requestUserId= request.Copy(CreatorId:long.Parse(userId!));
        var saved = await service.CreateAsync(requestUserId);
        if (saved.IsSuccess)
            return await service.UpdateImageAsync(saved.Value.Id, file).Match(
                onSuccess: IActionResult (response) => Created($"/api/Products/{response.Id}", response),
                onFailure: error => error switch {
                    ProductNotFoundError=> NotFound(new {message= error.Message}),
                    _ => BadRequest( new { message = error.Message })
                    
                }
            );
        return Conflict(new { message =saved.Error.Message });
    }

    /// <summary>
    /// Actualiza un producto existente, incluyendo opcionalmente sus imágenes.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Requiere rol MANAGER</item>
    ///     <item>Solo el creador puede editar</item>
    ///     <item>Acepta multipart/form-data</item>
    /// </list>
    /// </remarks>
    /// <param name="id">ID del producto a actualizar.</param>
    /// <param name="request">Datos del producto.</param>
    /// <param name="files">Archivos de imagen (opcional).</param>
    /// <response code="200">Producto actualizado.</response>
    /// <response code="403">Forbidden (no eres el creador).</response>
    /// <response code="404">Producto no encontrado.</response>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.MANAGER)]
    public async Task<IActionResult> PutAsync(
        string id,
        [FromForm] ProductRequestDto request,
        [FromForm] List<IFormFile> files)
    {
        var creatorId = await service.GetUserProductIdAsync(id);
        if (creatorId.IsFailure)
        {
            return NotFound(new {error = creatorId.Error.Message});
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (creatorId.Value==long.Parse(userId!))
        {
            var images = await service.UpdateImageAsync(id, files);
            if (images.IsFailure)
            {
                return images.Error switch
                {
                    ProductNotFoundError => NotFound(new { message = images.Error.Message }),
                    _ => BadRequest(new { message = images.Error.Message })
                };
            }

            var copied = request.Copy(Images: images.Value.Images);
            return await service.UpdateAsync(id, copied).Match(
                onSuccess: response => Ok(response),
                onFailure: error => error switch
                {
                    ProductValidationError => BadRequest(new { message = error.Message }),
                    ProductNotFoundError => NotFound(new { message = error.Message }),
                    _ => StatusCode(500, new { message = error.Message })
                });
        }

        return Forbid();
    }

    /// <summary>
    /// Elimina un producto existente del sistema.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Admin puede eliminar cualquier producto</item>
    ///     <item>Manager solo puede eliminar sus propios productos</item>
    ///     <item>Elimina también las imágenes asociadas</item>
    /// </list>
    /// </remarks>
    /// <param name="id">ID del producto a eliminar.</param>
    /// <response code="200">Producto eliminado.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Producto no encontrado.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.ADMIN + "," + UserRoles.MANAGER)]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        if (User.IsInRole(UserRoles.MANAGER))
        {
            var creatorId = await service.GetUserProductIdAsync(id);
            if (creatorId.IsFailure) return NotFound(new {error = creatorId.Error.Message});
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (creatorId.Value != long.Parse(userId!)) return Forbid();
        }
        return await service.DeleteAsync(id).Match(
            onSuccess: response => Ok(response),
            onFailure: error => error switch
            {
                ProductNotFoundError => NotFound(new { message = error.Message }),
                _ => StatusCode(500, new { message = error.Message })
            });
    }
}