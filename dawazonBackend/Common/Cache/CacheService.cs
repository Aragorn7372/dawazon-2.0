using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace dawazonBackend.Common.Cache;

/// <summary>
/// Implementación del servicio de caché utilizando Redis como almacenamiento distribuido.
/// </summary>
/// <remarks>
/// Esta clase implementa la interfaz <see cref="ICacheService"/> y utiliza
/// <see cref="IDistributedCache"/> de ASP.NET Core para el almacenamiento.
///
/// <para><b>Patrón de diseño:</b></para>
/// <list type="bullet">
///     <item>Cache-Aside: La caché se populate bajo demanda, no automáticamente</item>
///     <item>Serialization: Los objetos se serializan a JSON para almacenarse</item>
///     <item>Expiración configurable: Cada entrada puede tener su propio TTL</item>
/// </list>
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item><see cref="IDistributedCache"/> - Interfaz de caché distribuida de ASP.NET Core</item>
///     <item>ILogger&lt;CacheService&gt; - Para logging de operaciones</item>
/// </list>
/// 
/// <para><b>Serialización:</b></para>
/// Usa <see cref="JsonSerializer"/> con opciones configuradas para nombres de propiedades
/// case-insensitive. Esto permite que los nombres en JSON coincidan independientemente del caso.
///
/// <para><b>Manejo de errores:</b></para>
/// Todas las operaciones capturan excepciones y las logged, retornando valores por defecto
/// en lugar de lanzar errores. Esto previene que fallos de caché afecten la funcionalidad.
/// </remarks>
public class CacheService(
    IDistributedCache cache,
    ILogger<CacheService> logger
) : ICacheService
{
    /// <summary>
    /// Opciones de serialización JSON compartidas.
    /// </summary>
    /// <remarks>
    /// PropertyNameCaseInsensitive = true permite deserializar correctamente
    /// aunque el caso de las propiedades no coincida exactamente.
    /// </remarks>
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc/>
    /// <summary>
    /// Obtiene un valor de la caché, deserializándolo desde JSON.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Busca el valor en Redis por la clave</item>
    ///     <item>Si no existe o está vacío, retorna default (cache miss)</item>
    ///     <item>Si existe, deserializa el JSON al tipo especificado</item>
    /// </list>
    /// 
    /// <para><b>Logging:</b></para>
    /// Registra "Cache hit" o "Cache miss" para debugging.
/// </remarks>
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedValue = await cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedValue))
            {
                logger.LogDebug("Cache miss para clave: {Key}", key);
                return default;
            }

            logger.LogDebug("Cache hit para clave: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener valor de caché para clave: {Key}", key);
            return default;
        }
    }

    /// <inheritdoc/>
    /// <summary>
    /// Guarda un valor en la caché, serializándolo a JSON.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Serializa el objeto a JSON</item>
    ///     <item>Crea opciones de caché con TTL (default 5 minutos)</item>
    ///     <item>Guarda en Redis</item>
    /// </list>
    /// 
    /// <para><b>TTL por defecto:</b></b>
    /// Si no se especifica expiration, usa 5 minutos.
    /// Esto es un balance entre frescura de datos y carga de servidor.
    /// </remarks>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var jsonValue = JsonSerializer.Serialize(value, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            await cache.SetStringAsync(key, jsonValue, options);

            logger.LogDebug("Valor cacheado para clave: {Key} con expiración: {Expiration}",
                key, expiration ?? TimeSpan.FromMinutes(5));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al guardar en caché para clave: {Key}", key);
        }
    }

    /// <inheritdoc/>
    /// <summary>
    /// Elimina un valor específico de la caché.
    /// </summary>
    /// <remarks>
    /// Operacion idempotente: si la clave no existe, no lanza error.
    /// </remarks>
    public async Task RemoveAsync(string key)
    {
        try
        {
            await cache.RemoveAsync(key);
            logger.LogDebug("Entrada de caché eliminada para clave: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al eliminar de caché para clave: {Key}", key);
        }
    }

    /// <inheritdoc/>
    /// <summary>
    /// Elimina todas las entradas de caché que coincidan con un patrón.
    /// </summary>
    /// <remarks>
    /// <para><b>Nota de implementación:</b></b>
    /// La implementación actual es un placeholder. Una implementación completa
    /// usaría SCAN de Redis para encontrar claves que coincidan y eliminarlas.
    /// </remarks>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            logger.LogDebug("Eliminando entradas de caché que coinciden con patrón: {Pattern}", pattern);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al eliminar entradas de caché por patrón: {Pattern}", pattern);
        }
    }
}