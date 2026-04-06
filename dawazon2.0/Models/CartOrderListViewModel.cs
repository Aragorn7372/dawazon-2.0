namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la página de listado de pedidos con paginación.
/// </summary>
/// <remarks>
/// Proporciona datos para renderizar la lista de pedidos del usuario.
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Orders: Lista de resúmenes de pedidos</item>
///     <item>Pagination: TotalPages, PageNumber, TotalElements</item>
/// </list>
/// 
/// <para><b>Propiedades calculadas:</b></para>
/// <list type="bullet">
///     <item>First/Last: Primera/última página</item>
///     <item>PrevPage/NextPage: Página anterior/siguiente</item>
/// </list>
/// </remarks>
public class CartOrderListViewModel
{
    /// <summary>Lista de resúmenes de pedidos.</summary>
    public List<CartOrderSummaryViewModel> Orders { get; set; } = [];

    /// <summary>Número total de páginas.</summary>
    public int TotalPages { get; set; }
    
    /// <summary>Número de página actual (0-indexed).</summary>
    public int PageNumber { get; set; }
    
    /// <summary>Total de pedidos en todas las páginas.</summary>
    public long TotalElements { get; set; }

    /// <summary>Indica si es la primera página.</summary>
    public bool First => PageNumber == 0;
    
    /// <summary>Indica si es la última página.</summary>
    public bool Last  => PageNumber >= TotalPages - 1;
    
    /// <summary>Número de la página anterior.</summary>
    public int PrevPage => Math.Max(0, PageNumber - 1);
    
    /// <summary>Número de la siguiente página.</summary>
    public int NextPage => Math.Min(TotalPages - 1, PageNumber + 1);
}
