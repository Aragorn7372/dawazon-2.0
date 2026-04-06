using dawazonBackend.Products.Models.Dto;

namespace dawazon2._0.Models
{
    /// <summary>
    /// ViewModel para contenido paginado de productos.
    /// </summary>
    /// <remarks>
    /// <para><b>Propiedades:</b></para>
    /// <list type="bullet">
    ///     <item>Content: Lista de productos</item>
    ///     <item>Pagination: TotalPages, PageNumber, First, Last</item>
    ///     <item>Sorting: SortBy, Direction</item>
    /// </list>
    /// </remarks>
public class ProductPageViewModel
{
    /// <summary>Lista de productos en la página actual.</summary>
    public List<Product> Content { get; set; } = new();
    
    /// <summary>Número total de páginas.</summary>
    public int TotalPages { get; set; }
    
    /// <summary>Número de página actual (0-indexed).</summary>
    public int PageNumber { get; set; }
    
    /// <summary>Indica si es la primera página.</summary>
    public bool First { get; set; }
    
    /// <summary>Indica si es la última página.</summary>
    public bool Last { get; set; }
    
    /// <summary>Cantidad de elementos en la página actual.</summary>
    public int TotalPageElements { get; set; }
    
    /// <summary>Total de elementos en todas las páginas.</summary>
    public long TotalElements { get; set; }
    
    /// <summary>Campo por el cual ordenar (default: Name).</summary>
    public string SortBy { get; set; } = "Name";
    
    /// <summary>Dirección de ordenación (asc/desc).</summary>
    public string Direction { get; set; } = "asc";
}

/// <summary>
/// Representa un producto en la lista paginada.
/// </summary>
/// <remarks>
/// Modelo ligero usado para renderizar productos en listas.
/// </remarks>
public class Product
{
    /// <summary>Identificador del producto.</summary>
    public int Id { get; set; }
    
    /// <summary>Nombre del producto.</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Precio del producto.</summary>
    public decimal Price { get; set; }
    
    /// <summary>Cantidad en stock.</summary>
    public int Stock { get; set; }
    
    /// <summary>Lista de URLs de imágenes del producto.</summary>
    public List<string> Images { get; set; } = new();
    
    /// <summary>
    /// Lista de comentarios asociados al producto.
    /// </summary>
    public List<CommentDto> Comments { get; set; } = new();
    
    /// <summary>ID del creador/manager del producto.</summary>
    public long CreatorId { get; set; }
}
}