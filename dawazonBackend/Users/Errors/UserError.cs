using dawazonBackend.Common;
using dawazonBackend.Common.Error;

namespace dawazonBackend.Users.Errors;

/// <summary>
/// Record base para errores relacionados con carritos.
/// </summary>
public record UserError(string Message) : DomainError (Message);


/// <summary>
/// Error devuelto cuando no se encuentra un usuario.
/// </summary>
public record UserNotFoundError(string Message) : UserError(Message);
public record UserUpdateError(string Message):UserError(Message);
public record UserConflictError(string Message):UserError(Message);
public record UnauthorizedError(string Message):UserError(Message);
