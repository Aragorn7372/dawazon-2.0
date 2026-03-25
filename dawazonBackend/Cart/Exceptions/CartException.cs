namespace dawazonBackend.Cart.Exceptions;

/// <summary>
/// Excepción base para todos los errores relacionados con operaciones del carrito de compras.
/// </summary>
/// <remarks>
/// Esta clase sirve como excepción base para el módulo de carrito. hereda de
/// <see cref="Exception"/> de .NET y proporciona la estructura fundamental para
/// todas las excepciones específicas del dominio.
///
/// <para><b>Jerarquía de excepciones:</b></para>
/// <code>
/// Exception (System)
///   └── CartException
///       └── CartNotFoundException
/// </code>
/// 
/// <para><b>Uso típico:</b></para>
/// Se lanza cuando ocurre un error inesperado en las operaciones del carrito
/// que no tiene un <see cref="Errors.CartError"/> específico, como errores de base de datos,
/// problemas de conexión, o errores no controlados.
///
/// <example>
/// Ejemplo de lanzamiento:
/// <code>
/// if (cart == null)
///     throw new CartException("Error al recuperar el carrito de la base de datos");
/// </code>
/// </example>
/// </remarks>
public class CartException(string message) : Exception(message);

/// <summary>
/// Excepción específica para cuando no se encuentra un carrito en el sistema.
/// </summary>
/// <remarks>
/// Esta excepción se lanza cuando se intenta acceder a un carrito que no existe
/// en la base de datos. A diferencia de <see cref="Errors.CartNotFoundError"/> que es
/// un error del dominio, esta es una excepción técnica.
///
/// <para><b>Diferencia con CartNotFoundError:</b></para>
/// <list type="bullet">
///     <item><see cref="CartNotFoundError"/>: Error de dominio, usado en el flujo normal de la aplicación</item>
///     <item><see cref="CartNotFoundException"/>: Excepción técnica, indica un problema inesperado</item>
/// </list>
/// 
/// <para><b>Escenarios de uso:</b></para>
/// <list type="bullet">
///     <item>El ID del carrito no existe en la base de datos</item>
///     <item>El carrito fue eliminado por otra operación</item>
///     <item>Error de sincronización con la base de datos</item>
/// </list>
/// 
/// <example>
/// Ejemplo de lanzamiento:
/// <code>
/// var cart = await _cartRepository.GetByIdAsync(id);
/// if (cart == null)
///     throw new CartNotFoundException($"El carrito con ID {id} no fue encontrado");
/// </code>
/// </example>
/// </remarks>
public class CartNotFoundException(string message) : CartException(message);
