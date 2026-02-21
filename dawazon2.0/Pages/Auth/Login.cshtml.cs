using dawazon2._0.Mapper;
using dawazon2._0.Models;
using dawazonBackend.Users.Service.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dawazon2._0.Pages.Auth;


public class Login(IAuthService auth, ILogger<Login> logger) : PageModel
{
    // Propiedades vinculadas al formulario
    [BindProperty] 
    public new LoginModelView User { get; set; } = new();

    public string? ErrorMessage { get; set; }
    

    public void OnGet()
    {
        logger.LogInformation("iniciando registro");
        // Limpiar errores previos
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
            
            var result = await auth.SignInAsync(User.ToDto());

            if (result.IsSuccess)
            {
                logger.LogInformation("Usuario {Email} ha iniciado sesión correctamente.", User.UsernameOrEmail);
                return LocalRedirect(returnUrl);
            }

            ErrorMessage = result.Error.Message;
            logger.LogWarning(ErrorMessage);
            return Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            ErrorMessage = "Ocurrió un error al intentar iniciar sesión. Por favor, inténtalo de nuevo.";
            return Page();
        }
    }


}