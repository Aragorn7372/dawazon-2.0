using System.Security.Claims;

namespace dawazonBackend.Users.Service.Jwt;

/// <summary>
/// Interfaz que define el contrato para extraer información de tokens JWT.
/// </summary>
/// <remarks>
/// Proporciona métodos para extraer diferentes partes de información de un token JWT
/// sin necesidad de validar la firma.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Extracción de ID de usuario</item>
///     <item>Extracción de rol</item>
///     <item>Verificación de rol de administrador</item>
///     <item>Extracción de email</item>
///     <item>Extracción de todos los claims</item>
///     <item>Validación de formato de token</item>
/// </list>
///
/// <para><b>Uso típico:</b></para>
/// <code>
/// IJwtTokenExtractor extractor = httpContext.RequestServices.GetRequiredService&lt;IJwtTokenExtractor&gt;();
/// var token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
///
/// var userId = extractor.ExtractUserId(token);
/// var isAdmin = extractor.IsAdmin(token);
/// var userInfo = extractor.ExtractUserInfo(token);
/// </code>
/// </remarks>
public interface IJwtTokenExtractor
{
    /// <summary>
    /// Extrae el ID del usuario del token JWT.
    /// </summary>
    /// <param name="token">Token JWT del cual extraer el ID.</param>
    /// <returns>
    /// El ID del usuario como long, o null si no se puede extraer.
    /// </returns>
    /// <remarks>
    /// Busca en los claims: ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub, o "nameid".
    /// </remarks>
    /// <example>
    /// <code>
    /// var userId = extractor.ExtractUserId(token);
    /// if (userId.HasValue)
    /// {
    ///     Console.WriteLine($"User ID: {userId.Value}");
    /// }
    /// </code>
    /// </example>
    long? ExtractUserId(string token);
    
    /// <summary>
    /// Extrae el rol del usuario del token JWT.
    /// </summary>
    /// <param name="token">Token JWT del cual extraer el rol.</param>
    /// <returns>
    /// El nombre del rol, o null si no se puede extraer.
    /// </returns>
    /// <remarks>
    /// Busca en los claims: ClaimTypes.Role o "role".
    /// </remarks>
    /// <example>
    /// <code>
    /// var role = extractor.ExtractRole(token);
    /// Console.WriteLine($"Rol: {role}");
    /// </code>
    /// </example>
    string? ExtractRole(string token);
    
    /// <summary>
    /// Determina si el token pertenece a un administrador.
    /// </summary>
    /// <param name="token">Token JWT a verificar.</param>
    /// <returns>
    /// true si el rol es "admin" (case-insensitive), false en caso contrario.
    /// </returns>
    /// <remarks>
    /// Utiliza comparación case-insensitive para el rol.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (extractor.IsAdmin(token))
    /// {
    ///     // Es administrador
    /// }
    /// </code>
    /// </example>
    bool IsAdmin(string token);
    
    /// <summary>
    /// Extrae toda la información relevante del usuario del token en una sola llamada.
    /// </summary>
    /// <param name="token">Token JWT del cual extraer la información.</param>
    /// <returns>
    /// Tupla con UserId, IsAdmin y Role.
    /// </returns>
    /// <remarks>
    /// Método optimizado para obtener toda la información en una sola operación.
    /// </remarks>
    /// <example>
    /// <code>
    /// var (userId, isAdmin, role) = extractor.ExtractUserInfo(token);
    /// </code>
    /// </example>
    (long? UserId, bool IsAdmin, string? Role) ExtractUserInfo(string token);
    
    /// <summary>
    /// Extrae todos los claims del token JWT.
    /// </summary>
    /// <param name="token">Token JWT del cual extraer los claims.</param>
    /// <returns>
    /// Un <see cref="ClaimsPrincipal"/> con todos los claims, o null si falla.
    /// </returns>
    /// <remarks>
    /// Si el token no puede ser parseado, intenta decodificar el payload manualmente.
    /// Normaliza los tipos de claim a tipos estándar de .NET.
    /// </remarks>
    /// <example>
    /// <code>
    /// var claimsPrincipal = extractor.ExtractClaims(token);
    /// var identity = claimsPrincipal?.Identity;
    /// </code>
    /// </example>
    ClaimsPrincipal? ExtractClaims(string token);
    
    /// <summary>
    /// Extrae el email del token JWT.
    /// </summary>
    /// <param name="token">Token JWT del cual extraer el email.</param>
    /// <returns>
    /// El email del usuario, o null si no se puede extraer.
    /// </returns>
    /// <remarks>
    /// Busca en los claims: JwtRegisteredClaimNames.Email o ClaimTypes.Email.
    /// </remarks>
    /// <example>
    /// <code>
    /// var email = extractor.ExtractEmail(token);
    /// </code>
    /// </example>
    string? ExtractEmail(string token);
    
    /// <summary>
    /// Valida el formato básico del token JWT sin verificar la firma.
    /// </summary>
    /// <param name="token">Token a validar.</param>
    /// <returns>
    /// true si el formato es válido, false en caso contrario.
    /// </returns>
    /// <remarks>
    /// <para><b>Validaciones:</b></para>
    /// <list type="bullet">
    ///     <item>No estar vacío</item>
    ///     <item>Tener 3 partes separadas por puntos</item>
    ///     <item>Header y payload no estar vacíos</item>
    /// </list>
    /// 
    /// <para>También detecta tokens con algoritmo "none".</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!extractor.IsValidTokenFormat(token))
    /// {
    ///     return BadRequest("Token inválido");
    /// }
    /// </code>
    /// </example>
    bool IsValidTokenFormat(string token);
}
