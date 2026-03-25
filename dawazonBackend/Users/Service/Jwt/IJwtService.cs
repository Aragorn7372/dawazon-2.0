using dawazonBackend.Users.Models;

namespace dawazonBackend.Users.Service.Jwt;

/// <summary>
/// Interfaz que define el contrato para el servicio de generación y validación de tokens JWT.
/// </summary>
/// <remarks>
/// Proporciona métodos para crear y verificar tokens de autenticación JWT.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Generación de tokens JWT firmados</item>
///     <item>Validación de tokens JWT</item>
///     <item>Extracción de información del usuario</item>
/// </list>
///
/// <para><b>Configuración requerida en appsettings.json:</b></para>
/// <code>
/// "Jwt": {
///   "Key": "tu-clave-secreta-minimo-32-caracteres",
///   "Issuer": "DawazonApi",
///   "Audience": "DawazonApi",
///   "ExpireMinutes": 60
/// }
/// </code>
/// </remarks>
public interface IJwtService
{
    /// <summary>
    /// Genera un token JWT firmado con la información del usuario.
    /// </summary>
    /// <param name="user">Usuario para el cual generar el token.</param>
    /// <returns>
    /// Una cadena que representa el token JWT generado.
    /// </returns>
    /// <remarks>
    /// <para><b>Claims incluidos en el token:</b></para>
    /// <list type="bullet">
    ///     <item>Sub: ID del usuario</item>
    ///     <item>Name: Nombre del usuario</item>
    ///     <item>Email: Correo electrónico del usuario</item>
    ///     <item>Role: Rol del usuario</item>
    ///     <item>Jti: Identificador único del token</item>
    /// </list>
    /// 
    /// <para><b>Configuración:</b></para>
    /// El token se genera con los valores de configuración:
    /// <list type="bullet">
    ///     <item>Issuer: Configurable (default: "TiendaApi")</item>
    ///     <item>Audience: Configurable (default: "TiendaApi")</item>
    ///     <item>Expiration: Configurable en minutos (default: 60)</item>
    ///     <item>Algorithm: HMAC-SHA256</item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Se lanza si la clave JWT no está configurada en appsettings.json.
    /// </exception>
    /// <example>
    /// <code>
    /// var user = await userManager.FindByNameAsync("john");
    /// var token = await jwtService.GenerateTokenAsync(user);
    /// // token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// </code>
    /// </example>
    Task<string> GenerateTokenAsync(User user);

    /// <summary>
    /// Valida un token JWT y extrae el nombre de usuario.
    /// </summary>
    /// <param name="token">Token JWT a validar.</param>
    /// <returns>
    /// El nombre de usuario contenido en el token, o null si el token es inválido.
    /// </returns>
    /// <remarks>
    /// <para><b>Validaciones realizadas:</b></para>
    /// <list type="bullet">
    ///     <item>Firma del token</item>
    ///     <item>Issuer</item>
    ///     <item>Audience</item>
    ///     <item>Fecha de expiración</item>
    /// </list>
    /// 
    /// <para><b>Nota:</b> El método retorna null en caso de cualquier error de validación,
    /// incluyendo token expirado, firma inválida, o formato incorrecto.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
    /// var username = jwtService.ValidateToken(token);
    /// if (username != null)
    /// {
    ///     Console.WriteLine($"Token válido para usuario: {username}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Token inválido o expirado");
    /// }
    /// </code>
    /// </example>
    string? ValidateToken(string token);
}
