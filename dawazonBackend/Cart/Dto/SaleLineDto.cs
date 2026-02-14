using dawazonBackend.Cart.Models;
namespace dawazonBackend.Cart.Dto;

public class SaleLineDto
{
    public string SaleId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double ProductPrice { get; set; }
    public double TotalPrice { get; set; }
    public Status Status { get; set; }
    public long ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public Client Client { get; set; } = new();
    public long UserId { get; set; }
    public DateTime CreateAt {get; set;}
    public DateTime UpdateAt {get; set;}
    public string GetUserName(){return Client.Name;}
    public string GetUserEmail(){return Client.Email;}
}