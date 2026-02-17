using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Errors;
using dawazonBackend.Cart.Mapper;
using dawazonBackend.Cart.Models;
using dawazonBackend.Cart.Repository;
using dawazonBackend.Common;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Mail;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Repository.Productos;

namespace dawazonBackend.Cart.Service
{
    public class CartService : ICartService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartService> _logger;
        // Dependencias faltantes comentadas por ahora:
        // private readonly IUserRepository _userRepository;
        // private readonly IStripeService _stripeService;
        private readonly IEmailService _mailService;

        public CartService(
            IProductRepository productRepository,
            ICartRepository cartRepository,
            IEmailService mailService,
            ILogger<CartService> logger
            )
        {
            _productRepository = productRepository;
            _cartRepository = cartRepository;
            _mailService = mailService;
            _logger = logger;
        }

        // TODO: findAllSalesAsLines CUANDO TENGAMOS LA PARTE DE USUARIOS. 
        
        public async Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin)
        {
            return await _cartRepository.CalculateTotalEarningsAsync(managerId, isAdmin);
        }

        public async Task<PageResponseDto<CartResponseDto>> FindAllAsync(long? userId, string purchased, FilterCartDto filter)
        {
            // Deconstruimos la tupla devuelta por el repositorio
            var (itemsEnumerable, totalCount) = await _cartRepository.GetAllAsync(filter);
            var items = itemsEnumerable
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

        public async Task<CartResponseDto> RemoveProductAsync(string cartId, string productId)
        {
            _logger.LogInformation($"Eliminando producto del carrito, con ID: {productId}");
            
            var line = new CartLine { ProductId = productId };
            await _cartRepository.RemoveCartLineAsync(cartId, line);

            var newCart = await RecalculateCartTotalsAsync(cartId);

            return newCart.Value.ToDto();
        }

        public async Task<Result<CartResponseDto, DomainError>> GetByIdAsync(string id)
        {
            var cart = await _cartRepository.FindCartByIdAsync(id);
            if (cart == null)
                return Result.Failure<CartResponseDto, DomainError>(
                    new CartNotFoundError($"No se encontró el Carrito con id: {id}."));
            return cart.ToDto();
        }

        public async Task<CartResponseDto> SaveAsync(Models.Cart entity)
        {
            foreach (var line in entity.CartLines)
            {
                line.Status = Status.Preparado;
            }
            
            entity.Purchased = true;
            entity.CheckoutInProgress = false;
            entity.CheckoutStartedAt = null;
            
            var savedCart = await _cartRepository.UpdateCartAsync(entity.Id, entity);
            
            // TODO: createNewCart(entity.UserId); -> Pendiente de IUserRepository

            return savedCart!.ToDto();
        }

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
            
            return updatedStockCart.Value.ToDto();
        }

        public async Task<Result<string, DomainError>> CheckoutAsync(string id)
        {
            var entity = await _cartRepository.FindCartByIdAsync(id);

            if (entity != null)
            {
                entity.CheckoutInProgress = true;
                entity.CheckoutStartedAt = DateTime.UtcNow;

                // TODO: Buscar cliente del User y asignarlo en el campo Cliente -> Pendiente de UserRepository

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

                // TODO: Lógica de Stripe
                return "url_de_stripe_simulada";
            }
            return Result.Failure<string, DomainError>(
                new CartNotFoundError($"No se encontró el Carrito con id: {id}."));
        }

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

        public async Task DeleteByIdAsync(string id)
        {
            await _cartRepository.DeleteCartAsync(id);
        }

        public async Task<Result<CartResponseDto, DomainError>> GetCartByUserIdAsync(long userId)
        {
            var cart = await _cartRepository.FindByUserIdAndPurchasedAsync(userId, false);
            
            if (cart == null) return Result.Failure<CartResponseDto, DomainError>(
                new CartNotFoundError($"No se encontró carrito perteneciente al usuario con id: {userId}."));
            
            return cart.ToDto();
        }

        public async Task<DomainError?> CancelSaleAsync(string ventaId, string productId, long managerId, bool isAdmin)
        {
            var cartResult = await GetByIdAsync(ventaId);
    
            if (cartResult.Value == null) 
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
        
        private async Task<Result<Models.Cart, DomainError>> RecalculateCartTotalsAsync(string cartId)
        {
            var cart = await _cartRepository.FindCartByIdAsync(cartId);
            if (cart == null) return Result.Failure<Models.Cart, DomainError>(
                new CartNotFoundError($"No se encontró carrito con id: {cartId}."));

            cart.TotalItems = cart.CartLines.Count;
            cart.Total = cart.CartLines.Sum(l => l.TotalPrice);

            return await _cartRepository.UpdateCartAsync(cart.Id, cart) ?? cart;
        }
    }
}