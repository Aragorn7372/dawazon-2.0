using System.ComponentModel.DataAnnotations;
using dawazonBackend.Cart.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Models;

public class User: IdentityUser<long>
{
    public const string DEFAULT_IMAGE = "default.png";
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public bool IsDeleted { get; set; } = false;
    public Client Client { get; set; } = new Client();
    public string Avatar { get; set; } =DEFAULT_IMAGE;
   
}