using dawazonBackend.Cart.Models;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para una línea de detalle de un pedido (producto comprado).
/// </summary>
/// <remarks>
/// Representa un producto específico dentro de un pedido con su estado.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>Status: Enum del backend</item>
/// </list>
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Producto: ProductId, ProductName, ManagerName</item>
///     <item>Cantidad: Quantity, ProductPrice, TotalPrice</item>
///     <item>Estado: Status</item>
/// </list>
/// 
/// <para><b>Propiedades calculadas:</b></para>
/// <list type="bullet">
///     <item>StatusLabel: Texto legible del estado</item>
///     <item>StatusBadgeClass: Clase CSS para el badge</item>
/// </list>
/// </remarks>
public class CartLineViewModel
{
    /// <summary>ID del producto.</summary>
    public string ProductId { get; set; } = string.Empty;
    
    /// <summary>Nombre del producto.</summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>Nombre del manager/vendedor.</summary>
    public string ManagerName { get; set; } = string.Empty;
    
    /// <summary>Cantidad comprada.</summary>
    public int Quantity { get; set; }
    
    /// <summary>Precio unitario del producto.</summary>
    public double ProductPrice { get; set; }
    
    /// <summary>Precio total (Quantity * ProductPrice).</summary>
    public double TotalPrice { get; set; }
    
    /// <summary>Estado actual de la línea del pedido.</summary>
    public Status Status { get; set; }

    /// <summary>
    /// Texto legible del estado del pedido.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    ///     <item><term>EnCarrito</term><description>En carrito</description></item>
    ///     <item><term>Preparado</term><description>Preparado</description></item>
    ///     <item><term>Enviado</term><description>Enviado</description></item>
    ///     <item><term>Recibido</term><description>Recibido</description></item>
    ///     <item><term>Cancelado</term><description>Cancelado</description></item>
    /// </list>
    /// </remarks>
    public string StatusLabel => Status switch
    {
        Status.EnCarrito => "En carrito",
        Status.Preparado => "Preparado",
        Status.Enviado   => "Enviado",
        Status.Recibido  => "Recibido",
        Status.Cancelado => "Cancelado",
        _                => Status.ToString()
    };

    /// <summary>
    /// Clase CSS para el badge del estado.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    ///     <item><term>EnCarrito</term><description>bg-secondary</description></item>
    ///     <item><term>Preparado</term><description>bg-warning text-dark</description></item>
    ///     <item><term>Enviado</term><description>bg-info text-dark</description></item>
    ///     <item><term>Recibido</term><description>bg-success</description></item>
    ///     <item><term>Cancelado</term><description>bg-danger</description></item>
    /// </list>
    /// </remarks>
    public string StatusBadgeClass => Status switch
    {
        Status.EnCarrito => "bg-secondary",
        Status.Preparado => "bg-warning text-dark",
        Status.Enviado   => "bg-info text-dark",
        Status.Recibido  => "bg-success",
        Status.Cancelado => "bg-danger",
        _                => "bg-secondary"
    };
}
