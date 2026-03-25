using dawazonBackend.Common;
using dawazonBackend.Common.Error;

namespace dawazonBackend.Users.Errors;

/// <summary>
/// Record base para errores relacionados con usuarios.
/// </summary>
/// <remarks>
/// Hereda de <see cref="DomainError"/> y forma parte del sistema de manejo de errores.
/// Se utiliza para retornar errores de forma segura en lugar de lanzar excepciones.
/// </remarks>
public record UserError(string Message) : DomainError (Message);

/// <summary>
/// Error cuando no se encuentra un usuario en el sistema.
/// </summary>
/// <remarks>
/// Se lanza cuando se intenta acceder a un usuario que no existe.
/// </remarks>
public record UserNotFoundError(string Message) : UserError(Message);

/// <summary>
/// Error cuando falla la actualización de un usuario.
/// </summary>
/// <remarks>
/// Puede ser por datos inválidos o error de base de datos.
/// </remarks>
public record UserUpdateError(string Message):UserError(Message);

/// <summary>
/// Error de conflicto al gestionar un usuario.
/// </summary>
/// <remarks>
/// Ejemplos: email duplicado, username ya existe.
/// </remarks>
public record UserConflictError(string Message):UserError(Message);

/// <summary>
/// Error cuando el usuario no tiene permisos para una operación.
/// </summary>
public record UnauthorizedError(string Message):UserError(Message);

/// <summary>
/// Error cuando el usuario ya tiene un producto asignado.
/// </summary>
/// <remarks>
/// Se usa cuando se intenta asignar un producto que ya está asignado al usuario.
/// </remarks>
public record UserHasThatProductError(string Message):UserError(Message);