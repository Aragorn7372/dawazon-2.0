using dawazonBackend.Products.Models.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la vista de detalle de un producto.
/// </summary>
/// <remarks>
/// Proporciona todos los datos necesarios para renderizar la página de detalle.
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Datos básicos: Id, Name, Price, Stock, Category, Description</item>
///     <item>Multimedia: Images (lista completa)</item>
///     <item>Interacción: Comments</item>
/// </list>
/// 
/// <para><b>Propiedades calculadas:</b></para>
/// <list type="bullet">
///     <item>MainImage: Primera imagen o placeholder</item>
/// </list>
/// </remarks>
public class ProductDetailViewModel
{
    /// <summary>Identificador único del producto.</summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>Nombre del producto.</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Precio del producto.</summary>
    public double Price { get; set; }
    
    /// <summary>Cantidad disponible en stock.</summary>
    public int Stock { get; set; }
    
    /// <summary>Categoría del producto.</summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>Descripción detallada del producto.</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Lista de URLs de todas las imágenes del producto.</summary>
    public List<string> Images { get; set; } = [];
    
    /// <summary>Lista de comentarios de usuarios sobre el producto.</summary>
    public List<CommentDto> Comments { get; set; } = [];

    /// <summary>
    /// URL de la primera imagen o placeholder.
    /// </summary>
    /// <remarks>
    /// Retorna la primera imagen de la lista, o "placeholder.png" si no hay imágenes.
    /// </remarks>
    public string MainImage => Images.FirstOrDefault() ?? "placeholder.png";
}
