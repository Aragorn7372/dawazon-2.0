using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Models;

namespace dawazonBackend.Cart.Repository;

public interface ICartRepository
{
    Task<bool> UpdateCartLineStatusAsync(string id, string productId, Status status);
    
    Task<IEnumerable<Models.Cart>> FindByUserIdAsync(long userId, FilterCartDto filter);

    Task<bool> AddCartLineAsync(string cartId, CartLine cartLine);
    
    Task<bool> RemoveCartLineAsync(string cartId, CartLine cartLine);
    
    Task<Models.Cart?> FindByUserIdAndPurchasedAsync(long userId, bool purchased);
    
    Task<Models.Cart?> FindCartByIdAsync(string cartId);
    
    Task<Models.Cart> CreateCartAsync(Models.Cart cart);
    
    Task<Models.Cart?> UpdateCartAsync(string id, Models.Cart cart);
    
    Task DeleteCartAsync(string id);
}