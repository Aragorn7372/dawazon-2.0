using System.ComponentModel.DataAnnotations;
using dawazonBackend.Cart.Models;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la vista de confirmación de envío antes de pasar a Stripe.
/// Agrupa los datos del carrito (solo lectura) y los datos de cliente/dirección (editables).
/// </summary>
public class CheckoutViewModel
{
    public string CartId { get; set; } = string.Empty;
    public List<CartLineViewModel> Lines { get; set; } = [];
    public int TotalItems { get; set; }
    public double Total { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombre completo")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "El teléfono debe tener 9 dígitos")]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "La calle es obligatoria")]
    [Display(Name = "Calle")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número es obligatorio")]
    [Range(1, 99999, ErrorMessage = "Número no válido")]
    [Display(Name = "Número")]
    public short Number { get; set; }

    [Required(ErrorMessage = "La ciudad es obligatoria")]
    [Display(Name = "Ciudad")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "La provincia es obligatoria")]
    [Display(Name = "Provincia")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país es obligatorio")]
    [Display(Name = "País")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código postal es obligatorio")]
    [Range(1000, 99999, ErrorMessage = "Código postal no válido")]
    [Display(Name = "Código postal")]
    public int PostalCode { get; set; }
}
