using dawazonBackend.Cart.Models;
namespace dawazonBackend.Cart.Dto;

/// <summary>
/// Data Transfer Object (DTO) que representa una línea de venta dentro de un carrito de compras.
/// </summary>
/// <remarks>
/// Este DTO contiene información detallada sobre un producto específico incluido en un carrito,
/// incluyendo precios, cantidades, estado del pedido, y datos del cliente y manager asociados.
///
/// <para><b>Ciclo de vida de una línea de carrito:</b></para>
/// <list type="number">
///     <item><see cref="Status.EnCarrito"/> - El producto está en el carrito del usuario</item>
///     <item><see cref="Status.Preparado"/> - El pedido está siendo preparado por el vendedor</item>
///     <item><see cref="Status.Enviado"/> - El pedido ha sido enviado al cliente</item>
///     <item><see cref="Status.Recibido"/> - El cliente ha recibido el pedido</item>
///     <item><see cref="Status.Cancelado"/> - El pedido ha sido cancelado</item>
/// </list>
/// 
/// <para><b>Uso típico:</b></para>
/// Se utiliza como elemento dentro de la colección <see cref="CartResponseDto.CartLines"/>
/// para mostrar el contenido completo de un carrito de compras.
/// </remarks>
public class SaleLineDto
{
    /// <summary>
    /// Identificador único de la venta o línea de pedido.
    /// </summary>
    /// <value>String con el identificador de la venta (ejemplo: "SALE-001").</value>
    /// <remarks>
    /// Este campo relaciona la línea con la venta padre en el sistema.
    /// </remarks>
    public string SaleId { get; set; } = string.Empty;
    
    /// <summary>
    /// Identificador del producto en el catálogo.
    /// </summary>
    /// <value>String con el identificador del producto (ejemplo: "PROD-123").</value>
    /// <remarks>
    /// Se utiliza para relacionar la línea con el producto en el sistema de inventario.
    /// </remarks>
    public string ProductId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del producto en el momento de la compra.
    /// </summary>
    /// <value>Nombre descriptivo del producto.</value>
    /// <remarks>
    /// Se almacena una copia del nombre para mantener historial de compras
    /// incluso si el producto es modificado o eliminado posteriormente.
    /// </remarks>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Cantidad de unidades del producto en esta línea.
    /// </summary>
    /// <value>Número entero positivo representando las unidades.</value>
    /// <remarks>
    /// Debe ser mayor que 0. Se utiliza para calcular el precio total
    /// y para gestionar el inventario.
    /// </remarks>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Precio unitario del producto en el momento de la compra.
    /// </summary>
    /// <value>Precio decimal del producto (ejemplo: 19.99).</value>
    /// <remarks>
    /// Se almacena el precio en el momento de añadir al carrito para mantener
    /// el precio acordado aunque el producto cambie de precio posteriormente.
    /// </remarks>
    public double ProductPrice { get; set; }
    
    /// <summary>
    /// Precio total de la línea (Precio unitario × Cantidad).
    /// </summary>
    /// <value>Total decimal de la línea (ejemplo: 39.98).</value>
    /// <remarks>
    /// Se calcula como ProductPrice × Quantity. Se almacena para evitar
    /// recalcular y para mantener historial de precios.
    /// </remarks>
    public double TotalPrice { get; set; }
    
    /// <summary>
    /// Estado actual del pedido para esta línea.
    /// </summary>
    /// <value>Enumeración <see cref="Status"/> indicando el estado del ciclo de vida.</value>
    /// <remarks>
    /// Representa el estado de procesamiento del pedido. Permite hacer seguimiento
    /// desde que el cliente añade el producto hasta que lo recibe o cancela.
    /// </remarks>
    public Status Status { get; set; }
    
    /// <summary>
    /// Identificador del manager o vendedor que procesa el pedido.
    /// </summary>
    /// <value>Identificador numérico del usuario manager.</value>
    /// <remarks>
    /// Se asigna cuando un manager asume la gestión del pedido.
    /// </remarks>
    public long ManagerId { get; set; }
    
    /// <summary>
    /// Nombre del manager o vendedor que procesa el pedido.
    /// </summary>
    /// <value>Nombre del manager asignado.</value>
    /// <remarks>
    /// Se almacena para mostrar en la interfaz de administración y en histórico.
    /// </remarks>
    public string ManagerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Objeto cliente asociado a esta línea de venta.
    /// </summary>
    /// <value>Instancia de <see cref="Client"/> con los datos del cliente.</value>
    /// <remarks>
    /// Contiene la información de envío y contacto del cliente para esta línea.
    /// Se inicializa con una instancia vacía por defecto.
    /// </remarks>
    public Client Client { get; set; } = new();
    
    /// <summary>
    /// Identificador del usuario propietario del carrito.
    /// </summary>
    /// <value>Identificador numérico del usuario.</value>
    /// <remarks>
    /// Relaciona la línea con el usuario que inició la compra.
    /// </remarks>
    public long UserId { get; set; }
    
    /// <summary>
    /// Fecha y hora de creación de la línea de venta.
    /// </summary>
    /// <value>Fecha en formato UTC.</value>
    /// <remarks>
    /// Se establece cuando el producto se añade al carrito por primera vez.
    /// </remarks>
    public DateTime CreateAt {get; set;}
    
    /// <summary>
    /// Fecha y hora de última modificación de la línea de venta.
    /// </summary>
    /// <value>Fecha en formato UTC.</value>
    /// <remarks>
    /// Se actualiza cada vez que se modifica la línea (cambio de cantidad, estado, etc.).
    /// </remarks>
    public DateTime UpdateAt {get; set;}
    
    /// <summary>
    /// Obtiene el nombre del cliente asociado a esta línea de venta.
    /// </summary>
    /// <returns>Nombre del cliente o cadena vacía si no hay cliente asignado.</returns>
    /// <remarks>
    /// Método de conveniencia que accede a la propiedad <see cref="Client"/>
    /// para obtener el nombre del cliente de forma segura.
    /// </remarks>
    public string GetUserName(){return Client.Name;}
    
    /// <summary>
    /// Obtiene el correo electrónico del cliente asociado a esta línea de venta.
    /// </summary>
    /// <returns>Email del cliente o cadena vacía si no hay cliente asignado.</returns>
    /// <remarks>
    /// Método de conveniencia que accede a la propiedad <see cref="Client"/>
    /// para obtener el email del cliente de forma segura.
    /// </remarks>
    public string GetUserEmail(){return Client.Email;}
}