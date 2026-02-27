using CSharpFunctionalExtensions;
using dawazonBackend.Common.Error;
using dawazonBackend.Stripe.Errors;
using Stripe;
using Stripe.Checkout;

namespace dawazonBackend.Stripe;

/// <summary>
/// Servicio para gestionar la integración con la pasarela de pagos Stripe.
/// </summary>
public class StripeService : IStripeService
    {
        private readonly string _serverUrl;
        private readonly ILogger<StripeService> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="StripeService"/>.
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <param name="logger">Logger para registrar eventos.</param>
        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:Key"];
            _serverUrl = configuration["Server:Url"] ?? "https://www.dawazon.es";
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Result<string, DomainError>> CreateCheckoutSessionAsync(Cart.Models.Cart cart)
        {
            try
            {
                var lineItems = new List<SessionLineItemOptions>();

                foreach (var line in cart.CartLines)
                {
                    long amount = (long)(line.ProductPrice * 100);

                    lineItems.Add(new SessionLineItemOptions
                    {
                        Quantity = line.Quantity,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",
                            UnitAmount = amount,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Producto " + line.ProductId
                            }
                        }
                    });
                }

                var options = new SessionCreateOptions
                {
                    Mode = "payment",
                    SuccessUrl = $"{_serverUrl}/pedidos/success?cartId={cart.Id}&session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl  = $"{_serverUrl}/pedidos/cancel?cartId={cart.Id}",
                    CustomerEmail = cart.Client.Email,
                    LineItems = lineItems
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);
                
                return Result.Success<string, DomainError>(session.Url);
            }
            catch (StripeException e)
            {
                // e.StripeError puede ser null en errores de red/configuración, por eso usamos la ?.
                _logger.LogWarning($"Error de Stripe al crear sesión de pago: {e.StripeError?.Message ?? e.Message}");
                return Result.Failure<string, DomainError>(
                    new StripePaymentError($"Error en la pasarela de pago: {e.StripeError?.Message ?? e.Message}"));
            }
            catch (Exception e)
            {
                _logger.LogError($"Error inesperado creando sesión de pago: {e.Message}");
                return Result.Failure<string, DomainError>(
                    new StripePaymentError("Ocurrió un error inesperado al procesar el pago."));
            }
        }
    }
