using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Cart.Dto;

/// <summary>
/// Data Transfer Object (DTO) que representa la información de un cliente asociado a un carrito de compras.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transferir datos de dirección y contacto del cliente entre las capas
/// de la aplicación. Contiene validaciones para garantizar la integridad de los datos.
/// 
/// <para><b>Validaciones:</b></para>
/// <list type="bullet">
///     <item>Name: Requerido, entre 2 y 100 caracteres</item>
///     <item>Email: Requerido, formato de email válido</item>
///     <item>Phone: Requerido, exactamente 9 dígitos (formato español)</item>
///     <item>Number: Requerido, número de dirección válido</item>
///     <item>Street: Requerida, entre 2 y 100 caracteres</item>
///     <item>City: Requerida, entre 2 y 100 caracteres</item>
///     <item>Province: Requerida, entre 2 y 100 caracteres</item>
///     <item>Country: Requerido, entre 2 y 100 caracteres</item>
///     <item>PostalCode: Requerido, código postal entre 0 y 60000</item>
/// </list>
/// 
/// <para><b>Uso típico:</b></para>
/// Se utiliza en <see cref="CartResponseDto"/> para mostrar la información del cliente
/// asociado a un carrito, y en operaciones de creación/actualización de direcciones de envío.
/// </remarks>
public record ClientDto{
  /// <summary>
  /// Nombre completo del cliente.
  /// </summary>
  /// <value>Nombre del cliente con un mínimo de 2 y máximo de 100 caracteres.</value>
  /// <remarks>
  /// Este campo es requerido y debe contener el nombre real del cliente.
  /// Se utiliza para direccionar envíos y para identificación en el pedido.
  /// </remarks>
  [Required]
  [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre tiene que tener entre 2 y 100 caracteres")]
  public string Name  {get; set;} = string.Empty;
  
  /// <summary>
  /// Dirección de correo electrónico del cliente.
  /// </summary>
  /// <value>Email válido en formato estándar (ejemplo: usuario@dominio.com).</value>
  /// <remarks>
  /// Este campo es requerido y debe contener un email válido. Se utiliza para
  /// notificaciones del pedido y recuperación de cuenta.
  /// </remarks>
  [Required]
  [EmailAddress]
  public string Email {get; set;} = string.Empty;
  
  /// <summary>
  /// Número de teléfono móvil del cliente.
  /// </summary>
  /// <value>Número de teléfono de 9 dígitos en formato español.</value>
  /// <remarks>
  /// Este campo es requerido y debe contener exactamente 9 dígitos.
  /// Se utiliza para notificaciones SMS sobre el estado del pedido.
  /// </remarks>
  [Required]
  [RegularExpression("^\\d{9}$")]
  public string Phone {get; set;} = string.Empty;
  
  /// <summary>
  /// Número de dirección (número de portal/casa).
  /// </summary>
  /// <value>Número entero positivo que representa el número de la dirección.</value>
  /// <remarks>
  /// Este campo es requerido y debe ser un número válido para completar la dirección.
  /// </remarks>
  [Required]
  [Range(0, short.MaxValue, ErrorMessage = "El número no es válido")]
  public int Number {get; set;}
  
  /// <summary>
  /// Nombre de la calle o vía pública.
  /// </summary>
  /// <value>Nombre de la calle con un mínimo de 2 y máximo de 100 caracteres.</value>
  /// <remarks>
  /// Este campo es requerido y contiene el nombre de la calle donde se entregará el pedido.
  /// </remarks>
  [Required]
  [StringLength(100, MinimumLength = 2, ErrorMessage = "La calle no es válida.")]
  public string Street {get; set;} = string.Empty;
  
  /// <summary>
  /// Ciudad o población de la dirección.
  /// </summary>
  /// <value>Nombre de la ciudad con un mínimo de 2 y máximo de 100 caracteres.</value>
  /// <remarks>
  /// Este campo es requerido y representa la ciudad de envío.
  /// </remarks>
  [Required] 
  [StringLength(100, MinimumLength = 2, ErrorMessage = "La ciudad no es válida.")]
  public string City { get; set; } = string.Empty;
  
  /// <summary>
  /// Provincia o región administrativa.
  /// </summary>
  /// <value>Nombre de la provincia con un mínimo de 2 y máximo de 100 caracteres.</value>
  /// <remarks>
  /// Este campo es requerido y se utiliza para calcular costes de envío y restricciones.
  /// </remarks>
  [Required] 
  [StringLength(100, MinimumLength = 2, ErrorMessage = "La provincia no es válida.")]
  public string Province { get; set; } = string.Empty;
  
  /// <summary>
  /// País de la dirección de envío.
  /// </summary>
  /// <value>Nombre del país con un mínimo de 2 y máximo de 100 caracteres.</value>
  /// <remarks>
  /// Este campo es requerido. Para envíos internacionales, este campo indica el país de destino.
  /// </remarks>
  [Required] 
  [StringLength(100, MinimumLength = 2, ErrorMessage = "El país no es válido.")]
  public string Country { get; set; } = string.Empty;
  
  /// <summary>
  /// Código postal de la dirección.
  /// </summary>
  /// <value>Código postal numérico entre 0 y 60000.</value>
  /// <remarks>
  /// Este campo es requerido y se utiliza para calcular costes de envío,
  /// validar la zona de entrega y para estadística geográfica.
  /// </remarks>
  [Required]
  [Range(0, 60000, ErrorMessage = "El código postal no es válido.")]  
  public int PostalCode {get; set;}
}