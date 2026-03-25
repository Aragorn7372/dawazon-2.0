using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Common;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;

namespace dawazonBackend.Cart.Service;

/// <summary>
/// Interfaz que define el contrato para el servicio de gestión de carritos de compra, 
/// ventas y procesamiento de pedidos.
/// </summary>
/// <remarks>
/// Esta interfaz define todas las operaciones de negocio relacionadas con el módulo de carrito,
/// incluyendo gestión de productos en el carrito, checkout, procesamiento de pagos con Stripe,
/// y administración de ventas.
///
/// <para><b>Patrón de diseño:</b></para>
/// Utiliza el patrón Service que encapsula la lógica de negocio. Las operaciones retornan
/// <see cref="Result{T, TError}"/> para manejar errores de forma funcional.
///
/// <para><b>Principios aplicados:</b></para>
/// <list type="bullet">
///     <item>Single Responsibility: Solo maneja lógica de carrito y ventas</item>
///     <item>Dependency Inversion: Los controladores dependen de esta abstracción</item>
///     <item>Error Handling: Usa Result pattern para manejo de errores</item>
/// </list>
/// 
/// <para><b>Dependencias externas:</b></para>
/// <list type="bullet">
///     <item><see cref="Repository.ICartRepository"/> - Acceso a datos</item>
///     <item><see cref="Products.Repository.Productos.IProductRepository"/> - Gestión de productos</item>
///     <item><see cref="Stripe.IStripeService"/> - Procesamiento de pagos</item>
///     <item><see cref="Common.Mail.IEmailService"/> - Envío de emails</item>
///     <item>UserManager&lt;User&gt; - Gestión de usuarios</item>
/// </list>
/// </remarks>
public interface ICartService
{
    /// <summary>
    /// Obtiene todas las líneas de venta filtradas por permisos para administradores o gestores.
    /// </summary>
    /// <param name="managerId">ID del manager para filtrar sus ventas (opcional).</param>
    /// <param name="isAdmin">Indica si el usuario es administrador.</param>
    /// <param name="filter">Filtros de paginación.</param>
    /// <returns>Objeto paginado con las líneas de venta.</returns>
    /// <remarks>
    /// Si es admin, retorna todas las ventas. Si es manager, solo retorna sus productos.
    /// </remarks>
    Task<PageResponseDto<SaleLineDto>> FindAllSalesAsLinesAsync(long? managerId, bool isAdmin, FilterDto filter);

