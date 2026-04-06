using System.Text.Json;

namespace dawazon2._0.Session;

/// <summary>
/// Métodos de extensión para gestionar objetos JSON en la sesión HTTP.
/// </summary>
/// <remarks>
/// Permite almacenar y recuperar objetos complejos en la sesión serializándolos como JSON.
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Serialización automática con System.Text.Json</item>
///     <item>Compatibilidad con cualquier tipo genérico</item>
/// </list>
/// </remarks>
public static class SessionExtensions {
    /// <summary>
    /// Guarda un objeto en la sesión como JSON.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Serializa el objeto a JSON</item>
    ///     <item>Almacena en la sesión con la clave especificada</item>
    /// </list>
    /// </remarks>
    /// <typeparam name="T">Tipo del objeto a guardar.</typeparam>
    /// <param name="session">Sesión HTTP.</param>
    /// <param name="key">Clave para identificar el valor.</param>
    /// <param name="value">Objeto a guardar.</param>
    public static void SetJson<T>(this ISession session, string key, T value) =>
        session.SetString(key, JsonSerializer.Serialize(value));

    /// <summary>
    /// Recupera un objeto de la sesión por su clave.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    ///     <item>Obtiene el string JSON de la sesión</item>
    ///     <item>Deserializa al tipo especificado</item>
    ///     <item>Retorna null si no existe la clave</item>
    /// </list>
    /// </remarks>
    /// <typeparam name="T">Tipo del objeto a recuperar.</typeparam>
    /// <param name="session">Sesión HTTP.</param>
    /// <param name="key">Clave del valor a recuperar.</param>
    /// <returns>Objeto deserializado o null.</returns>
    public static T? GetJson<T>(this ISession session, string key) {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}