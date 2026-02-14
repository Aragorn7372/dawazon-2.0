using System.ComponentModel.DataAnnotations;
using dawazonBackend.Common.Attribute;

namespace dawazonBackend.Cart.Models;

public class Cart
{
    [Key]
    [GenerateCustomIdAtribute]
    public string Id { get; set; } = string.Empty;
    [Required]
    public long UserId {get; set;}
    [Required]
    public bool Purchased {get; set;}

    [Required] 
    public Client Client { get; set; } = new();

    [Required] 
    public List<CartLine> CartLines { get; set; } = new List<CartLine>();
    [Required]
    public int TotalItems {get; set;}
    [Required]
    public double Total {get; set;}
    [Required]
    public DateTime CreatedAt {get; set;}= DateTime.UtcNow;
    [Required]
    public DateTime UploadAt {get; set;}= DateTime.UtcNow;

    [Required] 
    public bool CheckoutInProgress { get; set; } = false;
    public DateTime? CheckoutStartedAt {get; set;} 
    public long GetMinutesSinceCheckoutStarted()
    {
        if (this.CheckoutStartedAt== null)
        {
            return 0;
        }
        return (long)(DateTime.UtcNow - CheckoutStartedAt.Value).TotalMinutes;
    }

    

}