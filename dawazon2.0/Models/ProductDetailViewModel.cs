using dawazonBackend.Products.Models.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel con todos los datos de un producto para la vista de detalle.
/// </summary>
public class ProductDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Images { get; set; } = [];
    public List<CommentDto> Comments { get; set; } = [];

    /// <summary>URL de la primera imagen o placeholder.</summary>
    public string MainImage => Images.FirstOrDefault() ?? "placeholder.png";
}
