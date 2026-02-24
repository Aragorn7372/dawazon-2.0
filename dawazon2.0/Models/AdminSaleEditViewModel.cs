using System.ComponentModel.DataAnnotations;
using dawazonBackend.Cart.Models;

namespace dawazon2._0.Models;

/// <summary>
/// Modelo de vista para la edición del estado de una venta (línea de pedido).
/// </summary>
public class AdminSaleEditViewModel
{
    public string SaleId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double TotalPrice { get; set; }
    
    public Status CurrentStatus { get; set; }

    [Required(ErrorMessage = "El nuevo estado es obligatorio")]
    public Status NewStatus { get; set; }
    
    public string? Notes { get; set; }
}
