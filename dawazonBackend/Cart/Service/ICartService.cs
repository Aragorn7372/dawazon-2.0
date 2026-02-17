using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Common;
using dawazonBackend.Common.Dto;

namespace dawazonBackend.Cart.Service;

public interface ICartService
{
    Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin);

    Task<PageResponseDto<CartResponseDto>> FindAllAsync(long? userId, string purchased, FilterCartDto filter);

    Task<Result<CartResponseDto, DomainError>> AddProductAsync(string cartId, string productId);

    Task<CartResponseDto> RemoveProductAsync(string cartId, string productId);

    Task<Result<CartResponseDto, DomainError>> GetByIdAsync(string id);

    Task<CartResponseDto> SaveAsync(Models.Cart entity);

    Task SendConfirmationEmailAsync(Models.Cart pedido);

    Task<Result<CartResponseDto, DomainError>> UpdateStockWithValidationAsync(string cartId, string productId, int quantity);

    Task<Result<string, DomainError>> CheckoutAsync(string id);

    Task RestoreStockAsync(string cartId);

    Task DeleteByIdAsync(string id);

    Task<Result<CartResponseDto, DomainError>> GetCartByUserIdAsync(long userId);

    Task<DomainError?> CancelSaleAsync(string ventaId, string productId, long managerId, bool isAdmin);
}