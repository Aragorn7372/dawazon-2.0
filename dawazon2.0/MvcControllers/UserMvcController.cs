using System.Security.Claims;
using dawazon2._0.Models;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service;
using dawazonBackend.Users.Service.Favs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace dawazon2._0.MvcControllers;

/// <summary>
/// Controlador MVC para la gestión del perfil del usuario y favoritos.
/// Solo accesible por usuarios con rol User.
/// </summary>
[Route("perfil")]
[Authorize(Roles = UserRoles.USER)]
public class UserMvcController(
    IUserService userService,
    IFavService favService,
    UserManager<User> userManager,
    SignInManager<User> signInManager) : Controller
{

    /// <summary>Vista de resumen del perfil del usuario logueado.</summary>
    [HttpGet("")]
    public async Task<IActionResult> Profile()
    {
        var userId = GetUserId();
        Log.Information("[UserMvc] Profile → userId={UserId}", userId);

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        var vm = new UserProfileViewModel
        {
            Name       = user.Name,
            Email      = user.Email ?? string.Empty,
            Phone      = user.PhoneNumber ?? string.Empty,
            Avatar     = user.Avatar,
            FavCount   = user.ProductsFavs.Count,
            Street     = user.Client.Address?.Street   ?? string.Empty,
            City       = user.Client.Address?.City     ?? string.Empty,
            Province   = user.Client.Address?.Province ?? string.Empty,
            PostalCode = user.Client.Address?.PostalCode.ToString() ?? string.Empty,
            Country    = user.Client.Address?.Country  ?? string.Empty,
        };

        return View(vm);
    }

    /// <summary>Formulario de edición del perfil.</summary>
    [HttpGet("editar")]
    public async Task<IActionResult> EditProfile()
    {
        var userId = GetUserId();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        var vm = new UserEditViewModel
        {
            Nombre      = user.Name,
            Email       = user.Email ?? string.Empty,
            Telefono    = user.PhoneNumber ?? string.Empty,
            Calle       = user.Client.Address?.Street   ?? string.Empty,
            Ciudad      = user.Client.Address?.City     ?? string.Empty,
            CodigoPostal= user.Client.Address?.PostalCode.ToString() ?? string.Empty,
            Provincia   = user.Client.Address?.Province ?? string.Empty,
        };

        return View(vm);
    }

    /// <summary>Guarda los cambios del perfil.</summary>
    [HttpPost("editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile([FromForm] UserEditViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var userId = GetUserId();
        Log.Information("[UserMvc] EditProfile → userId={UserId}", userId);

        var dto = new UserRequestDto
        {
            Nombre      = vm.Nombre,
            Email       = vm.Email,
            Telefono    = vm.Telefono,
            Calle       = vm.Calle,
            Ciudad      = vm.Ciudad,
            CodigoPostal= vm.CodigoPostal,
            Provincia   = vm.Provincia
        };

        var result = await userService.UpdateByIdAsync(userId, dto, vm.Avatar);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(vm);
        }

        TempData["Success"] = "Perfil actualizado correctamente.";
        return RedirectToAction(nameof(Profile));
    }

    /// <summary>Borrado lógico de la cuenta del usuario</summary>
    [HttpPost("eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = GetUserId();
        Log.Warning("[UserMvc] DeleteAccount → userId={UserId}", userId);

        await userService.BanUserById(userId.ToString());
        await signInManager.SignOutAsync();

        TempData["Info"] = "Tu cuenta ha sido desactivada.";
        return RedirectToAction("Index", "ProductsMvc");
    }

    /// <summary>Lista paginada de productos favoritos.</summary>
    [HttpGet("favoritos")]
    public async Task<IActionResult> Favs([FromQuery] int page = 0, [FromQuery] int size = 12)
    {
        var userId = GetUserId();
        Log.Information("[UserMvc] Favs → userId={UserId} page={Page}", userId, page);

        var filter = new FilterDto(null, null, page, size, "id", "asc");
        var result = await favService.GetFavs(userId, filter);

        if (result.IsFailure)
            return StatusCode(500);

        var vm = new UserFavsViewModel
        {
            Products      = result.Value.Content,
            PageNumber    = result.Value.PageNumber,
            TotalPages    = result.Value.TotalPages,
            TotalElements = result.Value.TotalElements
        };

        return View(vm);
    }

    /// <summary>Añade un producto a favoritos y vuelve al detalle del producto.</summary>
    [HttpPost("favoritos/add/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFav(string id)
    {
        var userId = GetUserId();
        Log.Information("[UserMvc] AddFav → userId={UserId} productId={ProductId}", userId, id);

        await favService.AddFav(id, userId);

        return RedirectToAction("Detail", "ProductsMvc", new { id });
    }

    /// <summary>Quita un producto de favoritos y vuelve al detalle del producto.</summary>
    [HttpPost("favoritos/remove/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFav(string id)
    {
        var userId = GetUserId();
        Log.Information("[UserMvc] RemoveFav → userId={UserId} productId={ProductId}", userId, id);

        await favService.RemoveFav(id, userId);

        return RedirectToAction("Detail", "ProductsMvc", new { id });
    }

    private long GetUserId() =>
        long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
