namespace dawazon2._0.Models;

/// <summary>
/// Datos completos de un pedido para la vista de detalle.
/// </summary>
public class CartOrderDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double Total { get; set; }
    public int TotalItems { get; set; }

    // Líneas de productos
    public List<CartLineViewModel> Lines { get; set; } = [];

    // Dirección de envío (datos del cliente)
    public string ClientName { get; set; } = string.Empty;
    public string ClientStreet { get; set; } = string.Empty;
    public int ClientNumber { get; set; }
    public string ClientCity { get; set; } = string.Empty;
    public int ClientPostalCode { get; set; }
    public string ClientProvince { get; set; } = string.Empty;
    public string ClientCountry { get; set; } = string.Empty;
}
