using System.Linq.Expressions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Models;
using dawazonBackend.Common.Database;
using dawazonBackend.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace dawazonBackend.Cart.Repository;

public class CartRepository(
    DawazonDbContext context,
    ILogger<CartRepository> logger
    ): ICartRepository
{
    public async Task<(IEnumerable<Models.Cart> Items, int TotalCount)> GetAllAsync(FilterCartDto filter)
    {
        var query = context.Carts.AsQueryable();

        // Aplicar filtros si vienen en el DTO
        if (!string.IsNullOrEmpty(filter.purchased) && bool.TryParse(filter.purchased, out bool isPurchased))
        {
            query = query.Where(c => c.Purchased == isPurchased);
        }

        // OBTENEMOS EL TOTAL ANTES DE PAGINAR
        var totalCount = await query.CountAsync();

        // Ordenación
        bool isDesc = filter.Direction.ToLower() == "desc";

        query = filter.SortBy.ToLower() switch
        {
            "total" => isDesc ? query.OrderByDescending(c => c.Total) : query.OrderBy(c => c.Total),
            "createdat" => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            "userid" => isDesc ? query.OrderByDescending(c => c.UserId) : query.OrderBy(c => c.UserId),
            _ => isDesc ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id)
        };

        // Paginación
        int skip = filter.Page * filter.Size;

        var items = await query
            .Skip(skip)
            .Take(filter.Size)
            .ToListAsync();

        return (items, totalCount);
    }
    
    public async Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin)
    {
        logger.LogInformation($"Calculando ganancias totales - Manager: {managerId}, isAdmin: {isAdmin}");

        // Si no es admin y no manda managerId, no debe ver nada
        if (!isAdmin && !managerId.HasValue)
        {
            return 0;
        }

        // Navegamos de Carritos Comprados -> Líneas de Carrito
        var query = context.Carts
            .Where(c => c.Purchased == true)
            .SelectMany(c => c.CartLines)
            .AsQueryable();

        // Si hay managerId, filtramos por sus productos
        if (managerId.HasValue)
        {
            query = query.Where(cl => cl.Product != null && cl.Product.CreatorId == managerId.Value);
        }

        // Sumamos calculando Precio * Cantidad directamente en la BBDD. 
        // No usamos la propiedad calculada TotalPrice porque EF Core no la puede traducir a SQL.
        return await query.SumAsync(cl => cl.ProductPrice * cl.Quantity);
    }
    public async Task<bool> UpdateCartLineStatusAsync(string id, string productId, Status status)
    {
        logger.LogInformation($"Actualizando linea de carrito {id} con status {status}");
        
        var oldCart = await context.Carts.Include(c => c.CartLines)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (oldCart == null) return false;
        
        oldCart.CartLines.Find(cl => cl.ProductId == productId)!.Status = status;
        context.Carts.Update(oldCart);
        await context.SaveChangesAsync();
        
        return true;
    }

    public Task<IEnumerable<Models.Cart>> FindByUserIdAsync(long userId, FilterCartDto filter)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> AddCartLineAsync(string cartId, CartLine cartLine)
    {
        logger.LogInformation($"Añadiendo línea de carrito al carrito con ID: {cartId}");
    
        var cart = await context.Carts.Include(c => c.CartLines)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    
        if(cart == null) return false;

        var existingLine = cart.CartLines
            .FirstOrDefault(cl => cl.ProductId == cartLine.ProductId);

        if (existingLine != null) existingLine.Quantity = cartLine.Quantity; 
        
        else cart.CartLines.Add(cartLine);
        
        context.Carts.Update(cart);
        await context.SaveChangesAsync();
    
        return true;
    }

    public async Task<bool> RemoveCartLineAsync(string cartId, CartLine cartLine)
    {
        logger.LogInformation($"Deletando linea de carrito {cartId}");
        var cart = await context.Carts.Include(c => c.CartLines)
            .FirstOrDefaultAsync(c => c.Id == cartId);
        if(cart == null) return false;
        var lineToRemove = cart.CartLines.FirstOrDefault(cl => cl.ProductId == cartLine.ProductId);
        if (lineToRemove != null) cart.CartLines.Remove(lineToRemove);        
        context.Carts.Update(cart);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Models.Cart?> FindByUserIdAndPurchasedAsync(long userId, bool purchased)
    {
        logger.LogInformation($"bucando carrito con  ID: {userId} y estatus {purchased}");
        return await context.Carts.Include(c => c.CartLines).Include(c => c.Client)
            .ThenInclude(cl => cl.Address).FirstOrDefaultAsync(c => c.UserId == userId && c.Purchased == purchased);
    }

    public async Task<Models.Cart?> FindCartByIdAsync(string cartId)
    {
        logger.LogInformation($"cartId: {cartId}");
        return await context.Carts.Include(c => c.CartLines).Include(c => c.Client)
            .ThenInclude(cl => cl.Address).FirstOrDefaultAsync(c => c.Id == cartId);
    }

    public async Task<Models.Cart> CreateCartAsync(Models.Cart cart)
    {
        logger.LogInformation($"creando carrito");
        var saved=await context.Carts.AddAsync(cart);
        await context.SaveChangesAsync();
        await context.Carts.Entry(cart).Reference(c=> c.Client).LoadAsync();
        await context.Carts.Entry(cart).Collection(c => c.CartLines).LoadAsync();
        await context.Entry(cart.Client).Reference(cl => cl.Address).LoadAsync();
        return saved.Entity;
    }

    public async Task<Models.Cart?> UpdateCartAsync(string id, Models.Cart cart)
    {
        var oldCart = await context.Carts.Include(c => c.CartLines).Include(c => c.Client)
            .ThenInclude(cl => cl.Address).FirstOrDefaultAsync(c => c.Id == id);
        if (oldCart == null) return null;
        oldCart.Client=cart.Client;
        oldCart.CartLines.Clear();
        oldCart.CartLines.AddRange(cart.CartLines);
        oldCart.Total=cart.Total;
        oldCart.TotalItems=cart.TotalItems;
        oldCart.Purchased=cart.Purchased;
        oldCart.CheckoutInProgress=cart.CheckoutInProgress;
        oldCart.CheckoutStartedAt=cart.CheckoutStartedAt;
        oldCart.UploadAt= DateTime.UtcNow;
        var saved=context.Carts.Update(oldCart);
        await context.SaveChangesAsync();
        return saved.Entity;
    }

    public async Task DeleteCartAsync(string id)
    {
        var cart = await context.Carts.FindAsync(id);

        if (cart == null)
            throw new Exception("No se encontro carrito");

        context.Carts.Remove(cart);

        await context.SaveChangesAsync();
    }
    private static IQueryable<Models.Cart> ApplySorting(IQueryable<Models.Cart> query, string sortBy, string direction)
    {
        var isDescending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        Expression<Func<Models.Cart,object>> keySelector = sortBy.ToLower() switch
        {
            "Comprado" => p => p.Purchased,
            "precio" => p => p.Total,
            "createdat" => p => p.CreatedAt,
            "ultima modificacion" => p => p.UploadAt,
            _ => p => p.Id!
        };
        return isDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}