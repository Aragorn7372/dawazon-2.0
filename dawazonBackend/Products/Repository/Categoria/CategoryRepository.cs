using dawazonBackend.Common.Database;
using dawazonBackend.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace dawazonBackend.Products.Repository.Categoria;

public class CategoryRepository(ILogger<CategoryRepository> logger,DawazonDbContext db): ICategoriaRepository
{
    public async Task<List<string>> GetCategoriesAsync()
    {
        logger.LogDebug($"Getting categories for {nameof(CategoryRepository)}");
        return await db.Categorias.Select(c => c.Name).ToListAsync();
    }

    public async Task<Category?> GetCategoryAsync(string id)
    {
        logger.LogDebug($"Getting categorie with id {id}");
        return await db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Category?> GetByNameAsync(string name)
    {
        logger.LogDebug($"Getting category by name: {name}");
        return await db.Categorias
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }
}