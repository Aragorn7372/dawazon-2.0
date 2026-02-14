using System.ComponentModel.DataAnnotations;
using dawazonBackend.Common.Attribute;

namespace dawazonBackend.Cart.Dto;

public record CartStockRequestDto
{
    public string? CartId { get; set; }

    public string UserId { get; set; } = string.Empty;
    
    public int Quantity { get; set; }

}