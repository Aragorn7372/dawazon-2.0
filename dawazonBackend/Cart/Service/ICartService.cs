using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Common;
using dawazonBackend.Common.Dto;

namespace dawazonBackend.Cart.Service;

public interface ICartService
{
    Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin);

    Task<PageResponseDto<Models.Cart>> FindAllAsync(long? userId, string purchased, FilterCartDto filter);

    Task<Result<Models.Cart, DomainError>> AddProductAsync(string cartId, string productId);

    Task<Models.Cart> RemoveProductAsync(string cartId, string productId);

    Task<Result<Models.Cart, DomainError>> GetByIdAsync(string id);

    Task<Models.Cart> SaveAsync(Models.Cart entity);

    Task SendConfirmationEmailAsync(Models.Cart pedido);

    Task<Result<Models.Cart, DomainError>> UpdateStockWithValidationAsync(string cartId, string productId, int quantity);

    Task<Result<string, DomainError>> CheckoutAsync(string id);

    Task RestoreStockAsync(string cartId);

    Task DeleteByIdAsync(string id);

    Task<Result<Models.Cart, DomainError>> GetCartByUserIdAsync(long userId);

    Task<DomainError?> CancelSaleAsync(string ventaId, string productId, long managerId, bool isAdmin);
}