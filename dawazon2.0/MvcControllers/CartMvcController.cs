using System.Security.Claims;
using dawazon2._0.Mapper;
using dawazon2._0.Models;
using dawazon2._0.Pdf;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Models;
using dawazonBackend.Cart.Service;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace dawazon2._0.MvcControllers;

/// <summary>
/// Controlador MVC para la sección "Mis Pedidos" del usuario.
/// Muestra los carritos ya comprados (Purchased = true) del usuario autenticado.
/// </summary>
[Route("pedidos")]
[Authorize(Roles = UserRoles.USER)]
public class CartMvcController(
    ICartService cartService,
    UserManager<User> userManager,
    IOrderPdfService pdfService) : Controller
{
    /// <summary>Muestra el carrito activo (no comprado) del usuario.</summary>
    [HttpGet("carrito")]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var cartResult = await cartService.GetCartByUserIdAsync(userId);

        if (cartResult.IsFailure)
        {
            // Carrito vacío o no encontrado: mostrar vista vacía
            ViewBag.CartEmpty = true;
            return View(new CartOrderDetailViewModel());
        }

        var vm = cartResult.Value.ToOrderDetailViewModel();
        return View(vm);
    }

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

    /// <summary>Añade un producto al carrito activo del usuario.</summary>
    [HttpPost("add/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(string productId)
    {
        var userId = GetUserId();
        var cartResult = await cartService.GetCartByUserIdAsync(userId);
        if (cartResult.IsFailure)
        {
            TempData["Error"] = "No se pudo encontrar tu carrito.";
            return RedirectToAction("Detail", "ProductsMvc", new { id = productId });
        }

        var result = await cartService.AddProductAsync(cartResult.Value.Id, productId);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Producto añadido al carrito."
            : result.Error.Message;

        return RedirectToAction("Detail", "ProductsMvc", new { id = productId });
    }

    /// <summary>Elimina un producto del carrito activo del usuario.</summary>
    [HttpPost("remove/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromCart(string productId)
    {
        var userId = GetUserId();
        var cartResult = await cartService.GetCartByUserIdAsync(userId);
        if (cartResult.IsFailure)
        {
            TempData["Error"] = "No se pudo encontrar tu carrito.";
            return RedirectToAction("Detail", "ProductsMvc", new { id = productId });
        }

        await cartService.RemoveProductAsync(cartResult.Value.Id, productId);
        TempData["Success"] = "Producto eliminado del carrito.";

        return RedirectToAction("Detail", "ProductsMvc", new { id = productId });
    }
    /// <summary>Muestra la página de confirmación de datos antes de pagar con Stripe.</summary>
    [HttpGet("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var userId = GetUserId();
        var cartResult = await cartService.GetCartByUserIdAsync(userId);
        if (cartResult.IsFailure)
            return RedirectToAction(nameof(Index));

        var user = await userManager.FindByIdAsync(userId.ToString());
        var client = user?.Client;
        var addr = client?.Address;

        var vm = new CheckoutViewModel
        {
            CartId    = cartResult.Value.Id,
            Lines     = cartResult.Value.ToOrderDetailViewModel().Lines,
            TotalItems= cartResult.Value.TotalItems,
            Total     = cartResult.Value.Total,
            Name      = client?.Name    ?? string.Empty,
            Email     = client?.Email   ?? string.Empty,
            Phone     = client?.Phone   ?? string.Empty,
            Street    = addr?.Street    ?? string.Empty,
            Number    = addr?.Number    ?? 0,
            City      = addr?.City      ?? string.Empty,
            Province  = addr?.Province  ?? string.Empty,
            Country   = addr?.Country   ?? string.Empty,
            PostalCode= addr?.PostalCode ?? 0,
        };

        return View(vm);
    }

    /// <summary>Procesa el formulario de checkout: guarda el Client y lanza Stripe.</summary>
    [HttpPost("checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel vm)
    {
        // Si el modelo no es válido, recargamos las líneas del carrito y volvemos al form
        if (!ModelState.IsValid)
        {
            var cartResult2 = await cartService.GetCartByUserIdAsync(GetUserId());
            if (cartResult2.IsSuccess)
            {
                vm.Lines      = cartResult2.Value.ToOrderDetailViewModel().Lines;
                vm.TotalItems = cartResult2.Value.TotalItems;
                vm.Total      = cartResult2.Value.Total;
            }
            return View(vm);
        }

        // Actualizar el Client del usuario con los datos del formulario
        var userId = GetUserId();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.Client = new Client
            {
                Name  = vm.Name.Trim(),
                Email = vm.Email.Trim(),
                Phone = vm.Phone.Trim(),
                Address = new Address
                {
                    Street     = vm.Street.Trim(),
                    Number     = vm.Number,
                    City       = vm.City.Trim(),
                    Province   = vm.Province.Trim(),
                    Country    = vm.Country.Trim(),
                    PostalCode = vm.PostalCode
                }
            };
            await userManager.UpdateAsync(user);
        }

        // Lanzar checkout de Stripe
        var result = await cartService.CheckoutAsync(vm.CartId);
        if (result.IsFailure)
        {
            // Recargamos líneas para poder repintar el resumen
            var cartReload = await cartService.GetCartByUserIdAsync(GetUserId());
            if (cartReload.IsSuccess)
            {
                vm.Lines      = cartReload.Value.ToOrderDetailViewModel().Lines;
                vm.TotalItems = cartReload.Value.TotalItems;
                vm.Total      = cartReload.Value.Total;
            }
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(vm);
        }

        return Redirect(result.Value); // URL de Stripe
    }

    /// <summary>
    /// Stripe redirige aquí tras un pago exitoso.
    /// Marca el carrito como comprado, crea un carrito nuevo vacío y envía el email de confirmación.
    /// </summary>
    [HttpGet("success")]
    [AllowAnonymous] // Stripe redirige desde fuera — no podemos garantizar la cookie de sesión
    public async Task<IActionResult> Success([FromQuery] string cartId)
    {
        if (string.IsNullOrEmpty(cartId))
            return RedirectToAction(nameof(Index));

        // Cargar el carrito comprado
        var cartResult = await cartService.GetByIdAsync(cartId);
        if (cartResult.IsFailure)
        {
            TempData["Error"] = "No se encontró el pedido.";
            return View();
        }

        var cart = await cartService.GetCartModelByIdAsync(cartId);
        if (cart != null)
        {
            // Marcar como comprado y crear nuevo carrito vacío para el usuario
            await cartService.SaveAsync(cart);

            TempData["OrderId"] = cartId;

            // Enviar email de confirmación (no bloqueante)
            await cartService.SendConfirmationEmailAsync(cart);
        }
        else
        {
            TempData["OrderId"] = cartId;
        }

        return View();
    }

    /// <summary>
    /// Stripe redirige aquí si el usuario cancela el pago.
    /// El carrito permanece intacto.
    /// </summary>
    [HttpGet("cancel")]
    [AllowAnonymous]
    public IActionResult Cancel()
    {
        return View();
    }

    /// <summary>Incrementa en 1 la cantidad de un producto en el carrito activo.</summary>
    [HttpPost("qty/add/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IncreaseQty(string productId)
    {
        var userId = GetUserId();
        var cartResult = await cartService.GetCartByUserIdAsync(userId);
        if (cartResult.IsFailure)
        {
            TempData["Error"] = "No se pudo encontrar tu carrito.";
            return RedirectToAction(nameof(Index));
        }

        var cart = cartResult.Value;
        var currentLine = cart.CartLines.FirstOrDefault(l => l.ProductId == productId);
        int newQty = (currentLine?.Quantity ?? 0) + 1;

        var result = await cartService.UpdateStockWithValidationAsync(cart.Id, productId, newQty);
        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Decrementa en 1 la cantidad de un producto. Si llega a 0, lo elimina del carrito.</summary>
    [HttpPost("qty/remove/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DecreaseQty(string productId)
    {
        var userId = GetUserId();
        var cartResult = await cartService.GetCartByUserIdAsync(userId);
        if (cartResult.IsFailure)
        {
            TempData["Error"] = "No se pudo encontrar tu carrito.";
            return RedirectToAction(nameof(Index));
        }

        var cart = cartResult.Value;
        var currentLine = cart.CartLines.FirstOrDefault(l => l.ProductId == productId);
        int currentQty = currentLine?.Quantity ?? 1;

        if (currentQty <= 1)
        {
            // Cantidad llega a 0 eliminar del carrito
            await cartService.RemoveProductAsync(cart.Id, productId);
        }
        else
        {
            await cartService.UpdateStockWithValidationAsync(cart.Id, productId, currentQty - 1);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Elimina directamente un producto del carrito y redirige al resumen del carrito.</summary>
    [HttpPost("delete/{productId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLine(string productId)
    {
        var userId = GetUserId();
        var cartResult = await cartService.GetCartByUserIdAsync(userId);
        if (cartResult.IsFailure)
        {
            TempData["Error"] = "No se pudo encontrar tu carrito.";
            return RedirectToAction(nameof(Index));
        }

        await cartService.RemoveProductAsync(cartResult.Value.Id, productId);
        TempData["Success"] = "Producto eliminado del carrito.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Descarga el PDF con el resumen de un pedido concreto.</summary>
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPdf(string id)
    {
        var userId = GetUserId();
        Log.Information("[CartMvc] DownloadPdf → userId={UserId} cartId={CartId}", userId, id);

        var result = await cartService.GetByIdAsync(id);

        if (result.IsFailure)
        {
            Log.Warning("[CartMvc] DownloadPdf: pedido {CartId} no encontrado", id);
            return NotFound();
        }

        if (result.Value.UserId != userId)
        {
            Log.Warning("[CartMvc] DownloadPdf: acceso denegado userId={UserId} cartId={CartId}", userId, id);
            return Forbid();
        }

        if (!result.Value.Purchased)
        {
            Log.Warning("[CartMvc] DownloadPdf: carrito {CartId} no está comprado", id);
            return NotFound();
        }

        var vm = result.Value.ToOrderDetailViewModel();
        var bytes = await pdfService.GenerateOrderPdfAsync(vm);

        var fileName = $"pedido-{id}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    private long GetUserId() =>
        long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
