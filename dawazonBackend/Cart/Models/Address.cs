using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Cart.Models;

/// <summary>
/// Representa una dirección postal.
/// </summary>
public class Address
{
    /// <summary>
    /// Número de la calle o vivienda.
    /// </summary>
    [Range(0,int.MaxValue)]
    public int Number { get; set; }

    /// <summary>
    /// Nombre de la calle.
    /// </summary>
    [Required]
    public string Street {get; set;} = string.Empty;

    /// <summary>
    /// Ciudad.
    /// </summary>
    [Required]
    public string City {get; set;} = string.Empty;

    /// <summary>
    /// Provincia o región.
    /// </summary>
    [Required]
    public string Province {get; set;} = string.Empty;

    /// <summary>
    /// País.
    /// </summary>
    [Required]
    public string Country {get; set;} = string.Empty;

    /// <summary>
    /// Código postal.
    /// </summary>
    [Range(0,63000)]
    public int PostalCode { get; set; } 
    
}