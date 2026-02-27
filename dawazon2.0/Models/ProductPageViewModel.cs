using dawazonBackend.Products.Models.Dto;

namespace dawazon2._0.Models
{
    public class ProductPageViewModel
    {
        public List<Product> Content { get; set; } = new();
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public bool First { get; set; }
        public bool Last { get; set; }
        public int TotalPageElements { get; set; }
        public long TotalElements { get; set; }
        public string SortBy { get; set; } = "Name";
        public string Direction { get; set; } = "asc";
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public List<string> Images { get; set; } = new();
        public List<CommentDto> Comments { get; set; } = new();
        public long CreatorId { get; set; }
    }
}