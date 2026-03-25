using System.ComponentModel.DataAnnotations;
using dawazonBackend.Products.Models;

namespace dawazonBackend.Cart.Models;

/// <summary>
/// Modelo de dominio que representa una línea de producto dentro de un carrito de compras.
/// </summary>
/// <remarks>
/// Cada <see cref="CartLine"/> representa un producto específico con su cantidad
/// dentro de un <see cref="Cart"/>. Contiene el precio en el momento de añadir
/// para mantener historial de precios.
///
/// <para><b>Relaciones:</b></para>
/// <list type="bullet">
///     <item>Pertenece a un <see cref="Cart"/> mediante CartId</item>
///     <item>Referencia a <see cref="Product"/> mediante ProductId (navigation property)</item>
/// </list>
/// 
/// <para><b>Patrón de precios:</b></para>
/// El precio unitario se almacena en el momento de añadir al carrito.
/// Esto protege al usuario de cambios de precio durante el proceso de compra.
/// El precio total se calcula dinámicamente mediante <see cref="TotalPrice"/>.
/// </remarks>
public class CartLine
{ 
    /// <summary>
    /// Identificador del carrito al que pertenece esta línea.
    /// </summary>
    /// <value>String con el ID del carrito padre.</value>
    /// <remarks>
    /// Foreign key hacia la tabla de carritos.
    /// </remarks>
    [Required]
    public string CartId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del producto en el catálogo.
    /// </summary>
    /// <value>String con el ID del producto.</value>
    /// <remarks>
    /// Foreign key hacia la tabla de productos.
    /// </remarks>
    [Required]
    public string ProductId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navegación hacia el modelo del producto.
    /// </summary>
    /// <value>Instancia de <see cref="Product"/> o null si no se ha cargado.</value>
    /// <remarks>
    /// Esta propiedad es opcional y se usa para obtener información
    /// adicional del producto (nombre, imagen, etc.) cuando se requiere.
    /// </remarks>
    public Product? Product { get; set; }

    /// <summary>
    /// Cantidad de unidades del producto en esta línea.
    /// </summary>
    /// <value>Entero no negativo.</value>
    /// <remarks>
    /// Debe ser mayor que 0 para una línea válida.
    /// Se valida en el servicio antes de guardar.
    /// </remarks>
    [Required] 
    public int Quantity { get; set; } = 0;

    /// <summary>
    /// Precio unitario del producto en el momento de añadir al carrito.
    /// </summary>
    /// <value>Decimal con el precio (ejemplo: 19.99).</value>
    /// <remarks>
    /// Se almacena al añadir el producto y no se actualiza automáticamente.
    /// Esto mantiene el precio acordado aunque el producto cambie de precio.
    /// </remarks>
    [Required]
    public double ProductPrice { get; set; }
    
    /// <summary>
    /// Estado actual de esta línea de pedido.
    /// </summary>
    /// <value>Enumeración <see cref="Status"/>.</value>
    /// <remarks>
    /// Representa el estado en el ciclo de vida del pedido.
    /// Se actualiza a medida que el pedido progresa.
    /// </remarks>
    [Required]
    public Status  Status { get; set; }

    /// <summary>
    /// Calcula el precio total de esta línea (Precio unitario × Cantidad).
    /// </summary>
    /// <value>Decimal con el total (ejemplo: 39.98 para 2 unidades a 19.99).</value>
    /// <remarks>
    /// Propiedad calculada (readonly). Se recalcula cada vez que se accede.
    /// No se persiste en la base de datos.
    /// </remarks>
    public double TotalPrice => ProductPrice * Quantity;
}