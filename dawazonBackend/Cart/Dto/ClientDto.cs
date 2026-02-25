using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Cart.Dto;

public record ClientDto{
  [Required]
  [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre tiene que tener entre 2 y 100 caracteres")]
  public string Name  {get; set;} = string.Empty;
  [Required]
  [EmailAddress]
  public string Email {get; set;} = string.Empty;
  [Required]
  [RegularExpression("^\\d{9}$")]
  public string Phone {get; set;} = string.Empty;
  [Required]
  [Range(0, short.MaxValue, ErrorMessage = "El número no es válido")]
  public int Number {get; set;}
  [Required]
  [StringLength(100, MinimumLength = 2, ErrorMessage = "La calle no es válida.")]
  public string Street {get; set;} = string.Empty;
  [Required] 
  [StringLength(100, MinimumLength = 2, ErrorMessage = "La ciudad no es válida.")]
  public string City { get; set; } = string.Empty;
  [Required] 
  [StringLength(100, MinimumLength = 2, ErrorMessage = "La provincia no es válida.")]
  public string Province { get; set; } = string.Empty;
  [Required] 
  [StringLength(100, MinimumLength = 2, ErrorMessage = "El país no es válido.")]
  public string Country { get; set; } = string.Empty;
  [Required]
  [Range(0, 60000, ErrorMessage = "El código postal no es válido.")]  
  public int PostalCode {get; set;}
}