using dawazonBackend.Common;
using dawazonBackend.Common.Error;

namespace dawazonBackend.Products.Errors;

/// <summary>
/// Record base para errores relacionados con productos.
/// </summary>
/// <remarks>
/// Hereda de <see cref="DomainError"/> y forma parte del sistema de manejo de errores.
/// </remarks>
public record ProductError (string Message) : DomainError (Message);

/// <summary>
/// Error cuando no se encuentra un producto.
/// </summary>
public record ProductNotFoundError (string Message) : ProductError (Message);

/// <summary>
/// Error de validación en los datos del producto.
/// </summary>
public record ProductValidationError (string Message) : ProductError (Message);

/// <summary>
/// Error de solicitud incorrecta (Bad Request).
/// </summary>
public record ProductBadRequestError (string Message) : ProductError (Message);

/// <summary>
/// Error de conflicto (ej. producto duplicado).
/// </summary>
public record ProductConflictError (string Message) : ProductError (Message);

/// <summary>
/// Error relacionado con el almacenamiento de archivos.
/// </summary>
public record ProductStorageError (string Message) : ProductError (Message);

/// <summary>
/// Error cuando no hay suficiente stock del producto.
/// </summary>
public record InsufficientStockError(string Message) : ProductError(Message);