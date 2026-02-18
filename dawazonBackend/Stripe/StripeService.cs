using CSharpFunctionalExtensions;
using dawazonBackend.Common.Error;
using dawazonBackend.Stripe.Errors;
using Stripe;
using Stripe.Checkout;

namespace dawazonBackend.Stripe;

public class StripeService : IStripeService
    {
        private readonly string _serverUrl;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:Key"];
            _serverUrl = configuration["Server:Url"] ?? "https://www.dawazon.es";
            _logger = logger;
        }

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
                    SuccessUrl = $"{_serverUrl}/auth/me/cart/checkout/success/",
                    CancelUrl = $"{_serverUrl}/auth/me/cart/checkout/cancel/",
                    CustomerEmail = cart.Client.Email,
                    LineItems = lineItems
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);
                
                return Result.Success<string, DomainError>(session.Url);
            }
            catch (StripeException e)
            {
                // Errores específicos de la API de Stripe (tarjeta rechazada, datos inválidos...)
                _logger.LogWarning($"Error de Stripe al crear sesión de pago: {e.StripeError.Message}");
                return Result.Failure<string, DomainError>(
                    new StripePaymentError($"Error en la pasarela de pago: {e.StripeError.Message}"));
            }
            catch (Exception e)
            {
                // Errores inesperados
                _logger.LogError($"Error inesperado creando sesión de pago: {e.Message}");
                return Result.Failure<string, DomainError>(
                    new StripePaymentError("Ocurrió un error inesperado al procesar el pago."));
            }
        }
    }
