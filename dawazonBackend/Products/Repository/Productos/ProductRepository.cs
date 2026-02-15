using System.Linq.Expressions;
using dawazonBackend.Common.Database;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Models;
using HotChocolate.Data.Sorting.Expressions;
using Microsoft.EntityFrameworkCore;

namespace dawazonBackend.Products.Repository.Productos;

public class ProductRepository(ILogger<ProductRepository> logger, DawazonDbContext db): IProductRepository
{
    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetAllAsync(FilterDto filter)
    {
        logger.LogDebug("GetAllAsync");
        var query= db.Products.Include(p => p.Category ).AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter?.Nombre))
            query = query.Where(p => p.Name.Contains(filter.Nombre));
        if (!string.IsNullOrWhiteSpace(filter?.Categoria))
            query = query.Where(p => p.Category!.Name.Contains(filter.Categoria));
        query=query.Where(p=>p.IsDeleted==false);
        var totalCount = await query.CountAsync();
        query= ApplySorting(query, filter!.SortBy, filter.Direction);
        var items= await query.Skip(filter.Page * filter.Size)
            .Take(filter.Size)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<List<Product>> GetAllByCreatedAtBetweenAsync(DateTime start, DateTime end)
    {
        logger.LogDebug("GetAllByCreatedAtBetweenAsync");
        return await db.Products.Include(p=>p.Category)
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end && p.IsDeleted==false).ToListAsync();
    }

    public async Task<int> SubstractStockAsync(string id, int amount, long version)
    {
        logger.LogDebug("SubstractStockAsync");
        var found = await db.Products.Where(p=>p.IsDeleted==false && p.Version==version&& p.Id==id).ToListAsync();
        if (found.FirstOrDefault() != null)
        {
            var product = found.First();
            product.Stock -= amount;
            if (product.Stock < 0)
            {
                // cambiar mas adelante
                throw new Exception($"Producto {id} no puede tener stock negativo");
            }
            var updated = db.Products.Update(product);
            await db.SaveChangesAsync();
            await db.Products.Entry(product).Reference(p => p.Category).LoadAsync();
            return 1;
        }
        return 0;
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> FindAllByCreatorId(long userId, FilterDto filter)
    {
        logger.LogDebug("FindAllByCreatorIdAsync");
        var query = db.Products.Include(p => p.Category).AsQueryable();
        query = query.Where(p => p.CreatorId == userId && p.IsDeleted == false);
        var totalCount = await query.CountAsync();
        query= ApplySorting(query,filter.SortBy, filter.Direction);
        
        var items = await query.Skip(filter.Page * filter.Size).Take(filter.Size).ToListAsync();
        return (items, totalCount);
    }

    public async Task DeleteByIdAsync(string id)
    {
        logger.LogDebug("DeleteByIdAsync");
        var found = await db.Products.Where(p=>p.IsDeleted==false &&  p.Id==id).ToListAsync();
        if (found.FirstOrDefault() == null) throw new Exception($"Producto {id} no existe");
        {
            var product = found.First();
            product.IsDeleted = true;
            var updated = db.Products.Update(product);
            await db.SaveChangesAsync();
            await db.Products.Entry(product).Reference(p => p.Category).LoadAsync();
        }
    }

    public Task<Product?> GetProductAsync(string id)
    {
        logger.LogDebug("GetProductAsync");
        return db.Products.Include(p => p.Category).Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);
    }

    public async Task<Product?> UpdateProductAsync(Product product, string id)
    {
        var productOld= await db.Products.Include(p => p.Category).Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);
        if (productOld == null) return null;
        productOld.Stock = product.Stock;
        productOld.UpdatedAt = DateTime.UtcNow;
        productOld.CategoryId = product.CategoryId;
        productOld.Name = product.Name;
        productOld.Description = product.Description;
        productOld.Price = product.Price;
        productOld.Category = product.Category;
        if (product.Comments.Count > 0)
            productOld.Comments = product.Comments;
        if (product.Images.Count > 0)
            productOld.Images = product.Images;
            
        var updated = db.Products.Update(productOld);
        await db.SaveChangesAsync();
        return updated.Entity;
    }

    public async Task<Product?> CreateProductAsync(Product product)
    {
        logger.LogInformation("Adding Product");
        var saved=await db.Products.AddAsync(product);
        await db.SaveChangesAsync();
        await db.Products.Entry(product).Reference(p => p.Category).LoadAsync();
        await db.Products.Entry(product).Collection(p =>p.Images).LoadAsync();
        await db.Products.Entry(product).Collection(p => p.Comments).LoadAsync();
        return saved.Entity;
    }
    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy, string direction)
    {
        var isDescending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        Expression<Func<Product,object>> keySelector = sortBy.ToLower() switch
        {
            "nombre" => p => p.Name,
            "precio" => p => p.Price,
            "createdat" => p => p.CreatedAt,
            "categoria" => p => p.Category!.Name,
            _ => p => p.Id!
        };
        return isDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
