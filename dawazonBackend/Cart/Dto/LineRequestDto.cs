using dawazonBackend.Cart.Models;

namespace dawazonBackend.Cart.Dto;

public class LineRequestDto
{
    public string CartId {get; set;} = string.Empty;
    public string ProductId {get; set;} = string.Empty;
    public Status Status { get; set; } = new();
}