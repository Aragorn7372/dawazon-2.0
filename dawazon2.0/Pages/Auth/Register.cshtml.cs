using dawazon2._0.Mapper;
using dawazon2._0.Models;
using dawazonBackend.Users.Service.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazon2._0.Pages.Auth;

public class Register(IAuthService auth, ILogger<Register> logger, SignInManager<User> signInManager, UserManager<User> userManager) : PageModel
{
    [BindProperty]
    public RegisterModelView UserRegister { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        ErrorMessage = null;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await auth.SignUpAsync(UserRegister.ToDto());

            if (result.IsSuccess)
            {
                // Buscamos el usuario recién creado
                var newUser = await userManager.FindByEmailAsync(UserRegister.Email);
                if (newUser != null)
                {
                    // Iniciar sesión automáticamente
                    await signInManager.SignInAsync(newUser, isPersistent: true);
                }

                logger.LogInformation("Usuario {Email} se ha registrado e iniciado sesión correctamente.", UserRegister.Email);
                return LocalRedirect(returnUrl);
            }

            ErrorMessage = result.Error.Message;
            logger.LogWarning("Error en registro: {Error}", ErrorMessage);
            return Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico durante el registro");
            ErrorMessage = "Ocurrió un error inesperado. Por favor, inténtalo de nuevo.";
            return Page();
        }
    }
}