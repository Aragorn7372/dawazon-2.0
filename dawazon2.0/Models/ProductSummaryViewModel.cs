namespace dawazon2._0.Models;

/// <summary>
/// ViewModel ligero para mostrar productos en tarjetas o listas.
/// </summary>
/// <remarks>
/// Proporciona información mínima necesaria para renderizar un producto en una lista.
/// 
/// <para><b>Utilizado en:</b></para>
/// <list type="bullet">
///     <item>ProductListViewModel.Products</item>
///     <item>Tarjetas de productos en el frontend</item>
///     <item>Listados de búsqueda</item>
/// </list>
/// </remarks>
public class ProductSummaryViewModel
{
    /// <summary>Identificador único del producto.</summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>Nombre del producto.</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Precio del producto.</summary>
    public double Price { get; set; }
    
    /// <summary>Cantidad disponible en stock.</summary>
    public int Stock { get; set; }
    
    /// <summary>Categoría a la que pertenece el producto.</summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>Primera imagen del producto, o vacío si no tiene.</summary>
    public string FirstImage { get; set; } = string.Empty;
    
    /// <summary>ID del manager que creó el producto.</summary>
    public long CreatorId { get; set; }
}
