namespace dawazon2._0.Models;

/// <summary>
/// Resumen de un pedido para la vista de listado "Mis Pedidos".
/// </summary>
public class CartOrderSummaryViewModel
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double Total { get; set; }
    public int TotalItems { get; set; }

    // Datos del cliente para mostrar en la columna "Enviar a"
    public string ClientName { get; set; } = string.Empty;
    public string ClientCity { get; set; } = string.Empty;
    public int ClientPostalCode { get; set; }
}
