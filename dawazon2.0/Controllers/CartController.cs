using System.Security.Claims;
using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Errors;
using dawazonBackend.Cart.Service;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Errors;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace dawazon2._0.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CartController (
    ICartService service, 
    ILogger<CartController> logger) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SaleLineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.ADMIN)]

    public async Task<IActionResult> GetSaleLinesAsync(
        [FromQuery] string sortBy = "id",
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string direction = "asc")
    {
        logger.LogInformation("Obteniendo todas las líneas de venta");
        var filter = new FilterDto(null, null,page, size, sortBy,direction);
        return Ok(await service.FindAllSalesAsLinesAsync(null, true, filter));
        
    }
    
    [HttpGet("purchased")]
    [ProducesResponseType(typeof(IEnumerable<CartResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.USER)]
    public async Task<IActionResult> GetPurchasedCartsAsync(
        [FromQuery] string sortBy = "id",
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string direction = "asc")
    {
        logger.LogInformation("Obteniendo todas las líneas de venta compradas");
        var filter = new FilterCartDto(null, null,true, page, size, sortBy, direction);
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await service.FindAllAsync(long.Parse(userId!), true, filter));
    }
    
    [HttpGet("notPurchased")]
    [ProducesResponseType(typeof(IEnumerable<CartResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.USER)]
    public async Task<IActionResult> GetNotPurchasedCartsAsync(
        [FromQuery] string sortBy = "id",
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string direction = "asc")
    {
        logger.LogInformation("Obteniendo todas las líneas de venta no compradas");
        var filter = new FilterCartDto(null, null,false, page, size, sortBy, direction);
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await service.FindAllAsync(long.Parse(userId!), false, filter));
    }

    [HttpPost("addToCart/{cartId}/{productId}")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.USER)]
    public async Task<IActionResult> AddProcuctAsync(
        string cartId,
        string productId
    )
    {
        logger.LogInformation("Añadiendo producto al carrito");
        return Ok(await service.AddProductAsync(cartId, productId));
    }
    
    [HttpPost("removeFromCart/{cartId}/{productId}")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.USER)]
    public async Task<IActionResult> RemoveProcuctAsync(
        string cartId,
        string productId
    )
    {
        logger.LogInformation("Eliminando producto del carrito");
        return Ok(await service.RemoveProductAsync(cartId, productId));
    }

    [HttpDelete("{cartId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.ADMIN)]
    public async Task<IActionResult> RemoveCartAsync(
        string cartId
    )
    {
        await service.DeleteByIdAsync(cartId);
        return NoContent();
    }

    [HttpGet("cart")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.USER)]

    public async Task<IActionResult> GetCartByUserIdAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await service.GetCartByUserIdAsync(long.Parse(userId!)).Match(
            onSuccess: cart => Ok(cart),
            onFailure: error => error switch
            {
                CartNotFoundError => NotFound(new { message = error.Message }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = error.Message })
            });
    }

    [HttpPut("cancel/{cartId}/{productId}")]
    [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = UserRoles.ADMIN + "," + UserRoles.MANAGER)]
    public async Task<IActionResult> CancelSaleLineAsync(string cartId, string productId)
    {
        string? userId = null;
        if (User.IsInRole(UserRoles.MANAGER))
        {
            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        long? longId = userId is { } ? long.Parse(userId) : null;
        
        return await service.CancelSaleAsync(cartId, productId, longId, User.IsInRole(UserRoles.ADMIN)) is {} error
            ? error switch
            {
                CartNotFoundError or ProductNotFoundError => NotFound(new {message = error.Message}),
                CartUnauthorizedError => Unauthorized(new {message = error.Message}),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = error.Message })
            }
            : NoContent();
        
    }
    
}