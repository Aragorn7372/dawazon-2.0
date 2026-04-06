using dawazonBackend.Cart.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para el listado de ventas en el panel de administración.
/// </summary>
/// <remarks>
/// Proporciona datos para renderizar el listado de ventas/líneas de pedido.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>SaleLineDto: Modelo de línea de venta del backend</item>
/// </list>
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Sales: Lista de ventas/líneas</item>
///     <item>Pagination: PageNumber, TotalPages, TotalElements, PageSize</item>
///     <item>TotalEarnings: Ganancias totales calculadas</item>
/// </list>
/// </remarks>
public class AdminSaleListViewModel
{
    /// <summary>Lista de ventas/líneas de pedido.</summary>
    public IEnumerable<SaleLineDto> Sales { get; set; } = new List<SaleLineDto>();
    
    /// <summary>Número de página actual (0-indexed).</summary>
    public int PageNumber { get; set; }
    
    /// <summary>Número total de páginas.</summary>
    public int TotalPages { get; set; }
    
    /// <summary>Total de ventas en todas las páginas.</summary>
    public long TotalElements { get; set; }
    
    /// <summary>Tamaño de cada página.</summary>
    public int PageSize { get; set; }
    
    /// <summary>Ganancias totales de todas las ventas.</summary>
    /// <remarks>
    /// Calculado en el backend.
    /// </remarks>
    public double TotalEarnings { get; set; }
}
