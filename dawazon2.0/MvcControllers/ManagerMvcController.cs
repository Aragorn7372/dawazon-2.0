using System.Security.Claims;
using dawazon2._0.Models;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Models;
using dawazonBackend.Cart.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace dawazon2._0.MvcControllers;

/// <summary>
/// Controlador MVC para las funcionalidades del panel de manager.
/// Solo accesible por usuarios con rol Manager.
/// </summary>
[Route("manager")]
[Authorize(Roles = UserRoles.MANAGER)]
public class ManagerMvcController(ICartService cartService) : Controller
{

    /// <summary>Lista paginada de todas las ventas que incluyen productos del manager.</summary>
    [HttpGet("ventas")]
    public async Task<IActionResult> Sales([FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        Log.Information("[ManagerMvc] Sales → page={Page} size={Size}", page, size);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var managerId))
        {
            return Forbid();
        }

        var filter = new FilterDto(null, null, page, size, "createAt", "desc");
        var result = await cartService.FindAllSalesAsLinesAsync(managerId, false, filter);
        var totalEarnings = await cartService.CalculateTotalEarningsAsync(managerId, false);

        var vm = new AdminSaleListViewModel
        {
            Sales         = result.Content,
            PageNumber    = result.PageNumber,
            TotalPages    = result.TotalPages,
            TotalElements = result.TotalElements,
            PageSize      = result.PageSize,
            TotalEarnings = totalEarnings
        };

        return View(vm);
    }

    /// <summary>Formulario de edición de estado de una venta del manager.</summary>
    [HttpGet("ventas/{id}/editar/{productId}")]
    public async Task<IActionResult> SaleEdit(string id, string productId)
    {
        Log.Information("[ManagerMvc] SaleEdit GET → id={Id}, productId={ProductId}", id, productId);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var managerId))
        {
            return Forbid();
        }

        var cartResult = await cartService.GetByIdAsync(id);
        if (cartResult.IsFailure) return NotFound();

        var cart = cartResult.Value;
        var line = cart.CartLines.FirstOrDefault(l => l.ProductId == productId);
        if (line == null) return NotFound();

        var vm = new AdminSaleEditViewModel
        {
            SaleId = id,
            ProductId = productId,
            ProductName = line.ProductName,
            ClientName = cart.Client.Name,
            Quantity = line.Quantity,
            TotalPrice = line.TotalPrice,
            CurrentStatus = line.Status,
            NewStatus = line.Status
        };

        return View(vm);
    }

    /// <summary>Guarda los cambios de estado de una venta del manager.</summary>
    [HttpPost("ventas/{id}/editar/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaleEdit(string id, string productId, [FromForm] AdminSaleEditViewModel vm)
    {
        Log.Information("[ManagerMvc] SaleEdit POST → id={Id}, productId={ProductId}, status={Status}", id, productId, vm.NewStatus);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var managerId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await cartService.UpdateSaleStatusAsync(id, productId, vm.NewStatus, managerId, false);
        if (result != null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(vm);
        }

        TempData["Success"] = "Estado modificado correctamente.";
        return RedirectToAction(nameof(Sales));
    }

    /// <summary>Cancela una venta del manager desde el listado.</summary>
    [HttpPost("ventas/{id}/cancelar/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaleCancel(string id, string productId)
    {
        Log.Warning("[ManagerMvc] SaleCancel → id={Id}, productId={ProductId}", id, productId);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var managerId))
        {
            return Forbid();
        }

        var result = await cartService.CancelSaleAsync(id, productId, managerId, false);
        if (result != null)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = "Cancelaste la venta de tu producto con éxito.";
        }

        return RedirectToAction(nameof(Sales));
    }
}
