using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Models;
using dawazonBackend.Common.Dto;

namespace dawazonBackend.Cart.Repository;

/// <summary>
/// Interfaz que define el contrato para el repositorio de carritos de compra.
/// </summary>
/// <remarks>
/// Esta interfaz sigue el patrón Repository y define todas las operaciones de acceso
/// a datos para la entidad <see cref="Models.Cart"/> y sus relacionados.
///
/// <para><b>Patrón de diseño:</b></para>
/// Utiliza el patrón Repository para abstraer la capa de acceso a datos.
/// Esto permite cambiar la implementación (Entity Framework, Dapper, etc.)
/// sin afectar la capa de servicio.
///
/// <para><b>Principios aplicados:</b></para>
/// <list type="bullet">
///     <item>Separation of Concerns: La interfaz define "qué" se hace, no "cómo"</item>
///     <item>Dependency Inversion: Los servicios dependen de esta abstracción</item>
///     <item>Single Responsibility: Solo maneja operaciones de Carrito</item>
/// </list>
/// 
/// <para><b>Implementación:</b></para>
/// Ver <see cref="CartRepository"/> para la implementación con Entity Framework Core.
/// </remarks>
public interface ICartRepository
{
    /// <summary>
    /// Obtiene todos los carritos filtrados de forma paginada.
    /// </summary>
    /// <param name="filter">Objeto <see cref="FilterCartDto"/> con los filtros, paginación y ordenación.</param>
    /// <returns>Tupla con: lista de carritos y total de elementos que cumplen el filtro.</returns>
    /// <remarks>
    /// Aplica filtros de búsqueda (por purchased, managerId, isAdmin), ordenación dinámica
    /// y paginación. Incluye las líneas de carrito y productos relacionados.
    /// </remarks>
    Task<(IEnumerable<Models.Cart> Items, int TotalCount)> GetAllAsync(FilterCartDto filter);    

    /// <summary>
    /// Actualiza el estado de una línea de carrito específica.
    /// </summary>
    /// <param name="id">Identificador del carrito.</param>
    /// <param name="productId">Identificador del producto.</param>
    /// <param name="status">Nuevo estado <see cref="Status"/> para la línea.</param>
    /// <returns>True si la actualización fue exitosa, false si no se encontró el carrito.</returns>
    Task<bool> UpdateCartLineStatusAsync(string id, string productId, Status status);
    
    /// <summary>
    /// Busca carritos asociados a un ID de usuario con filtros opcionales.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="filter">Filtros adicionales de búsqueda.</param>
    /// <returns>Enumerable de carritos que cumplen los criterios.</returns>
    /// <remarks>
    /// Nota: Este método actualmente lanza NotImplementedException.
    /// </remarks>
    Task<IEnumerable<Models.Cart>> FindByUserIdAsync(long userId, FilterCartDto filter);

    /// <summary>
    /// Añade una nueva línea de producto a un carrito o actualiza la cantidad si ya existe.
    /// </summary>
    /// <param name="cartId">Identificador del carrito.</param>
    /// <param name="cartLine">Línea de carrito a añadir.</param>
    /// <returns>True si la operación fue exitosa, false si el carrito no existe.</returns>
    /// <remarks>
    /// Si ya existe una línea con el mismo ProductId, se actualiza la cantidad.
    /// En caso contrario, se añade como nueva línea.
    /// </remarks>
    Task<bool> AddCartLineAsync(string cartId, CartLine cartLine);
    
    /// <summary>
    /// Elimina una línea de producto de un carrito.
    /// </summary>
    /// <param name="cartId">Identificador del carrito.</param>
    /// <param name="cartLine">Línea de carrito a eliminar.</param>
    /// <returns>True si la eliminación fue exitosa, false si no se encontró el carrito o la línea.</returns>
    Task<bool> RemoveCartLineAsync(string cartId, CartLine cartLine);
    
    /// <summary>
    /// Busca un carrito por usuario y estado de compra.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="purchased">Estado de compra (true = pedido completado, false = carrito activo).</param>
    /// <returns>El carrito encontrado o null si no existe.</returns>
    /// <remarks>
    /// Se utiliza principalmente para recuperar el carrito activo de un usuario.
    /// </remarks>
    Task<Models.Cart?> FindByUserIdAndPurchasedAsync(long userId, bool purchased);
    
