using System.ComponentModel.DataAnnotations;

namespace dawazon2._0.Models;

public class AddCommentViewModel
{
    public string ProductId { get; set; }=string.Empty;

    [Required]
    public string CommentText { get; set; }=string.Empty;

    [Required]
    public bool Recommended { get; set; }
}