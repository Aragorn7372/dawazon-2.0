namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la página de listado de pedidos con paginación.
/// </summary>
public class CartOrderListViewModel
{
    public List<CartOrderSummaryViewModel> Orders { get; set; } = [];

    // Paginación
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
    public long TotalElements { get; set; }

    public bool First => PageNumber == 0;
    public bool Last  => PageNumber >= TotalPages - 1;
    public int PrevPage => Math.Max(0, PageNumber - 1);
    public int NextPage => Math.Min(TotalPages - 1, PageNumber + 1);
}
