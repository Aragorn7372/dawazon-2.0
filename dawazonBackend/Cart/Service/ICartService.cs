using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Common;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;

namespace dawazonBackend.Cart.Service;

public interface ICartService
{
    Task<PageResponseDto<SaleLineDto>> FindAllSalesAsLinesAsync(long? managerId, bool isAdmin, FilterDto filter);
    Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin);

    Task<PageResponseDto<CartResponseDto>> FindAllAsync(long? userId, bool purchased, FilterCartDto filter);

    Task<Result<CartResponseDto, DomainError>> AddProductAsync(string cartId, string productId);

    Task<CartResponseDto> RemoveProductAsync(string cartId, string productId);

    Task<Result<CartResponseDto, DomainError>> GetByIdAsync(string id);

    Task<Result<CartResponseDto, DomainError>> SaveAsync(Models.Cart entity);
    Task SendConfirmationEmailAsync(Models.Cart pedido);

    Task<Result<CartResponseDto, DomainError>> UpdateStockWithValidationAsync(string cartId, string productId, int quantity);

    Task<Result<string, DomainError>> CheckoutAsync(string id);

    Task RestoreStockAsync(string cartId);

    Task DeleteByIdAsync(string id);

    Task<Result<CartResponseDto, DomainError>> GetCartByUserIdAsync(long userId);

    Task<DomainError?> CancelSaleAsync(string ventaId, string productId, long? managerId, bool isAdmin);
}