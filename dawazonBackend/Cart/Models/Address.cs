using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Cart.Models;

public class Address
{
    [Range(0,short.MaxValue)]
    public short Number { get; set; }
    [Required]
    public string Street {get; set;} = string.Empty;
    [Required]
    public string City {get; set;} = string.Empty;
    [Required]
    public string Province {get; set;} = string.Empty;
    [Required]
    public string Country {get; set;} = string.Empty;
    [Range(0,63000)]
    private int PostalCode { get; set; } 
    
}