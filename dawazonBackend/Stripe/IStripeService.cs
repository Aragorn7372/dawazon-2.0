using CSharpFunctionalExtensions;
using dawazonBackend.Common;
using dawazonBackend.Common.Error;

namespace dawazonBackend.Stripe;

public interface IStripeService
{
    Task<Result<string, DomainError>> CreateCheckoutSessionAsync(Cart.Models.Cart cart);
}