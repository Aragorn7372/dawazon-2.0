using dawazonBackend.Cart.Models;

namespace dawazonBackend.Cart.Dto;

/// <summary>
/// Data Transfer Object (DTO) utilizado para solicitudes de modificación de líneas de carrito.
/// </summary>
/// <remarks>
/// Este DTO se utiliza en las solicitudes HTTP (POST/PUT) para actualizar o añadir
/// líneas de productos en un carrito de compras.
///
/// <para><b>Uso típico:</b></para>
/// <list type="bullet">
///     <item>POST /cart/line - Añadir una nueva línea al carrito</item>
///     <item>PUT /cart/line - Actualizar una línea existente</item>
///     <item>PATCH /cart/line - Modificar el estado de una línea</item>
/// </list>
/// 
/// <para><b>Estados posibles:</b></para>
/// <see cref="Status"/> enum define los estados del ciclo de vida del pedido.
/// </remarks>
public class LineRequestDto
{
    /// <summary>
    /// Identificador del carrito al cual pertenece la línea.
    /// </summary>
    /// <value>String con el identificador del carrito (ejemplo: "CART-2024-001").</value>
    /// <remarks>
    /// Este campo es requerido. Identifica el carrito padre de la línea.
    /// </remarks>
    public string CartId {get; set;} = string.Empty;
    
    /// <summary>
    /// Identificador del producto a añadir o modificar.
    /// </summary>
    /// <value>String con el identificador del producto (ejemplo: "PROD-123").</value>
    /// <remarks>
    /// Este campo es requerido. Relaciona la línea con un producto del catálogo.
    /// </remarks>
    public string ProductId {get; set;} = string.Empty;
    
    /// <summary>
    /// Estado de la línea de venta en el carrito.
    /// </summary>
    /// <value>Enumeración <see cref="Status"/> indicando el estado actual.</value>
    /// <remarks>
    /// Por defecto se inicializa en <see cref="Status.EnCarrito"/>.
    /// Se utiliza para hacer seguimiento del procesamiento del pedido.
    /// </remarks>
    public Status Status { get; set; } = new();
}