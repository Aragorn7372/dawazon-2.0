using dawazonBackend.Cart.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Models;

public class User: IdentityUser<long>
{
    public string Name { get; set; } = string.Empty;
    public Client Client { get; set; } = new Client();
    
}