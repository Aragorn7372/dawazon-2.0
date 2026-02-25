using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Common;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;

namespace dawazonBackend.Cart.Service;

/// <summary>
/// Interfaz para el servicio de gestión de carritos de compra y ventas.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Obtiene todas las líneas de venta filtradas para administradores o gestores.
    /// </summary>
    Task<PageResponseDto<SaleLineDto>> FindAllSalesAsLinesAsync(long? managerId, bool isAdmin, FilterDto filter);

    /// <summary>
    /// Calcula las ganancias totales totales para administradores o gestores.
    /// </summary>
    Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin);

    /// <summary>
    /// Busca carritos filtrados por usuario y estado de compra.
    /// </summary>
    Task<PageResponseDto<CartResponseDto>> FindAllAsync(long? userId, bool purchased, FilterCartDto filter);

    /// <summary>
    /// Añade un producto al carrito especificado.
    /// </summary>
    Task<Result<CartResponseDto, DomainError>> AddProductAsync(string cartId, string productId);

    /// <summary>
    /// Elimina un producto del carrito.
    /// </summary>
    Task<CartResponseDto> RemoveProductAsync(string cartId, string productId);

    /// <summary>
    /// Obtiene un carrito por su ID (devuelve el DTO).
    /// </summary>
    Task<Result<CartResponseDto, DomainError>> GetByIdAsync(string id);

    /// <summary>
    /// Obtiene el modelo de dominio completo de un carrito por su ID.
    /// Útil para operaciones que requieren la entidad (SaveAsync, SendConfirmationEmailAsync).
    /// </summary>
    Task<Models.Cart?> GetCartModelByIdAsync(string id);

    /// <summary>
    /// Guarda o actualiza una entidad de carrito.
    /// </summary>
    Task<Result<CartResponseDto, DomainError>> SaveAsync(Models.Cart entity);

    /// <summary>
    /// Envía un correo de confirmación de pedido al cliente.
    /// </summary>
    Task SendConfirmationEmailAsync(Models.Cart pedido);

    /// <summary>
    /// Actualiza la cantidad de un producto en el carrito validando el stock.
    /// </summary>
    Task<Result<CartResponseDto, DomainError>> UpdateStockWithValidationAsync(string cartId, string productId, int quantity);

    /// <summary>
    /// Procesa el pago y finaliza la compra del carrito.
    /// </summary>
    Task<Result<string, DomainError>> CheckoutAsync(string id);

    /// <summary>
    /// Restaura el stock de los productos de un carrito (ej. en caso de cancelación).
    /// </summary>
    Task RestoreStockAsync(string cartId);

    /// <summary>
    /// Elimina un carrito por su ID.
    /// </summary>
    Task DeleteByIdAsync(string id);

    /// <summary>
    /// Obtiene el carrito activo (no comprado) de un usuario.
    /// </summary>
    Task<Result<CartResponseDto, DomainError>> GetCartByUserIdAsync(long userId);

    /// <summary>
    /// Cancela una venta de un producto específico en un pedido.
    /// </summary>
    Task<DomainError?> CancelSaleAsync(string ventaId, string productId, long? managerId, bool isAdmin);

    /// <summary>
    /// Actualiza el estado de una venta.
    /// </summary>
    Task<DomainError?> UpdateSaleStatusAsync(string ventaId, string productId, Models.Status newStatus, long? managerId, bool isAdmin);
     /// <summary>
    /// Cuenta las nuevas ventas de un gestor desde una fecha determinada.
    /// </summary>
    Task<Result<int, DomainError>> GetNewSalesCountAsync(long managerId, DateTime since);

    /// <summary>
    /// Obtiene el número total de ventas realizadas.
    /// </summary>
    Task<int> GetTotalSalesCountAsync();

    /// <summary>
    /// Limpia los carritos con checkout iniciado hace más de <paramref name="expirationMinutes"/> minutos
    /// restaurando el stock de sus productos y restableciendo el estado de checkout.
    /// </summary>
    Task CleanupExpiredCheckoutsAsync(int expirationMinutes = 5);
}