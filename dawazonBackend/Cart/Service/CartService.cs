using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Errors;
using dawazonBackend.Cart.Mapper;
using dawazonBackend.Cart.Models;
using dawazonBackend.Cart.Repository;
using dawazonBackend.Common;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Common.Mail;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Repository.Productos;
using dawazonBackend.Stripe;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Cart.Service
{
/// <summary>
/// Implementación del servicio de gestión de carritos de compra, ventas y pagos con Stripe.
/// </summary>
public class CartService : ICartService
{
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<CartService> _logger;
    private readonly IStripeService _stripeService;
    private readonly IEmailService _mailService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CartService"/>.
    /// </summary>
    public CartService(
        IProductRepository productRepository,
        ICartRepository cartRepository,
        UserManager<User> userManager,
        IStripeService stripeService,
        IEmailService mailService,
        ILogger<CartService> logger
        )
        {
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _userManager = userManager;
            _stripeService = stripeService;
            _mailService = mailService;
            _logger = logger;
        }

    /// <inheritdoc/>
    public async Task<PageResponseDto<SaleLineDto>> FindAllSalesAsLinesAsync(long? managerId, bool isAdmin, FilterDto filter)
        {
            _logger.LogInformation($"Buscando ventas - Manager: {managerId}, isAdmin: {isAdmin}");

            var (items, totalElements) = await _cartRepository.GetSalesAsLinesAsync(managerId, isAdmin, filter);

            _logger.LogInformation($"Ventas filtradas devueltas: {items.Count}");

            // Calculamos el total de páginas
            int totalPages = filter.Size > 0 ? (int)Math.Ceiling(totalElements / (double)filter.Size) : 0;

            return new PageResponseDto<SaleLineDto>(
                Content: items,
                TotalPages: totalPages,
                TotalElements: totalElements,
                PageSize: filter.Size,
                PageNumber: filter.Page,
                TotalPageElements: items.Count,
                SortBy: filter.SortBy,
                Direction: filter.Direction
            );
        }
        
    /// <inheritdoc/>
    public async Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin)
        {
            return await _cartRepository.CalculateTotalEarningsAsync(managerId, isAdmin);
        }

    /// <inheritdoc/>
    public async Task<PageResponseDto<CartResponseDto>> FindAllAsync(long? userId, bool purchased, FilterCartDto filter)
        {
            // Deconstruimos la tupla devuelta por el repositorio
            var (itemsEnumerable, totalCount) = await _cartRepository.GetAllAsync(filter);
            var items = userId is {} 
                    ?itemsEnumerable.Where(p => p.UserId == userId && p.Purchased == purchased)
                    .Select(c => c.ToDto())
                    .ToList() 
                    :itemsEnumerable.Where(p => p.Purchased == purchased)
                    .Select(c => c.ToDto())
                    .ToList();
    
            // Calculamos el total de páginas (redondeando hacia arriba)
            int totalPages = filter.Size > 0 ? (int)Math.Ceiling(totalCount / (double)filter.Size) : 0;

            return new PageResponseDto<CartResponseDto>(
                Content: items,
                TotalPages: totalPages,
                TotalElements: totalCount,
                PageSize: filter.Size,
                PageNumber: filter.Page,
                TotalPageElements: items.Count, // Elementos devueltos en esta página específica
                SortBy: filter.SortBy,
                Direction: filter.Direction
            );
        }

    /// <inheritdoc/>
    public async Task<Result<CartResponseDto, DomainError>> AddProductAsync(string cartId, string productId)
        {
            _logger.LogInformation($"Añadiendo producto {productId} a {cartId}");
            
            var product = await _productRepository.GetProductAsync(productId);
            if (product == null)
                return Result.Failure<CartResponseDto, DomainError>(
                    new ProductNotFoundError($"No se encontró el Product con id: {productId}."));

            var line = new CartLine
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = 1,
                ProductPrice = product.Price,
                Status = Status.EnCarrito
            };

            await _cartRepository.AddCartLineAsync(cartId, line);

            // Recuperar y recalcular
            var newCart = await RecalculateCartTotalsAsync(cartId);

            return newCart.Value.ToDto();
        }

    /// <inheritdoc/>
    public async Task<CartResponseDto> RemoveProductAsync(string cartId, string productId)
        {
            _logger.LogInformation($"Eliminando producto del carrito, con ID: {productId}");
            
            var line = new CartLine { ProductId = productId };
            await _cartRepository.RemoveCartLineAsync(cartId, line);

            var newCart = await RecalculateCartTotalsAsync(cartId);

            return newCart.Value.ToDto();
        }

    /// <inheritdoc/>
    public async Task<Result<CartResponseDto, DomainError>> GetByIdAsync(string id)
        {
            var cart = await _cartRepository.FindCartByIdAsync(id);
            if (cart == null)
                return Result.Failure<CartResponseDto, DomainError>(
                    new CartNotFoundError($"No se encontró el Carrito con id: {id}."));
            return cart.ToDto();
        }

    /// <inheritdoc/>
    public async Task<Models.Cart?> GetCartModelByIdAsync(string id)
        => await _cartRepository.FindCartByIdAsync(id);

    /// <inheritdoc/>
    public async Task<Result<CartResponseDto, DomainError>> SaveAsync(Models.Cart entity)
        {
            foreach (var line in entity.CartLines)
            {
                line.Status = Status.Preparado;
            }
            
            entity.Purchased = true;
            entity.CheckoutInProgress = false;
            entity.CheckoutStartedAt = null;
            
            await _cartRepository.UpdateCartAsync(entity.Id, entity);

            var newCart = await CreateNewCartAsync(entity.UserId);

            return newCart.IsSuccess? newCart.Value.ToDto(): newCart.Error;
        }

    /// <inheritdoc/>
    public async Task SendConfirmationEmailAsync(Models.Cart pedido)
        {
            _logger.LogInformation($"Preparando email de confirmación para pedido: {pedido.Id}");
    
            try
            {
                // Generamos el contenido en HTML usando los templates
                var htmlContent = EmailTemplates.PedidoConfirmado(pedido);
                var body = EmailTemplates.CreateBase("Confirmación de Pedido", htmlContent);

                // Creamos el objeto EmailMessage que espera tu servicio
                var emailMessage = new EmailMessage
                {
                    To = pedido.Client.Email,
                    Subject = $"¡Tu pedido {pedido.Id} está confirmado!",
                    Body = body,
                    IsHtml = true
                };

                // Lo encolamos para que BackgroundService lo procese sin bloquear este hilo
                await _mailService.EnqueueEmailAsync(emailMessage);
        
                _logger.LogInformation($"Email de pedido {pedido.Id} encolado correctamente para {pedido.Client.Email}");
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Error al encolar el email para el pedido {pedido.Id}: {e.Message}");
            }
        }

    /// <inheritdoc/>
    public async Task<Result<CartResponseDto, DomainError>> UpdateStockWithValidationAsync(string cartId, string productId, int quantity)
        {
            if (quantity < 1) return Result.Failure<CartResponseDto, DomainError>(
                new CartMinQuantityError("La cantidad mínima es 1"));

            var cart = await _cartRepository.FindCartByIdAsync(cartId);
            if (cart == null) return Result.Failure<CartResponseDto, DomainError>(
                new CartNotFoundError($"No se encontró el carrito con id: {cartId}."));
            
            var product = await _productRepository.GetProductAsync(productId);
            if (product == null) return Result.Failure<CartResponseDto, DomainError>(
                new ProductNotFoundError($"No se encontró el producto con id: {productId}."));
            
            if (product.Stock < quantity) return Result.Failure<CartResponseDto, DomainError>(
                new CartProductQuantityExceededError($"Stock insuficiente. Solo hay {product.Stock}"));

            var line = cart.CartLines.FirstOrDefault(l => l.ProductId == productId);
            if (line != null)
            {
                line.Quantity = quantity;
            }

            // Actualizamos en la BBDD
            await _cartRepository.AddCartLineAsync(cartId, line!); // AddCartLineAsync también actualiza cantidad si existe
            
            var updatedStockCart = await RecalculateCartTotalsAsync(cartId);
            
            return updatedStockCart.IsSuccess? updatedStockCart.Value.ToDto() : updatedStockCart.Error;
        }

    /// <inheritdoc/>
    public async Task<Result<string, DomainError>> CheckoutAsync(string id)
        {
            var entity = await _cartRepository.FindCartByIdAsync(id);

            if (entity != null)
            {
                entity.CheckoutInProgress = true;
                entity.CheckoutStartedAt = DateTime.UtcNow;

                //Buscar cliente del User y asignarlo
                var user = await _userManager.FindByIdAsync(entity.UserId.ToString());
                if (user == null)
                {
                    _logger.LogWarning($"User no encontrado con id: {entity.UserId}");
                    return Result.Failure<string, DomainError>(
                        new UserNotFoundError($"Usuario no encontrado para el carrito con id: {id}."));
                }

                entity.Client = new Client
                {
                    Name  = user.Client?.Name  ?? string.Empty,
                    Email = user.Client?.Email ?? string.Empty,
                    Phone = user.Client?.Phone ?? string.Empty,
                    Address = new Address
                    {
                        Street     = user.Client?.Address?.Street     ?? string.Empty,
                        Number     = user.Client?.Address?.Number     ?? 0,
                        City       = user.Client?.Address?.City       ?? string.Empty,
                        Province   = user.Client?.Address?.Province   ?? string.Empty,
                        Country    = user.Client?.Address?.Country    ?? string.Empty,
                        PostalCode = user.Client?.Address?.PostalCode ?? 0
                    }
                };

                // Actualizar el cliente sin perder las líneas cargadas
                // entity ya trae sus CartLines desde _cartRepository.FindCartByIdAsync
                await _cartRepository.UpdateCartAsync(id, entity);
                // Control de concurrencia optimista ajustado al repositorio de C#
                foreach (var line in entity.CartLines)
                {
                    int intentos = 0;
                    bool success = false;

                    while (!success)
                    {
                        var product = await _productRepository.GetProductAsync(line.ProductId);
                        if (product == null)
                            return Result.Failure<string, DomainError>(
                                new ProductNotFoundError($"No se encontró el producto con id: {line.ProductId}."));

                        if (product.Stock < line.Quantity)
                            return Result.Failure<string, DomainError>(
                                new CartProductQuantityExceededError($"Stock insuficiente. Solo hay {product.Stock}"));

                        // SubstractStockAsync maneja la versión y devuelve 1 si tuvo éxito, 0 si falló (concurrencia)
                        var result =
                            await _productRepository.SubstractStockAsync(line.ProductId, line.Quantity,
                                product.Version);

                        if (result == 1)
                        {
                            success = true;
                        }
                        else
                        {
                            intentos++;
                            if (intentos >= 3)
                                return Result.Failure<string, DomainError>(
                                    new CartAttemptAmountExceededError());
                        }
                    }
                }

                //STRIPE
                var stripeResult = await _stripeService.CreateCheckoutSessionAsync(entity);

                if (stripeResult.IsFailure)
                {
                    // Si falla, propagamos el error de dominio hacia el controlador
                    return Result.Failure<string, DomainError>(stripeResult.Error);
                }

                // Devolvemos la URL en caso de éxito
                return stripeResult.Value; 
            }

            return Result.Failure<string, DomainError>(
                new CartNotFoundError($"No se encontró el Carrito con id: {id}."));
        }

    /// <inheritdoc/>
    public async Task RestoreStockAsync(string cartId)
        {
            var cart = await _cartRepository.FindCartByIdAsync(cartId);
            if (cart != null) {
            
                if (!cart.Purchased)
                { 
                    foreach (var line in cart.CartLines) 
                    { 
                        var product = await _productRepository.GetProductAsync(line.ProductId);
                        if (product != null)
                        {
                            product.Stock += line.Quantity;
                            await _productRepository.UpdateProductAsync(product, product.Id!);
                        }
                    }
                
                    cart.CheckoutInProgress = false;
                    cart.CheckoutStartedAt = null;
                    await _cartRepository.UpdateCartAsync(cartId, cart);
                
                    _logger.LogInformation($"Stock restaurado para el carrito: {cartId}");
                }
            }
        }

    /// <inheritdoc/>
    public async Task DeleteByIdAsync(string id)
        {
            await _cartRepository.DeleteCartAsync(id);
        }

    /// <inheritdoc/>
    public async Task<Result<CartResponseDto, DomainError>> GetCartByUserIdAsync(long userId)
        {
            var cart = await _cartRepository.FindByUserIdAndPurchasedAsync(userId, false);
            
            if (cart == null) return Result.Failure<CartResponseDto, DomainError>(
                new CartNotFoundError($"No se encontró carrito perteneciente al usuario con id: {userId}."));
            
            return cart.ToDto();
        }

    /// <inheritdoc/>
    public async Task<DomainError?> CancelSaleAsync(string ventaId, string productId, long? managerId, bool isAdmin)
        {
            var cartResult = await GetByIdAsync(ventaId);
    
            if (cartResult.IsFailure) 
                return new CartNotFoundError($"Carrito no encontrado con id: {ventaId}");

            var line = cartResult.Value.CartLines.FirstOrDefault(l => l.ProductId == productId);
    
            if (line == null) 
                return new CartNotFoundError("Producto no encontrado en esta venta");

            var product = await _productRepository.GetProductAsync(productId);
            if (product == null) 
                return new ProductNotFoundError($"Producto no encontrado con id: {productId}");

            if (!isAdmin && product.CreatorId != managerId)
                return new CartUnauthorizedError("No tienes permisos para cancelar esta venta");

            if (line.Status != Status.Cancelado)
            {
                await _cartRepository.UpdateCartLineStatusAsync(ventaId, productId, Status.Cancelado);
        
                product.Stock += line.Quantity;
                await _productRepository.UpdateProductAsync(product, product.Id!);
        
                _logger.LogInformation($"Venta cancelada: Cart {ventaId} Product {productId}");
            }

            // Devolvemos null indicando que NO hay error (todo salió bien)
            return null; 
        }
        
    /// <inheritdoc/>
    public async Task<DomainError?> UpdateSaleStatusAsync(string ventaId, string productId, Status newStatus, long? managerId, bool isAdmin)
    {
        if (newStatus == Status.Cancelado)
        {
            return await CancelSaleAsync(ventaId, productId, managerId, isAdmin);
        }

        var cartResult = await GetByIdAsync(ventaId);
        if (cartResult.IsFailure) 
            return new CartNotFoundError($"Carrito no encontrado con id: {ventaId}");

        var line = cartResult.Value.CartLines.FirstOrDefault(l => l.ProductId == productId);
        if (line == null) 
            return new CartNotFoundError("Producto no encontrado en esta venta");

        var product = await _productRepository.GetProductAsync(productId);
        if (product == null) 
            return new ProductNotFoundError($"Producto no encontrado con id: {productId}");

        if (!isAdmin && product.CreatorId != managerId)
            return new CartUnauthorizedError("No tienes permisos para actualizar esta venta");

        if (line.Status == Status.Cancelado && newStatus != Status.Cancelado)
        {
            if (product.Stock < line.Quantity)
                return new CartProductQuantityExceededError($"Stock insuficiente. Solo hay {product.Stock}");

            product.Stock -= line.Quantity;
            await _productRepository.UpdateProductAsync(product, product.Id!);
        }

        await _cartRepository.UpdateCartLineStatusAsync(ventaId, productId, newStatus);
        _logger.LogInformation($"Estado de la venta actualizado: Cart {ventaId} Product {productId} a {newStatus}");

        return null;
    }

    private async Task<Result<Models.Cart, DomainError>> RecalculateCartTotalsAsync(string cartId)
        {
            var cart = await _cartRepository.FindCartByIdAsync(cartId);
            if (cart == null) return Result.Failure<Models.Cart, DomainError>(
                new CartNotFoundError($"No se encontró carrito con id: {cartId}."));

            cart.TotalItems = cart.CartLines.Count;
            cart.Total = cart.CartLines.Sum(l => l.TotalPrice);

            // Usamos UpdateCartScalarsAsync en lugar de UpdateCartAsync para evitar el bug
            // donde Clear()+AddRange() sobre la misma referencia EF Core borraba todas las líneas.
            await _cartRepository.UpdateCartScalarsAsync(cart.Id, cart.TotalItems, cart.Total);

            return cart;
        }

        private async Task<Result<Models.Cart, DomainError>> CreateNewCartAsync(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning($"Usuario no encontrado con id: {userId}");
                return Result.Failure<Models.Cart, DomainError>(
                    new UserNotFoundError($"No se encontró usuario con id: {userId}."));
            }

            var cart = new Models.Cart
            {
                Id = "CART" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                UserId = userId,
                Client = new Client
                {
                    Name = user.Client?.Name ?? string.Empty,
                    Email = user.Client?.Email ?? string.Empty,
                    Phone = user.Client?.Phone ?? string.Empty,
                    Address = new Address
                    {
                        Street = user.Client?.Address?.Street ?? string.Empty,
                        Number = user.Client?.Address?.Number ?? 0,
                        City = user.Client?.Address?.City ?? string.Empty,
                        Province = user.Client?.Address?.Province ?? string.Empty,
                        Country = user.Client?.Address?.Country ?? string.Empty,
                        PostalCode = user.Client?.Address?.PostalCode ?? 0
                    }
                },
                CartLines = new List<CartLine>(),
                Purchased = false,
                TotalItems = 0,
                Total = 0,
                CreatedAt = DateTime.UtcNow,
                UploadAt = DateTime.UtcNow
            };
            return await _cartRepository.CreateCartAsync(cart); 
        }

        /// <inheritdoc/>
        public async Task<Result<int, DomainError>> GetNewSalesCountAsync(long managerId, DateTime since)
        {
            try
            {
                var count = await _cartRepository.CountNewSalesAsync(managerId, since);
                return Result.Success<int, DomainError>(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando nuevas ventas para manager {ManagerId}", managerId);
                return Result.Failure<int, DomainError>(new CartError(ex.Message));
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetTotalSalesCountAsync()
        {
            _logger.LogInformation("Obteniendo total de ventas");
            return await _cartRepository.GetTotalSalesCountAsync();
        }

        /// <inheritdoc/>
        public async Task CleanupExpiredCheckoutsAsync(int expirationMinutes = 5)
        {
            _logger.LogDebug("🔍 Buscando carritos con checkout expirado (>{Min} min)...", expirationMinutes);

            var cutoff = DateTime.UtcNow.AddMinutes(-expirationMinutes);

            // Obtenemos todos los carritos no comprados con checkout en progreso
            var (allCarts, _) = await _cartRepository.GetAllAsync(
                new FilterCartDto(null, null, false, 0, int.MaxValue));

            var expired = allCarts
                .Where(c => c.CheckoutInProgress && c.CheckoutStartedAt.HasValue && c.CheckoutStartedAt.Value < cutoff)
                .ToList();

            if (expired.Count == 0)
            {
                _logger.LogDebug("No hay carritos con checkout expirado.");
                return;
            }

            _logger.LogInformation("Limpiando {Count} carrito(s) con checkout expirado.", expired.Count);

            foreach (var cart in expired)
            {
                _logger.LogInformation("  → Restaurando stock del carrito {CartId} (checkout iniciado: {Started})",
                    cart.Id, cart.CheckoutStartedAt);
                await RestoreStockAsync(cart.Id);
            }
        }
    }
}