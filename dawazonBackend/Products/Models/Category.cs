using System.ComponentModel.DataAnnotations;
using dawazonBackend.Common.Attribute;

namespace dawazonBackend.Products.Models;

public class Category
{
    [Key]
    [GenerateCustomIdAtribute]
    public string Id { get; set; }= string.Empty;
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}