using dawazonBackend.Common.Error;

namespace dawazonBackend.Stripe.Errors;

public record StripeError (string Message) : DomainError (Message);

public record StripePaymentError (string Message) : StripeError (Message);