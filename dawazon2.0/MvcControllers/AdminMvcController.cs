using dawazon2._0.Models;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace dawazon2._0.MvcControllers;

/// <summary>
/// Controlador MVC para las funcionalidades del panel de administración.
/// Solo accesible por usuarios con rol Admin.
/// </summary>
[Route("admin")]
[Authorize(Roles = UserRoles.ADMIN)]
public class AdminMvcController(IUserService userService) : Controller
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
        };

        ViewBag.UserId = id;
        return View(vm);
    }

    /// <summary>Guarda los cambios de edición de un usuario.</summary>
    [HttpPost("usuarios/{id}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserEdit(string id, [FromForm] UserEditViewModel vm)
    {
        Log.Information("[AdminMvc] UserEdit POST → id={Id}", id);

        ViewBag.UserId = id;

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

        var result = await userService.UpdateByIdAsync(long.Parse(id), dto);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(vm);
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
}
