namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la página de listado de productos.
/// </summary>
/// <remarks>
/// Proporciona datos para renderizar la lista de productos con paginación y filtros.
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Products: Lista de productos a mostrar</item>
///     <item>Pagination: TotalPages, PageNumber, TotalElements</item>
///     <item>Filters: SearchName, SearchCategory</item>
///     <item>Sorting: SortBy, Direction</item>
/// </list>
/// 
/// <para><b>Utilizado por:</b></para>
/// <list type="bullet">
///     <item>ProductsMvcController.Index</item>
///     <item>Vistas Razor de listado</item>
/// </list>
/// </remarks>
public class ProductListViewModel
{
    /// <summary>Lista de productos a mostrar en la página actual.</summary>
    public List<ProductSummaryViewModel> Products { get; set; } = [];
    
    /// <summary>Número total de páginas disponibles.</summary>
    public int TotalPages { get; set; }
    
    /// <summary>Número de página actual (0-indexed).</summary>
    public int PageNumber { get; set; }
    
    /// <summary>Indica si es la primera página.</summary>
    public bool First => PageNumber == 0;
    
    /// <summary>Indica si es la última página.</summary>
    public bool Last => PageNumber >= TotalPages - 1;
    
    /// <summary>Total de elementos en todas las páginas.</summary>
    public long TotalElements { get; set; }
    
    /// <summary>Filtro de búsqueda por nombre de producto.</summary>
    public string? SearchName { get; set; }
    
    /// <summary>Filtro de búsqueda por categoría.</summary>
    public string? SearchCategory { get; set; }
    
    /// <summary>Campo por el cual ordenar (default: id).</summary>
    public string SortBy { get; set; } = "id";
    
    /// <summary>Dirección de ordenación (asc/desc).</summary>
    public string Direction { get; set; } = "asc";
    
    /// <summary>Número de la página anterior.</summary>
    public int PrevPage => Math.Max(0, PageNumber - 1);
    
    /// <summary>Número de la siguiente página.</summary>
    public int NextPage => Math.Min(TotalPages - 1, PageNumber + 1);
}
