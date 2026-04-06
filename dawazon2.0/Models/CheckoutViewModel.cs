using System.ComponentModel.DataAnnotations;
using dawazonBackend.Cart.Models;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la vista de confirmación de envío antes de pasar a Stripe.
/// </summary>
/// <remarks>
/// Agrupa los datos del carrito (solo lectura) y los datos de cliente/dirección (editables).
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>DataAnnotations: Validaciones</item>
///     <item>CartLineViewModel: Líneas del carrito</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Datos del carrito (lectura): CartId, Lines, TotalItems, Total</item>
///     <item>Datos de envío (edición): Name, Email, Phone, Dirección</item>
/// </list>
/// </remarks>
public class CheckoutViewModel
{
    /// <summary>Identificador único del carrito.</summary>
    public string CartId { get; set; } = string.Empty;
    
    /// <summary>Lista de productos en el carrito.</summary>
    public List<CartLineViewModel> Lines { get; set; } = [];
    
    /// <summary>Cantidad total de artículos.</summary>
    public int TotalItems { get; set; }
    
    /// <summary>Total monetario del carrito.</summary>
    public double Total { get; set; }

    /// <summary>Nombre completo del cliente.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombre completo")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Correo electrónico del cliente.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    ///     <item>EmailAddress: Formato válido</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Teléfono de contacto.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    ///     <item>RegularExpression: Exactamente 9 dígitos</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "El teléfono debe tener 9 dígitos")]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Dirección - Calle.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "La calle es obligatoria")]
    [Display(Name = "Calle")]
    public string Street { get; set; } = string.Empty;

    /// <summary>Dirección - Número.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    ///     <item>Range: 1-99999</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El número es obligatorio")]
    [Range(1, 99999, ErrorMessage = "Número no válido")]
    [Display(Name = "Número")]
    public int Number { get; set; }

    /// <summary>Dirección - Ciudad.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatoria</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "La ciudad es obligatoria")]
    [Display(Name = "Ciudad")]
    public string City { get; set; } = string.Empty;

    /// <summary>Dirección - Provincia.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatoria</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "La provincia es obligatoria")]
    [Display(Name = "Provincia")]
    public string Province { get; set; } = string.Empty;

    /// <summary>Dirección - País.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El país es obligatorio")]
    [Display(Name = "País")]
    public string Country { get; set; } = string.Empty;

    /// <summary>Dirección - Código postal.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    ///     <item>Range: 1000-99999</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El código postal es obligatorio")]
    [Range(1000, 99999, ErrorMessage = "Código postal no válido")]
    [Display(Name = "Código postal")]
    public int PostalCode { get; set; }
}
