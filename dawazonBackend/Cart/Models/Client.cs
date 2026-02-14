using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Cart.Models;

public class Client
{
    [Required]
    public string Name {get; set;} = string.Empty;
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
    [Required]
    [RegularExpression("^\\d{9}$")]
    public string Phone {get; set;} = string.Empty;

    [Required] 
    public Address Address { get; set; } = new();
}