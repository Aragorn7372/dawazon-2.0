using dawazonBackend.Cart.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// Modelo de vista para el listado de ventas en la administración.
/// </summary>
public class AdminSaleListViewModel
{
    public IEnumerable<SaleLineDto> Sales { get; set; } = new List<SaleLineDto>();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public long TotalElements { get; set; }
    public int PageSize { get; set; }
    
    // Total earnings calculated in the background
    public double TotalEarnings { get; set; }
}
