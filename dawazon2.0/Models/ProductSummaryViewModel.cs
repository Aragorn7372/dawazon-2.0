namespace dawazon2._0.Models;

/// <summary>
/// ViewModel ligero para mostrar cada producto en la lista (tarjetas).
/// </summary>
public class ProductSummaryViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    /// <summary>Primera imagen del producto, o vacío si no tiene.</summary>
    public string FirstImage { get; set; } = string.Empty;
    /// <summary>ID del manager que creó el producto.</summary>
    public long CreatorId { get; set; }
}
