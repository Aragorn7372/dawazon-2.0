using dawazonBackend.Common.Database;
using dawazonBackend.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace dawazonBackend.Products.Repository.Categoria;

public class CategoryRepository(ILogger<CategoryRepository> logger,DawazonDbContext db): ICategoriaRepository
{
    public async Task<List<string>> GetCategoriesAsync()
    {
        logger.LogDebug($"Obteniendo categorías {nameof(CategoryRepository)}");
        return await db.Categorias.Select(c => c.Name).ToListAsync();
    }

    public async Task<Category?> GetCategoryAsync(string id)
    {
        logger.LogDebug($"Obteniendo categtía con id {id}");
        return await db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Category?> GetByNameAsync(string name)
    {
        logger.LogDebug($"Obteniendo categoría con nombre: {name}");
        return await db.Categorias
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }
}