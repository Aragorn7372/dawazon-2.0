using dawazonBackend.Common;

namespace dawazonBackend.Cart.Errors;

/// <summary>
/// Record base para errores relacionados con carritos.
/// </summary>
public record CartError(string Message) : DomainError (Message);


/// <summary>
/// Error devuelto cuando no se encuentra un carrito.
/// </summary>
public record CartNotFoundError(string Message) : CartError(Message);

/// <summary>
/// Error devuelto cuando la cantidad excede el stock disponible.
/// </summary>
public record CartProductQuantityExceededError(
    string Message = "La cantidad de producto supera el stock"
) : CartError(Message);

/// <summary>
/// Error devuelto cuando se exceden los intentos permitidos por concurrencia.
/// </summary>
public record CartAttemptAmountExceededError(
    string Message = "La cantidad de tries superado, vuelva a intentarlo"
) : CartError(Message);

/// <summary>
/// Error devuelto cuando un usuario no tiene autorización.
/// </summary>
public record CartUnauthorizedError(string Message) : CartError(Message);

public record CartMinQuantityError(string Message) : CartError(Message);




