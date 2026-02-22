using System.Security.Claims;
using dawazon2._0.Mapper;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Service;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace dawazon2._0.MvcControllers;

/// <summary>
/// Controlador MVC para la sección "Mis Pedidos" del usuario.
/// Muestra los carritos ya comprados (Purchased = true) del usuario autenticado.
/// </summary>
[Route("pedidos")]
[Authorize(Roles = UserRoles.USER)]
public class CartMvcController(ICartService cartService) : Controller
{
    /// <summary>Vista de listado de pedidos del usuario logueado.</summary>
    [HttpGet("")]
    [HttpGet("lista")]
    public async Task<IActionResult> MyOrders(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10)
    {
        var userId = GetUserId();
        Log.Information("[CartMvc] MyOrders → userId={UserId} page={Page}", userId, page);

        var filter = new FilterCartDto(null, null, true, page, size);
        var result = await cartService.FindAllAsync(userId, purchased: true, filter);

        var vm = result.Content.ToOrderListViewModel(
            pageNumber:    result.PageNumber,
            totalPages:    result.TotalPages,
            totalElements: result.TotalElements);

        return View(vm);
    }

    /// <summary>Vista de detalle de un pedido concreto.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(string id)
    {
        var userId = GetUserId();
        Log.Information("[CartMvc] Detail → userId={UserId} cartId={CartId}", userId, id);

        var result = await cartService.GetByIdAsync(id);

        if (result.IsFailure)
        {
            Log.Warning("[CartMvc] Pedido {CartId} no encontrado: {Error}", id, result.Error.Message);
            return NotFound();
        }

        // Verificar que el pedido pertenece al usuario autenticado
        if (result.Value.UserId != userId)
        {
            Log.Warning("[CartMvc] Acceso denegado: userId={UserId} intentó ver el pedido {CartId} de userId={OwnerId}",
                userId, id, result.Value.UserId);
            return Forbid();
        }

        // Solo mostrar pedidos ya comprados
        if (!result.Value.Purchased)
        {
            Log.Warning("[CartMvc] El carrito {CartId} no está comprado aún", id);
            return NotFound();
        }

        var vm = result.Value.ToOrderDetailViewModel();
        return View(vm);
    }

    private long GetUserId() =>
        long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
