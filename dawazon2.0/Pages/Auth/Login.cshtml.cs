using dawazon2._0.Mapper;
using dawazon2._0.Models;
using dawazonBackend.Users.Service.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazon2._0.Pages.Auth;


public class Login(IAuthService auth, ILogger<Login> logger, SignInManager<User> signInManager, UserManager<User> userManager) : PageModel
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
                // Buscamos el usuario real para dárselo a Identity y que cree la cookie
                var applicationUser = User.UsernameOrEmail.Contains("@") 
                    ? await userManager.FindByEmailAsync(User.UsernameOrEmail)
                    : await userManager.FindByNameAsync(User.UsernameOrEmail);

                if (applicationUser != null)
                {
                    // PasswordSignInAsync es lo que realmente crea la cookie de sesión — verificamos el resultado
                    var signInResult = await signInManager.PasswordSignInAsync(applicationUser.UserName!, User.Password, isPersistent: true, lockoutOnFailure: false);
                    
                    if (!signInResult.Succeeded)
                    {
                        logger.LogWarning("PasswordSignInAsync falló para {Email}: Locked={Locked}, NotAllowed={NotAllowed}",
                            User.UsernameOrEmail, signInResult.IsLockedOut, signInResult.IsNotAllowed);
                        ErrorMessage = "No se pudo iniciar sesión. Por favor, inténtalo de nuevo.";
                        return Page();
                    }
                    
                    logger.LogInformation("Usuario {Email} ha iniciado sesión con éxito (JWT + Cookie).", User.UsernameOrEmail);
                    return LocalRedirect(returnUrl);
                }
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