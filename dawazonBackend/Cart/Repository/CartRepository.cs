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
        cart.CartLines.Remove(cartLine);
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
        oldCart.CartLines=cart.CartLines;
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