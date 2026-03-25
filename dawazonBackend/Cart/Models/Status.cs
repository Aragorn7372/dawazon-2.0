namespace dawazonBackend.Cart.Models;

/// <summary>
/// Enumeración que representa los posibles estados de una línea de pedido en el sistema.
/// </summary>
/// <remarks>
/// Define el ciclo de vida completo de una línea de carrito/pedido desde que se añade
/// hasta que se completa o cancela. Cada estado representa una etapa del proceso de venta.
///
/// <para><b>Ciclo de vida:</b></para>
/// <code>
/// EnCarrito → Preparado → Enviado → Recibido
///           ↘ Cancelado (desde cualquier estado)
/// </code>
/// 
/// <para><b>Descripción de cada estado:</b></para>
/// <list type="table">
///     <listheader>
///         <term>Estado</term>
///         <description>Descripción</description>
///     </listheader>
///     <item>
///         <term><see cref="EnCarrito"/></term>
///         <description>El producto está en el carrito del usuario, sin comprar</description>
///     </item>
///     <item>
///         <term><see cref="Preparado"/></term>
///         <description>El pedido está siendo preparado por el vendedor/manager</description>
///     </item>
///     <item>
///         <term><see cref="Enviado"/></term>
///         <description>El pedido ha sido enviado al cliente</description>
///     </item>
///     <item>
///         <term><see cref="Recibido"/></term>
///         <description>El cliente ha recibido el pedido</description>
///     </item>
///     <item>
///         <term><see cref="Cancelado"/></term>
///         <description>El pedido ha sido cancelado (desde cualquier estado)</description>
///     </item>
/// </list>
/// </remarks>
public enum Status {
    /// <summary>
    /// El producto está en el carrito del usuario, sin completar la compra.
    /// </summary>
    /// <remarks>
    /// Este es el estado inicial cuando un usuario añade un producto a su carrito.
    /// El producto no está reservado ni comprometido hasta que se realiza el pago.
    /// </remarks>
    EnCarrito, 

    /// <summary>
    /// El pedido está siendo preparado por el vendedor/manager.
    /// </summary>
    /// <remarks>
    /// Indica que el manager ha aceptado o está procesando el pedido.
    /// En este estado se verifica el stock y se prepara para el envío.
    /// </remarks>
    Preparado, 

    /// <summary>
    /// El pedido ha sido enviado al cliente.
    /// </summary>
    /// <remarks>
    /// El producto está en tránsito hacia la dirección del cliente.
    /// Puede estar asociado a un número de seguimiento.
    /// </remarks>
    Enviado, 

    /// <summary>
    /// El cliente ha recibido el pedido correctamente.
    /// </summary>
    /// <remarks>
    /// Estado final exitoso. Indica que el pedido fue entregado
    /// y el cliente lo recibió conforme.
    /// </remarks>
    Recibido, 

    /// <summary>
    /// El pedido ha sido cancelado.
    /// </summary>
    /// <remarks>
    /// Puede cancelarse desde cualquier estado. Puede ser solicitado por:
    /// <list type="bullet">
    ///     <item>El cliente antes de recibir el producto</item>
    ///     <item>El manager por falta de stock</item>
    ///     <item>Sistema por timeout de pago</item>
    /// </list>
    /// </remarks>
    Cancelado
}