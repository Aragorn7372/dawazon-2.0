

namespace dawazonBackend.Cart.Dto;

/// <summary>
/// Data Transfer Object (DTO) utilizado para solicitudes de gestión de stock en el carrito.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para actualizar la cantidad de productos en el carrito,
/// operando sobre el inventario disponible.
///
/// <para><b>Uso típico:</b></para>
/// <list type="bullet">
///     <item>PUT /cart/stock - Actualizar cantidad de un producto en el carrito</item>
///     <item>POST /cart/stock/increase - Incrementar cantidad</item>
///     <item>POST /cart/stock/decrease - Reducir cantidad</item>
/// </list>
/// 
/// <para><b>Notas:</b></para>
/// <list type="bullet">
///     <item>Si CartId es null, se opera sobre el carrito activo del usuario.</item>
///     <item>La cantidad no puede ser negativa.</item>
///     <item>Es un record, por lo que implementa igualdad por valor.</item>
/// </list>
/// </remarks>
public record CartStockRequestDto
{
    /// <summary>
    /// Identificador opcional del carrito a modificar.
    /// </summary>
    /// <value>String con el ID del carrito o null para usar el carrito activo.</value>
    /// <remarks>
    /// Cuando es null, el sistema utilizará el carrito activo del usuario.
    /// Esto simplifica las llamadas desde el frontal cuando solo hay un carrito.
    /// </remarks>
    public string? CartId { get; set; }

    /// <summary>
    /// Identificador del usuario que realiza la operación.
    /// </summary>
    /// <value>String con el identificador del usuario (puede ser numérico como string).</value>
    /// <remarks>
    /// Se utiliza para localizar el carrito correcto y para auditoria.
    /// </remarks>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Cantidad de unidades del producto.
    /// </summary>
    /// <value>Entero no negativo con la cantidad.</value>
    /// <remarks>
    /// Representa la cantidad a establecer, incrementar o decrementar
    /// dependiendo de la operación específica que se realice.
    /// </remarks>
    public int Quantity { get; set; }

}