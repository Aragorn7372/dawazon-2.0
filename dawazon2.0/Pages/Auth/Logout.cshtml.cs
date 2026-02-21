using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dawazon2._0.Pages.Auth;

public class Logout(SignInManager<User> signInManager,ILogger<Logout> logger) : PageModel
{

    public async Task<IActionResult> OnGetAsync()
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("Usuario ha cerrado sesión.");
        TempData["SuccessMessage"] = "Has cerrado sesión correctamente.";
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("Usuario ha cerrado sesión.");
        TempData["SuccessMessage"] = "Has cerrado sesión correctamente.";
        return RedirectToPage("/Index");
    }
}