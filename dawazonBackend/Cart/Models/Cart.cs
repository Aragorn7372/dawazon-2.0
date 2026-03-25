using System.ComponentModel.DataAnnotations;
using dawazonBackend.Common.Attribute;

namespace dawazonBackend.Cart.Models;

/// <summary>
/// Modelo de dominio que representa un carrito de compras o un pedido finalizado.
/// </summary>
/// <remarks>
/// Esta es la entidad principal del módulo de carrito. Representa tanto carritos
/// activos (no comprados) como pedidos históricos (ya comprados).
///
/// <para><b>Estados del carrito:</b></para>
/// <list type="bullet">
///     <item><b>Carrito activo:</b> Purchased = false, CheckoutInProgress = false/true</item>
///     <item><b>Pedido:</b> Purchased = true, CheckoutInProgress = false</item>
/// </list>
/// 
/// <para><b>Ciclo de vida:</b></para>
/// <code>
/// Carrito nuevo → Añadir productos → Checkout → Pago → Pedido (Purchased=true)
/// </code>
/// 
/// <para><b>Relaciones:</b></para>
/// <list type="bullet">
///     <item>Un Cart tiene muchos <see cref="CartLine"/></item>
///     <item>Un Cart tiene un <see cref="Client"/></item>
/// </list>
/// 
/// <para><b>Patrón de precios:</b></para>
/// TotalItems y Total se calculan y almacenan para optimizar consultas.
/// Se actualizan cada vez que se modifica el carrito.
/// </remarks>
public class Cart
{
    /// <summary>
    /// Identificador único del carrito con formato personalizado.
    /// </summary>
    /// <value>String con ID generado automáticamente (ejemplo: "CART-2024-00001").</value>
    /// <remarks>
    /// Se genera automáticamente mediante <see cref="GenerateCustomIdAtribute"/> 
    /// en el momento de creación. Es la clave primaria de la entidad.
    /// </remarks>
    [Key]
    [GenerateCustomIdAtribute]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del usuario propietario del carrito.
    /// </summary>
    /// <value>Long con el ID del usuario.</value>
    /// <remarks>
    /// Relación con la tabla de usuarios. Cada usuario puede tener múltiples
    /// carritos (activos e históricos).
    /// </remarks>
    [Required]
    public long UserId {get; set;}

    /// <summary>
    /// Indica si el carrito ha sido convertido en un pedido completado.
    /// </summary>
    /// <value>True si el pedido fue pagado y completado, false si está activo.</value>
    /// <remarks>
    /// Cuando es true, el carrito se considera un pedido histórico y no
    /// puede ser modificado. Es el flag principal para distinguir
    /// entre carrito activo y pedido.
    /// </remarks>
    [Required]
    public bool Purchased {get; set;}

    /// <summary>
    /// Información del cliente para envío del pedido.
    /// </summary>
    /// <value>Instancia de <see cref="Client"/> con datos de contacto y dirección.</value>
    /// <remarks>
    /// Se cumplimenta cuando el usuario inicia el checkout.
    /// Se copia en el momento de la compra para mantener historial.
    /// </remarks>
    [Required] 
    public Client Client { get; set; } = new();

    /// <summary>
    /// Colección de líneas de productos en el carrito.
    /// </summary>
    /// <value>Lista de objetos <see cref="CartLine"/>.</value>
    /// <remarks>
    /// Cada línea representa un producto diferente con su cantidad.
    /// La lista puede estar vacía para un carrito sin productos.
    /// </remarks>
    [Required] 
    public List<CartLine> CartLines { get; set; } = [];

    /// <summary>
    /// Cantidad total de productos (suma de quantities).
    /// </summary>
    /// <value>Entero con la suma de cantidades de todas las líneas.</value>
    /// <remarks>
    /// Se calcula y persiste para evitar recalcular en cada consulta.
    /// Se actualiza automáticamente al añadir/eliminar productos.
    /// </remarks>
    [Required]
    public int TotalItems {get; set;}

    /// <summary>
    /// Importe total del carrito.
    /// </summary>
    /// <value>Double con la suma de TotalPrice de todas las líneas.</value>
    /// <remarks>
    /// Se calcula y persiste para optimizar consultas de display.
    /// Incluye el precio de los productos (sin impuestos adicionales).
    /// </remarks>
    [Required]
    public double Total {get; set;}

    /// <summary>
    /// Fecha y hora de creación del carrito.
    /// </summary>
    /// <value>Fecha en formato UTC.</value>
    /// <remarks>
    /// Se establece automáticamente al crear el carrito.
    /// Se usa para auditoría y para mostrar historial.
    /// </remarks>
    [Required]
    public DateTime CreatedAt {get; set;}= DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora de la última modificación.
    /// </summary>
    /// <value>Fecha en formato UTC.</value>
    /// <remarks>
    /// Se actualiza cada vez que se modifica el carrito (añadir, quitar productos, etc.).
    /// Útil para caché y para detectar cambios.
    /// </remarks>
    [Required]
    public DateTime UploadAt {get; set;}= DateTime.UtcNow;

    /// <summary>
    /// Indica si hay un proceso de checkout/pago en curso.
    /// </summary>
    /// <value>True si el usuario está en proceso de pago.</value>
    /// <remarks>
    /// Se usa para prevenir modificaciones concurrentes durante el pago.
    /// Cuando es true, se bloquean operaciones que puedan afectar el pedido.
    /// </remarks>
    [Required] 
    public bool CheckoutInProgress { get; set; } = false;

    /// <summary>
    /// Fecha y hora cuando se inició el proceso de checkout.
    /// </summary>
    /// <value>Fecha UTC o null si no hay checkout en curso.</value>
    /// <remarks>
    /// Se establece cuando CheckoutInProgress pasa a true.
    /// Se usa para implementar timeouts de checkout.
    /// </remarks>
    public DateTime? CheckoutStartedAt {get; set;} 

    /// <summary>
    /// Calcula los minutos transcurridos desde que se inició el proceso de checkout.
    /// </summary>
    /// <returns>Long con los minutos transcurridos, o 0 si no hay checkout iniciado.</returns>
    /// <remarks>
    /// Se utiliza para implementar un timeout de checkout (ejemplo: liberar stock
    /// después de 15 minutos de inactividad).
    /// </remarks>
    public long GetMinutesSinceCheckoutStarted()
    {
        if (this.CheckoutStartedAt== null)
        {
            return 0;
        }
        return (long)(DateTime.UtcNow - CheckoutStartedAt.Value).TotalMinutes;
    }
}