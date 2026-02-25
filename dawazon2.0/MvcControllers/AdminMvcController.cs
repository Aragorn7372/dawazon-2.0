using dawazon2._0.Models;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using dawazonBackend.Cart.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

using Microsoft.AspNetCore.Identity;

namespace dawazon2._0.MvcControllers;

/// <summary>
/// Controlador MVC para las funcionalidades del panel de administración.
/// Solo accesible por usuarios con rol Admin.
/// </summary>
[Route("admin")]
[Authorize(Roles = UserRoles.ADMIN)]
public class AdminMvcController(IUserService userService, ICartService cartService, UserManager<User> userManager) : Controller
{
    // ─── USUARIOS ───────────────────────────────────────────────────────────

    /// <summary>Lista paginada de todos los usuarios activos.</summary>
    [HttpGet("usuarios")]
    public async Task<IActionResult> Users([FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        Log.Information("[AdminMvc] Users → page={Page} size={Size}", page, size);

        var filter = new FilterDto(null, null, page, size, "id", "asc");
        var result = await userService.GetAllAsync(filter);

        var vm = new AdminUserListViewModel
        {
            Users         = result.Content,
            PageNumber    = result.PageNumber,
            TotalPages    = result.TotalPages,
            TotalElements = result.TotalElements,
            PageSize      = result.PageSize
        };

        return View(vm);
    }

    /// <summary>Detalle de un usuario concreto.</summary>
    [HttpGet("usuarios/{id}")]
    public async Task<IActionResult> UserDetail(string id)
    {
        Log.Information("[AdminMvc] UserDetail → id={Id}", id);

        var result = await userService.GetByIdAsync(id);
        if (result.IsFailure)
            return NotFound();

        var vm = new AdminUserDetailViewModel { User = result.Value };
        return View(vm);
    }

    /// <summary>Formulario de edición de un usuario.</summary>
    [HttpGet("usuarios/{id}/editar")]
    public async Task<IActionResult> UserEdit(string id)
    {
        Log.Information("[AdminMvc] UserEdit GET → id={Id}", id);

        var result = await userService.GetByIdAsync(id);
        if (result.IsFailure)
            return NotFound();

        var dto = result.Value;
        var vm = new UserEditViewModel
        {
            Nombre       = dto.Nombre,
            Email        = dto.Email,
            Telefono     = dto.Telefono,
            Calle        = dto.Calle        ?? string.Empty,
            Ciudad       = dto.Ciudad       ?? string.Empty,
            CodigoPostal = dto.CodigoPostal ?? string.Empty,
            Provincia    = dto.Provincia    ?? string.Empty,
            Rol          = dto.Roles.FirstOrDefault() // Asumimos un solo rol principal
        };

        ViewBag.UserId = id;
        ViewBag.CurrentUserId = userManager.GetUserId(User);
        ViewBag.Roles = new List<string> { UserRoles.USER, UserRoles.MANAGER, UserRoles.ADMIN };
        
        return View(vm);
    }

    /// <summary>Guarda los cambios de edición de un usuario.</summary>
    [HttpPost("usuarios/{id}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserEdit(string id, [FromForm] UserEditViewModel vm)
    {
        Log.Information("[AdminMvc] UserEdit POST → id={Id}", id);

        var currentUserId = userManager.GetUserId(User);
        ViewBag.UserId = id;
        ViewBag.CurrentUserId = currentUserId;
        ViewBag.Roles = new List<string> { UserRoles.USER, UserRoles.MANAGER, UserRoles.ADMIN };

        if (!ModelState.IsValid)
            return View(vm);

        var dto = new UserRequestDto
        {
            Nombre       = vm.Nombre,
            Email        = vm.Email,
            Telefono     = vm.Telefono,
            Calle        = vm.Calle,
            Ciudad       = vm.Ciudad,
            CodigoPostal = vm.CodigoPostal,
            Provincia    = vm.Provincia
        };

        var result = await userService.UpdateByIdAsync(long.Parse(id), dto, null); // Null porque admin no va a cambiar la imagen
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(vm);
        }

        // Actualizar rol si se ha especificado, es válido, y no es el usuario actual modificándose a sí mismo
        if (!string.IsNullOrEmpty(vm.Rol) && ViewBag.Roles.Contains(vm.Rol) && id != currentUserId)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                var currentRoles = await userManager.GetRolesAsync(user);
                if (vm.Rol != null && !currentRoles.Contains(vm.Rol))
                {
                    // Remover roles anteriores y añadir el nuevo
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                    await userManager.AddToRoleAsync(user, vm.Rol);
                }
            }
        }

        TempData["Success"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Users));
    }

    /// <summary>Soft-delete de un usuario (borrado lógico).</summary>
    [HttpPost("usuarios/{id}/eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserDelete(string id)
    {
        Log.Warning("[AdminMvc] UserDelete → id={Id}", id);

        await userService.BanUserById(id);

        TempData["Success"] = "Usuario desactivado correctamente.";
        return RedirectToAction(nameof(Users));
    }

    // ─── VENTAS ─────────────────────────────────────────────────────────────

    /// <summary>Lista paginada de todas las ventas.</summary>
    [HttpGet("ventas")]
    public async Task<IActionResult> Sales([FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        Log.Information("[AdminMvc] Sales → page={Page} size={Size}", page, size);

        var filter = new FilterDto(null, null, page, size, "createAt", "desc");
        var result = await cartService.FindAllSalesAsLinesAsync(null, true, filter);
        var totalEarnings = await cartService.CalculateTotalEarningsAsync(null, true);

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

    /// <summary>Formulario de edición de estado de una venta.</summary>
    [HttpGet("ventas/{id}/editar/{productId}")]
    public async Task<IActionResult> SaleEdit(string id, string productId)
    {
        Log.Information("[AdminMvc] SaleEdit GET → id={Id}, productId={ProductId}", id, productId);

        // Fetch the sale line via find all since we don't have a single line fetch by id easily, 
        // or we can fetch the cart and find the line.
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

    /// <summary>Guarda los cambios de estado de una venta.</summary>
    [HttpPost("ventas/{id}/editar/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaleEdit(string id, string productId, [FromForm] AdminSaleEditViewModel vm)
    {
        Log.Information("[AdminMvc] SaleEdit POST → id={Id}, productId={ProductId}, status={Status}", id, productId, vm.NewStatus);

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await cartService.UpdateSaleStatusAsync(id, productId, vm.NewStatus, null, true);
        if (result != null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(vm);
        }

        TempData["Success"] = "Estado de la venta actualizado correctamente.";
        return RedirectToAction(nameof(Sales));
    }

    /// <summary>Cancela una venta desde el listado.</summary>
    [HttpPost("ventas/{id}/cancelar/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaleCancel(string id, string productId)
    {
        Log.Warning("[AdminMvc] SaleCancel → id={Id}, productId={ProductId}", id, productId);

        var result = await cartService.CancelSaleAsync(id, productId, null, true);
        if (result != null)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = "Venta cancelada correctamente.";
        }

        return RedirectToAction(nameof(Sales));
    }
}
