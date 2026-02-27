namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la página de listado de productos con paginación y filtros.
/// </summary>
public class ProductListViewModel
{
    public List<ProductSummaryViewModel> Products { get; set; } = [];
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
    public bool First => PageNumber == 0;
    public bool Last => PageNumber >= TotalPages - 1;
    public long TotalElements { get; set; }
    public string? SearchName { get; set; }
    public string? SearchCategory { get; set; }
    public string SortBy { get; set; } = "id";
    public string Direction { get; set; } = "asc";
    public int PrevPage => Math.Max(0, PageNumber - 1);
    public int NextPage => Math.Min(TotalPages - 1, PageNumber + 1);
}
