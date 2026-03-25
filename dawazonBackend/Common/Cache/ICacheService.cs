namespace dawazonBackend.Common.Cache;

/// <summary>
/// Interfaz que define el contrato para el servicio de caché distribuido.
/// </summary>
/// <remarks>
/// Esta interfaz proporciona una abstracción sobre el sistema de caché subyacente,
/// permitiendo cambiar entre diferentes implementaciones (Redis, MemoryCache, etc.)
/// sin modificar el código que la consume.
///
/// <para><b>Patrón de diseño:</b></para>
/// Interfaz para el patrón Repository aplicada a caché.
/// Sigue el principio de Dependency Inversion.
///
/// <para><b>Operaciones soportadas:</b></para>
/// <list type="bullet">
///     <item>GetAsync: Recuperar valores con deserialización</item>
///     <item>SetAsync: Guardar valores con serialización y expiración</item>
///     <item>RemoveAsync: Eliminar entradas específicas</item>
///     <item>RemoveByPatternAsync: Eliminar entradas por patrón glob</item>
/// </list>
/// 
/// <para><b>Implementación:</b></para>
/// Ver <see cref="CacheService"/> para la implementación con Redis.
/// </remarks>
public interface ICacheService
{
    /// <summary>
    /// Obtiene un valor de la caché.
    /// </summary>
    /// <typeparam name="T">Tipo del valor a recuperar.</typeparam>
    /// <param name="key">Clave única del valor.</param>
    /// <returns>El valor encontrado deserializado o default(T) si no existe.</returns>
    /// <remarks>
    /// Si la clave no existe o expiró, retorna el valor por defecto del tipo.
    /// Implementa cache-aside: si no hay caché, el llamador debe obtener el dato de la fuente original.
    /// </remarks>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Guarda un valor en la caché.
    /// </summary>
    /// <typeparam name="T">Tipo del valor a guardar.</typeparam>
    /// <param name="key">Clave única para identificar el valor.</param>
    /// <param name="value">Valor a guardar (se serializa a JSON).</param>
    /// <param name="expiration">Tiempo hasta la expiración (opcional, default 5 minutos).</param>
    /// <returns>Task completo.</returns>
    /// <remarks>
    /// Si expiration es null, usa 5 minutos por defecto.
    /// El valor se serializa a JSON antes de guardar.
    /// </remarks>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Elimina un valor específico de la caché.
    /// </summary>
    /// <param name="key">Clave del valor a eliminar.</param>
    /// <returns>Task completo.</returns>
    /// <remarks>
    /// Si la clave no existe, la operación es idempotente y no lanza error.
    /// </remarks>
    Task RemoveAsync(string key);

    /// <summary>
    /// Elimina todas las entradas de caché que coincidan con un patrón.
    /// </summary>
    /// <param name="pattern">Patrón de búsqueda (ejemplo: "product:*" para todas las claves de productos).</param>
    /// <returns>Task completo.</returns>
    /// <remarks>
    /// Útil para invalidar caché relacionada cuando se actualizan datos.
    /// Por ejemplo, al actualizar un producto, eliminar "product:*" para forzar recarga.
    /// </remarks>
    Task RemoveByPatternAsync(string pattern);
}