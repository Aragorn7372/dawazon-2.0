using System.ComponentModel.DataAnnotations;

namespace dawazonBackend.Products.Models;

public class Comment
{
    [Required]
    public int UserId { get; set; }
    [Required]
    [MaxLength(200)]
    [MinLength(2)]
    public String Content { get; set; }= String.Empty;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [Required]
    public bool verified { get; set; } = false;
    [Required]
    public bool recommended { get; set; } = false;
}