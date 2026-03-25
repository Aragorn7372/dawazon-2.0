namespace dawazonBackend.Common.Error;

/// <summary>
/// Record abstracto base para errores de dominio en la aplicación.
/// </summary>
/// <remarks>
/// Este record sirve como base para todos los errores del dominio.
/// Utiliza el patrón Result para manejo funcional de errores.
///
/// <para><b>Uso:</b></para>
/// Se combina con <c>CSharpFunctionalExtensions.Result&lt;T, TError&gt;</c>
/// para retornar errores de forma segura en lugar de lanzar excepciones.
///
/// <para><b>Jerarquía:</b></para>
/// <code>
/// DomainError (base abstracta)
///   ├── CartError
///   │     ├── CartNotFoundError
///   │     ├── CartProductQuantityExceededError
///   │     ├── CartAttemptAmountExceededError
///   │     ├── CartUnauthorizedError
///   │     └── CartMinQuantityError
///   ├── ProductError
///   │     ├── ProductNotFoundError
///   │     └── ProductStockInsuficientError
///   └── UserError
///         └── UserNotFoundError
/// </code>
/// 
/// <para><b>Ejemplo de uso:</b></para>
/// <code>
/// public Task&lt;Result&lt;CartResponseDto, DomainError&gt;&gt; GetByIdAsync(string id)
/// {
///     var cart = await _repository.FindByIdAsync(id);
///     if (cart == null)
///         return Result.Failure&lt;CartResponseDto, DomainError&gt;(
///             new CartNotFoundError($"Carrito con ID {id} no encontrado"));
///     return cart.ToDto();
/// }
/// </code>
/// </remarks>
public abstract record DomainError(string Message);