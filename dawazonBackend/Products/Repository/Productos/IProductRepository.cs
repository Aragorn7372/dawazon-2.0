using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Models;

namespace dawazonBackend.Products.Repository.Productos;

public interface IProductRepository
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetAllAsync(FilterDto filter);
    Task<List<Product>> GetAllByCreatedAtBetweenAsync(DateTime start, DateTime end);
    Task<int> SubstractStockAsync(string id, int amount, long version);
    Task<(IEnumerable<Product> Items, int TotalCount)> FindAllByCreatorId(long userId, FilterDto filter);
    Task DeleteByIdAsync(string id);
    Task<Product?> GetProductAsync(string id);
    Task<Product?> UpdateProductAsync(Product product, string id);
    Task<Product?> CreateProductAsync(Product product);
}