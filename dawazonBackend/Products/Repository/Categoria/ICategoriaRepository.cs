using dawazonBackend.Products.Models;

namespace dawazonBackend.Products.Repository.Categoria;

public interface ICategoriaRepository
{
    Task<List<String>> GetCategoriesAsync();
    Task<Category?> GetCategoryAsync(string id);
    Task<Category?> GetByNameAsync(string name);
}