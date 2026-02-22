using dawazonBackend.Common;
using dawazonBackend.Common.Error;

namespace dawazonBackend.Users.Errors;

/// <summary>
/// Record base para errores relacionados con usuarios.
/// </summary>
public record UserError(string Message) : DomainError (Message);

/// <summary>
/// Error devuelto cuando no se encuentra un usuario.
/// </summary>
public record UserNotFoundError(string Message) : UserError(Message);

/// <summary>
/// Error devuelto cuando falla la actualización de un usuario.
/// </summary>
public record UserUpdateError(string Message):UserError(Message);

/// <summary>
/// Error devuelto cuando hay un conflicto (ej. email duplicado) al gestionar un usuario.
/// </summary>
public record UserConflictError(string Message):UserError(Message);

/// <summary>
/// Error devuelto cuando una operación requiere autorización y el usuario no la tiene.
/// </summary>
public record UnauthorizedError(string Message):UserError(Message);
///
///
/// 
public record UserHasThatProductError(string Message):UserError(Message);