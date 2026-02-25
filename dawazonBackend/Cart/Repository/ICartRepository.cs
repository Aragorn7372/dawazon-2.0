using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Models;
using dawazonBackend.Common.Dto;

namespace dawazonBackend.Cart.Repository;

/// <summary>
/// Interfaz para el repositorio de carritos de compra.
/// </summary>
public interface ICartRepository
{
    /// <summary>
    /// Obtiene todos los carritos filtrados de forma paginada.
    /// </summary>
    Task<(IEnumerable<Models.Cart> Items, int TotalCount)> GetAllAsync(FilterCartDto filter);    

    /// <summary>
    /// Actualiza el estado de una línea de carrito específica.
    /// </summary>
    Task<bool> UpdateCartLineStatusAsync(string id, string productId, Status status);
    
    /// <summary>
    /// Busca carritos asociados a un ID de usuario.
    /// </summary>
    Task<IEnumerable<Models.Cart>> FindByUserIdAsync(long userId, FilterCartDto filter);

    /// <summary>
    /// Añade una nueva línea de producto a un carrito.
    /// </summary>
    Task<bool> AddCartLineAsync(string cartId, CartLine cartLine);
    
    /// <summary>
    /// Elimina una línea de producto de un carrito.
    /// </summary>
    Task<bool> RemoveCartLineAsync(string cartId, CartLine cartLine);
    
    /// <summary>
    /// Busca un carrito por usuario y estado de compra (usado para recuperar el carrito activo).
    /// </summary>
    Task<Models.Cart?> FindByUserIdAndPurchasedAsync(long userId, bool purchased);
    
    /// <summary>
    /// Busca un carrito por su identificador único.
    /// </summary>
    Task<Models.Cart?> FindCartByIdAsync(string cartId);
    
    /// <summary>
    /// Crea un nuevo carrito en la base de datos.
    /// </summary>
    Task<Models.Cart> CreateCartAsync(Models.Cart cart);
    
    /// <summary>
    /// Actualiza los datos de un carrito existente.
    /// </summary>
    Task<Models.Cart?> UpdateCartAsync(string id, Models.Cart cart);

    /// <summary>
    /// Actualiza únicamente los campos Total y TotalItems de un carrito,
    /// sin tocar las líneas de carrito
    /// </summary>
    Task UpdateCartScalarsAsync(string cartId, int totalItems, double total);
    
    /// <summary>
    /// Elimina un carrito de la base de datos.
    /// </summary>
    Task DeleteCartAsync(string id);

    /// <summary>
    /// Calcula las ganancias acumuladas filtrando opcionalmente por gestor.
    /// </summary>
    Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin);

    /// <summary>
    /// Obtiene las ventas proyectadas como líneas de pedido individuales.
    /// </summary>
    Task<(List<SaleLineDto> Items, int TotalCount)> GetSalesAsLinesAsync(long? managerId, bool isAdmin, FilterDto filter);
}