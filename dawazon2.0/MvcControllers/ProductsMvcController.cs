using System.Security.Claims;
using dawazon2._0.Mapper;
using dawazon2._0.Models;
using dawazonBackend.Cart.Service;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Service;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace dawazon2._0.MvcControllers;

/// <summary>
/// Controlador MVC para la gestión de productos con vistas Razor.
/// Equivalente MVC del <see cref="RestControllers.ProductsController"/> REST.
/// </summary>
[Route("")]
[Route("productos")]
public class ProductsMvcController(IProductService service, UserManager<User> userManager, ICartService cartService) : Controller
{

    /// <summary>Lista paginada y filtrable de productos.</summary>
    [HttpGet("")]
    [HttpGet("lista")]
    public async Task<IActionResult> Index(
        [FromQuery] string? nombre,
        [FromQuery] string? categoria,
        [FromQuery] string sortBy = "id",
        [FromQuery] int page = 0,
        [FromQuery] int size = 12,
        [FromQuery] string direction = "asc",
        [FromQuery] bool misProductos = false)
    {
        var filter = new FilterDto(nombre, categoria, page, size, sortBy, direction);
        
        long? creatorId = null;
        if (misProductos && User.IsInRole(UserRoles.MANAGER))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && long.TryParse(userIdClaim, out var userId))
            {
                creatorId = userId;
            }
        }
        
        var result = await service.GetAllAsync(filter, creatorId);
        var vm = result.ToListViewModel(nombre, categoria, sortBy, direction);

        // Variables adicionales para la vista
        ViewBag.MisProductos = misProductos;
        
        // Categorías para el navbar
        await SetCategoriasBagAsync();

        return View(vm);
    }


    /// <summary>Vista de detalle de un producto concreto.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(string id)
    {
        var result = await service.GetByIdAsync(id);
        if (result.IsFailure)
            return result.Error is ProductNotFoundError
                ? NotFound()
                : StatusCode(500);

        if (User.IsInRole(UserRoles.USER))
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null && long.TryParse(userId, out long userIdLong))
            {
                var user = await userManager.FindByIdAsync(userId);
                ViewBag.IsFav = user?.ProductsFavs.Contains(id) ?? false;

                // Comprobar si el producto ya está en el carrito activo
                var cartResult = await cartService.GetCartByUserIdAsync(userIdLong);
                if (cartResult.IsSuccess)
                {
                    ViewBag.CartId = cartResult.Value.Id;
                    ViewBag.IsInCart = cartResult.Value.CartLines
                        .Any(l => l.ProductId == id);
                }
                else
                {
                    ViewBag.IsInCart = false;
                }
            }
        }

        return View(result.Value.ToDetailViewModel());
    }

    /// <summary>Formulario de creación de producto (solo Manager).</summary>
    [HttpGet("crear")]
    [Authorize(Roles = UserRoles.MANAGER)]
    public async Task<IActionResult> Create()
    {
        var vm = new ProductFormViewModel
        {
            AvailableCategories = await GetCategorySelectListAsync()
        };
        return View("Form", vm);
    }

    /// <summary>Procesa el formulario de creación de producto.</summary>
    [HttpPost("crear")]
    [Authorize(Roles = UserRoles.MANAGER)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] ProductFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AvailableCategories = await GetCategorySelectListAsync();
            return View("Form", vm);
        }

        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var dto = vm.ToRequestDto(userId);
        var created = await service.CreateAsync(dto);

        if (created.IsFailure)
        {
            ModelState.AddModelError(string.Empty, created.Error.Message);
            vm.AvailableCategories = await GetCategorySelectListAsync();
            return View("Form", vm);
        }

        // Si se adjuntaron imágenes, subirlas
        if (vm.Images is { Count: > 0 })
        {
            await service.UpdateImageAsync(created.Value.Id, vm.Images);
        }

        TempData["Success"] = $"Producto \"{created.Value.Name}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }


    /// <summary>Formulario de edición de un producto existente (solo su creador Manager).</summary>
    [HttpGet("editar/{id}")]
    [Authorize(Roles = UserRoles.MANAGER)]
    public async Task<IActionResult> Edit(string id)
    {
        var result = await service.GetByIdAsync(id);
        if (result.IsFailure)
            return result.Error is ProductNotFoundError ? NotFound() : StatusCode(500);

        // Solo el creador puede editar
        var creatorId = await service.GetUserProductIdAsync(id);
        if (creatorId.IsFailure) return NotFound();

        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (creatorId.Value != userId) return Forbid();

        var vm = result.Value.ToFormViewModel(await GetCategorySelectListAsync());
        return View("Form", vm);
    }


    /// <summary>Procesa el formulario de edición de un producto.</summary>
    [HttpPost("editar/{id}")]
    [Authorize(Roles = UserRoles.MANAGER)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [FromForm] ProductFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AvailableCategories = await GetCategorySelectListAsync();
            return View("Form", vm);
        }

        // Verificar autoría
        var creatorId = await service.GetUserProductIdAsync(id);
        if (creatorId.IsFailure) return NotFound();

        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (creatorId.Value != userId) return Forbid();

        // Actualizar imágenes si se adjuntaron nuevas
        if (vm.Images is { Count: > 0 })
        {
            var imgResult = await service.UpdateImageAsync(id, vm.Images);
            if (imgResult.IsSuccess)
                vm.CurrentImages = imgResult.Value.Images;
        }

        var dto = vm.ToRequestDto(userId);
        var updated = await service.UpdateAsync(id, dto);

        if (updated.IsFailure)
        {
            ModelState.AddModelError(string.Empty, updated.Error.Message);
            vm.AvailableCategories = await GetCategorySelectListAsync();
            return View("Form", vm);
        }

        TempData["Success"] = $"Producto \"{updated.Value.Name}\" actualizado correctamente.";
        return RedirectToAction(nameof(Detail), new { id });
    }


    /// <summary>Elimina un producto (Admin o el Manager creador).</summary>
    [HttpPost("eliminar/{id}")]
    [Authorize(Roles = UserRoles.ADMIN + "," + UserRoles.MANAGER)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        // Manager solo puede borrar sus propios productos
        if (User.IsInRole(UserRoles.MANAGER))
        {
            var creatorId = await service.GetUserProductIdAsync(id);
            if (creatorId.IsFailure) return NotFound();

            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (creatorId.Value != userId) return Forbid();
        }

        var result = await service.DeleteAsync(id);
        if (result.IsFailure)
            return result.Error is ProductNotFoundError ? NotFound() : StatusCode(500);

        TempData["Success"] = $"Producto \"{result.Value.Name}\" eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }


    /// <summary>Añade un comentario a un producto (solo rol User).</summary>
    [HttpPost("{id}/comentar")]
    [Authorize(Roles = UserRoles.USER)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(string id, [FromForm] AddCommentViewModel vm)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(vm.CommentText))
        {
            TempData["Error"] = "El comentario no puede estar vacío.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userId, out int numericUserId))
        {
            TempData["Error"] = "No se pudo identificar al usuario.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var comment = new dawazonBackend.Products.Models.Comment
        {
            UserId = numericUserId,
            Content = vm.CommentText.Trim(),
            recommended = vm.Recommended,
            verified = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await service.AddCommentAsync(id, comment);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "¡Comentario publicado correctamente!"
            : "No se pudo publicar el comentario. Inténtalo de nuevo.";

        return RedirectToAction(nameof(Detail), new { id });
    }

    /// <summary>Obtiene todas las categorías como SelectListItem para los formularios.</summary>
    private async Task<List<SelectListItem>> GetCategorySelectListAsync()
    {
        var all = await service.GetAllAsync(new FilterDto(null, null, 0, 1000, "id", "asc"));
        var categorias = all.Content
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        ViewBag.Categorias = categorias;
        return categorias.Select(c => new SelectListItem(c, c)).ToList();
    }

    /// <summary>Puebla ViewBag.Categorias para el navbar.</summary>
    private async Task SetCategoriasBagAsync()
    {
        await GetCategorySelectListAsync(); 
    }
}
