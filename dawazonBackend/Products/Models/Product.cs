using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using dawazonBackend.Common.Attribute;

namespace dawazonBackend.Products.Models;

public class Product
{
    [Key]
    [GenerateCustomIdAtribute]
    [StringLength(12)]
    public string? Id { get; set; }
    [Required]
    [StringLength(200,MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [Range(0,9999.99)]
    public double Price { get; set; }
    [Required]
    [Range(0,9999)]
    public int Stock { get; set; }
    [Required]
    [StringLength(300,MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;
    [Required]
    public long CreatorId { get; set; }
    [Required]
    public string CategoryId { get; set; }= string.Empty;
    public List<Comment> Comments { get; set; } = new();
    /// <summary>
    /// Propiedad de navegación a la categoría relacionada.
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }
    public List<string> Images = new List<string>();
    [Required]
    public bool IsDeleted { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [ConcurrencyCheck]
    public long Version { get; set; }

}