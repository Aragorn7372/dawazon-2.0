using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Cart.Models;

/// <summary>
/// Modelo de dominio que representa la información de un cliente asociado a un carrito o pedido.
/// </summary>
/// <remarks>
/// Esta clase contiene los datos personales y de contacto del cliente necesarios
/// para la entrega del pedido.
///
/// <para><b>Relaciones:</b></para>
/// <list type="bullet">
///     <item>Un <see cref="Cart"/> tiene un <see cref="Client"/></item>
///     <item><see cref="Address"/> se composiciona dentro de Client</item>
/// </list>
/// 
/// <para><b>Validaciones:</b></para>
/// <list type="bullet">
///     <item>Name: Requerido</item>
///     <item>Email: Formato válido de email</item>
///     <item>Phone: Exactly 9 dígitos (formato español)</item>
///     <item>Address: Requerido, composición de dirección</item>
/// </list>
/// </remarks>
public class Client
{
    /// <summary>
    /// Nombre completo del cliente.
    /// </summary>
    /// <value>Nombre del cliente.</value>
    /// <remarks>
    /// Se utiliza para identificar al cliente y para la etiqueta de envío.
    /// </remarks>
    [Required]
    public string Name {get; set;} = string.Empty;
    
    /// <summary>
    /// Dirección de correo electrónico del cliente.
    /// </summary>
    /// <value>Email válido del cliente.</value>
    /// <remarks>
    /// Se utiliza para notificaciones del pedido, facturas y comunicación.
    /// </remarks>
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
    
    /// <summary>
    /// Número de teléfono móvil del cliente.
    /// </summary>
    /// <value>Número de 9 dígitos en formato español.</value>
    /// <remarks>
    /// Se utiliza para notificaciones SMS sobre el estado del envío.
    /// </remarks>
    [Required]
    [RegularExpression("^\\d{9}$")]
    public string Phone {get; set;} = string.Empty;
    
    /// <summary>
    /// Dirección de envío del cliente.
    /// </summary>
    /// <value>Instancia de <see cref="Address"/> con los datos de dirección.</value>
    /// <remarks>
    /// Se composiciona (no es una relación de FK) para mantener la integridad.
    /// Si el cliente cambia de dirección, los pedidos anteriores mantienen su dirección original.
    /// </remarks>
    [Required] 
    public Address Address { get; set; } = new();
}