    /// <summary>
    /// Calcula las ganancias totales para administradores o gestores.
    /// </summary>
    /// <param name="managerId">ID del manager para filtrar ganancias (opcional).</param>
    /// <param name="isAdmin">Indica si el usuario es administrador.</param>
    /// <returns>Suma total de las ganancias.</returns>
    /// <remarks>
    /// Solo considera ventas completadas (carritos comprados).
    /// </remarks>
    Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin);

    /// <summary>
    /// Busca carritos filtrados por usuario y estado de compra de forma paginada.
    /// </summary>
    /// <param name="userId">Filtrar por ID de usuario (opcional, null para todos).</param>
    /// <param name="purchased">Estado de compra (true = pedidos, false = carritos activos).</param>
    /// <param name="filter">Filtros adicionales, paginación y ordenación.</param>
    /// <returns>Objeto paginado con los carritos encontrados.</returns>
    Task<PageResponseDto<CartResponseDto>> FindAllAsync(long? userId, bool purchased, FilterCartDto filter);

    /// <summary>
    /// Añade un producto al carrito especificado con cantidad 1.
    /// </summary>
    /// <param name="cartId">ID del carrito.</param>
    /// <param name="productId">ID del producto a añadir.</param>
    /// <returns>Result con el carrito actualizado o error si no se encontró el producto.</returns>
    /// <remarks>
    /// Si el producto ya está en el carrito, no aumenta la cantidad (solo añade 1).
    /// </remarks>
    Task<Result<CartResponseDto, DomainError>> AddProductAsync(string cartId, string productId);

    /// <summary>
    /// Elimina un producto del carrito.
    /// </summary>
    /// <param name="cartId">ID del carrito.</param>
    /// <param name="productId">ID del producto a eliminar.</param>
    /// <returns>Carrito actualizado sin el producto.</returns>
    Task<CartResponseDto> RemoveProductAsync(string cartId, string productId);

    /// <summary>
    /// Obtiene un carrito por su ID en formato DTO.
    /// </summary>
    /// <param name="id">ID del carrito.</param>
    /// <returns>Result con el DTO del carrito o error si no existe.</returns>
    Task<Result<CartResponseDto, DomainError>> GetByIdAsync(string id);

    /// <summary>
    /// Obtiene el modelo de dominio completo de un carrito por su ID.
    /// </summary>
    /// <param name="id">ID del carrito.</param>
    /// <returns>Entidad Cart o null si no existe.</returns>
    /// <remarks>
    /// Útil para operaciones que requieren la entidad completa (SaveAsync, SendConfirmationEmailAsync).
    /// </remarks>
    Task<Models.Cart?> GetCartModelByIdAsync(string id);

    /// <summary>
    /// Guarda o actualiza una entidad de carrito, marcándolo como comprado.
    /// </summary>
    /// <param name="entity">Entidad del carrito a guardar.</param>
    /// <returns>Result con el nuevo carrito creado o error.</returns>
    /// <remarks>
    /// Marca el carrito como Purchased=true, cambia estado de líneas a Preparado,
    /// y crea un nuevo carrito vacío para el usuario.
    /// </remarks>
    Task<Result<CartResponseDto, DomainError>> SaveAsync(Models.Cart entity);

    /// <summary>
    /// Envía un correo de confirmación de pedido al cliente de forma asíncrona.
    /// </summary>
    /// <param name="pedido">Entidad del carrito/pedido confirmado.</param>
    /// <returns>Task completo.</returns>
    /// <remarks>
    /// El email se encola para no bloquear la respuesta HTTP.
    /// </remarks>
    Task SendConfirmationEmailAsync(Models.Cart pedido);

    /// <summary>
    /// Actualiza la cantidad de un producto en el carrito validando el stock disponible.
    /// </summary>
    /// <param name="cartId">ID del carrito.</param>
    /// <param name="productId">ID del producto.</param>
    /// <param name="quantity">Nueva cantidad (debe ser >= 1).</param>
    /// <returns>Result con el carrito actualizado o error (stock insuficiente o cantidad mínima).</returns>
    Task<Result<CartResponseDto, DomainError>> UpdateStockWithValidationAsync(string cartId, string productId, int quantity);

    /// <summary>
    /// Procesa el pago y finaliza la compra del carrito.
    /// </summary>
    /// <param name="id">ID del carrito.</param>
    /// <returns>Result con la URL de checkout de Stripe o error.</returns>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Valida stock de todos los productos</item>
    ///     <li>Resta el stock con control de concurrencia</item>
    ///     <li>Crea sesión de checkout en Stripe</item>
    ///     <li>Marca CheckoutInProgress=true</item>
    /// </list>
    /// </remarks>
    Task<Result<string, DomainError>> CheckoutAsync(string id);

    /// <summary>
    /// Restaura el stock de los productos de un carrito (ej. en caso de cancelación o timeout).
    /// </summary>
    /// <param name="cartId">ID del carrito.</param>
    /// <returns>Task completo.</returns>
    /// <remarks>
    /// Solo restaura stock si el carrito no está marcado como Purchased.
    /// </remarks>
    Task RestoreStockAsync(string cartId);

    /// <summary>
    /// Elimina un carrito por su ID.
    /// </summary>
    /// <param name="id">ID del carrito a eliminar.</param>
    /// <returns>Task completo.</returns>
    /// <exception cref="Exceptions.CartNotFoundException">Si el carrito no existe.</exception>
    Task DeleteByIdAsync(string id);

    /// <summary>
    /// Obtiene el carrito activo (no comprado) de un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <returns>Result con el DTO del carrito o error si no existe.</returns>
    /// <remarks>
    /// Busca el carrito donde Purchased=false para el usuario especificado.
    /// </remarks>
    Task<Result<CartResponseDto, DomainError>> GetCartByUserIdAsync(long userId);

    /// <summary>
    /// Cancela una venta de un producto específico en un pedido.
    /// </summary>
    /// <param name="ventaId">ID del carrito/pedido.</param>
    /// <param name="productId">ID del producto a cancelar.</param>
    /// <param name="managerId">ID del manager que realiza la acción.</param>
    /// <param name="isAdmin">Indica si es administrador.</param>
    /// <returns>Error si ocurrió algún problema, null si fue exitoso.</returns>
    /// <remarks>
    /// Verifica permisos (solo el creator del producto o admin puede cancelar).
    /// Restaura el stock del producto.
    /// </remarks>
    Task<DomainError?> CancelSaleAsync(string ventaId, string productId, long? managerId, bool isAdmin);

    /// <summary>
    /// Actualiza el estado de una venta.
    /// </summary>
    /// <param name="ventaId">ID del carrito/pedido.</param>
    /// <param name="productId">ID del producto.</param>
    /// <param name="newStatus">Nuevo estado a aplicar.</param>
    /// <param name="managerId">ID del manager.</param>
    /// <param name="isAdmin">Indica si es administrador.</param>
    /// <returns>Error si ocurrió algún problema, null si fue exitoso.</returns>
    /// <remarks>
    /// Si se cambia de Cancelado a otro estado, descuenta stock.
    /// Si se cambia a Cancelado, restaura el stock.
    /// </remarks>
    Task<DomainError?> UpdateSaleStatusAsync(string ventaId, string productId, Models.Status newStatus, long? managerId, bool isAdmin);
    
    /// <summary>
    /// Cuenta las nuevas ventas de un gestor desde una fecha determinada.
    /// </summary>
    /// <param name="managerId">ID del manager.</param>
    /// <param name="since">Fecha desde la cual contar.</param>
    /// <returns>Result con el número de ventas o error.</returns>
    Task<Result<int, DomainError>> GetNewSalesCountAsync(long managerId, DateTime since);

    /// <summary>
    /// Obtiene el número total de ventas realizadas.
    /// </summary>
    /// <returns>Total de líneas en carritos comprados.</returns>
    Task<int> GetTotalSalesCountAsync();

    /// <summary>
    /// Limpia los carritos con checkout iniciado hace más del tiempo especificado,
    /// restaurando el stock de sus productos.
    /// </summary>
    /// <param name="expirationMinutes">Minutos de expiración (default 5).</param>
    /// <returns>Task completo.</returns>
    /// <remarks>
    /// Se utiliza para manejar timeouts de checkout. Busca carritos donde:
    /// <list type="bullet">
    ///     <item>CheckoutInProgress = true</item>
    ///     <li>CheckoutStartedAt &lt; ahora - expirationMinutes</item>
    /// </list>
    /// </remarks>
    Task CleanupExpiredCheckoutsAsync(int expirationMinutes = 5);
}