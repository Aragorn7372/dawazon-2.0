using dawazonBackend.Cart.Models;

namespace dawazon2._0.Models;

/// <summary>
/// Línea de detalle de un pedido (producto comprado).
/// </summary>
public class CartLineViewModel
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double ProductPrice { get; set; }
    public double TotalPrice { get; set; }
    public Status Status { get; set; }

    public string StatusLabel => Status switch
    {
        Status.EnCarrito => "En carrito",
        Status.Preparado => "Preparado",
        Status.Enviado   => "Enviado",
        Status.Recibido  => "Recibido",
        Status.Cancelado => "Cancelado",
        _                => Status.ToString()
    };

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