    /// <summary>
    /// Busca un carrito por su identificador único.
    /// </summary>
    /// <param name="cartId">Identificador del carrito.</param>
    /// <returns>El carrito encontrado o null si no existe.</returns>
    /// <remarks>
    /// Carga automáticamente las líneas de carrito, productos y cliente asociado.
    /// </remarks>
    Task<Models.Cart?> FindCartByIdAsync(string cartId);
    
    /// <summary>
    /// Crea un nuevo carrito en la base de datos.
    /// </summary>
    /// <param name="cart">Objeto <see cref="Models.Cart"/> a crear.</param>
    /// <returns>El carrito creado con su ID generado.</returns>
    /// <remarks>
    /// El ID se genera automáticamente mediante el atributo [GenerateCustomIdAtribute].
    /// </remarks>
    Task<Models.Cart> CreateCartAsync(Models.Cart cart);
    
    /// <summary>
    /// Actualiza los datos de un carrito existente.
    /// </summary>
    /// <param name="id">Identificador del carrito a actualizar.</param>
    /// <param name="cart">Nuevo datos del carrito.</param>
    /// <returns>El carrito actualizado o null si no se encontró.</returns>
    /// <remarks>
    /// Solo actualiza los campos permitidos (client, totals, purchased, checkout state).
    /// No permite modificar las líneas directamente.
    /// </remarks>
    Task<Models.Cart?> UpdateCartAsync(string id, Models.Cart cart);

    /// <summary>
    /// Actualiza únicamente los campos Total y TotalItems de un carrito.
    /// </summary>
    /// <param name="cartId">Identificador del carrito.</param>
    /// <param name="totalItems">Nueva cantidad total de items.</param>
    /// <param name="total">Nuevo importe total.</param>
    /// <remarks>
    /// Optimización para actualizar solo los valores agregados sin tocar las líneas.
    /// </remarks>
    Task UpdateCartScalarsAsync(string cartId, int totalItems, double total);
    
    /// <summary>
    /// Elimina un carrito de la base de datos.
    /// </summary>
    /// <param name="id">Identificador del carrito a eliminar.</param>
    /// <returns>Task completo.</returns>
    /// <exception cref="Exceptions.CartNotFoundException">
    /// Se lanza si el carrito no existe.
    /// </exception>
    Task DeleteCartAsync(string id);

    /// <summary>
    /// Calcula las ganancias acumuladas de ventas.
    /// </summary>
    /// <param name="managerId">Filtrar por ID del manager/creador del producto (opcional).</param>
    /// <param name="isAdmin">Indica si el usuario es administrador.</param>
    /// <returns>Suma total de las ganancias.</returns>
    /// <remarks>
    /// Solo considera carritos comprados (Purchased = true).
    /// Si no es admin y no se proporciona managerId, retorna 0.
    /// </remarks>
    Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin);

    /// <summary>
    /// Obtiene las ventas proyectadas como líneas de pedido individuales.
    /// </summary>
    /// <param name="managerId">Filtrar por manager (opcional).</param>
    /// <param name="isAdmin">Indica si el usuario es administrador.</param>
    /// <param name="filter">Filtros de paginación.</param>
    /// <returns>Tupla con lista de SaleLineDto y total de elementos.</returns>
    /// <remarks>
    /// Aplana las líneas de carritos comprados para mostrarlas como ventas individuales.
    /// Incluye información del manager que creó el producto.
    /// </remarks>
    Task<(List<SaleLineDto> Items, int TotalCount)> GetSalesAsLinesAsync(long? managerId, bool isAdmin, FilterDto filter);

    /// <summary>
    /// Cuenta las nuevas líneas de productos vendidas de un manager desde una fecha específica.
    /// </summary>
    /// <param name="managerId">Identificador del manager.</param>
    /// <param name="since">Fecha desde la cual contar.</param>
    /// <returns>Número de ventas nuevas.</returns>
    /// <remarks>
    /// Solo considera líneas de carritos comprados cuya fecha de última
    /// modificación sea posterior a la fecha especificada.
    /// </remarks>
    Task<int> CountNewSalesAsync(long managerId, DateTime since);

    /// <summary>
    /// Obtiene el número total de ventas realizadas.
    /// </summary>
    /// <returns>Total de líneas en carritos comprados.</returns>
    /// <remarks>
    /// Cuenta todas las líneas de todos los carritos con Purchased = true.
    /// </remarks>
    Task<int> GetTotalSalesCountAsync();
}