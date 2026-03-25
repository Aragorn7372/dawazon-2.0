using dawazonBackend.Common.Error;

namespace dawazonBackend.Stripe.Errors;

/// <summary>
/// Record base para errores de Stripe.
/// </summary>
public record StripeError (string Message) : DomainError (Message);

/// <summary>
/// Error específico de pagos con Stripe.
/// </summary>
public record StripePaymentError (string Message) : StripeError (Message);