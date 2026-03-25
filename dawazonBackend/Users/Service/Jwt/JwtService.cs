using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace dawazonBackend.Users.Service.Jwt;

/// <summary>
/// Implementación de <see cref="IJwtService"/> para generación y validación de tokens JWT.
/// </summary>
/// <remarks>
/// Proporciona la lógica de negocio para crear y verificar tokens JWT de autenticación.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item><see cref="IConfiguration"/>: Para acceder a la configuración de JWT</item>
///     <item><see cref="ILogger{JwtService}"/>: Para logging de operaciones</item>
///     <item><see cref="UserManager{User}"/>: Para obtener roles del usuario</item>
/// </list>
///
/// <para><b>Configuración requerida:</b></para>
/// La clave JWT debe estar configurada en appsettings.json en la sección "Jwt:Key".
/// </remarks>
public class JwtService(
    IConfiguration configuration,
    ILogger<JwtService> logger,
    UserManager<User> userManager
) : IJwtService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<JwtService> _logger = logger;

    /// <inheritdoc/>
    /// <summary>
    /// Genera un token JWT firmado con la información del usuario.
    /// </summary>
    /// <param name="user">Usuario para el token.</param>
    /// <returns>Token JWT firmado como cadena.</returns>
    /// <exception cref="InvalidOperationException">Si la clave JWT no está configurada.</exception>
    /// <remarks>
    /// <para><b>Proceso de generación:</b></para>
    /// <list type="number">
    ///     <item>Obtiene configuración de JWT (Key, Issuer, Audience, ExpireMinutes)</item>
    ///     <item>Crea clave de seguridad simétrica</item>
    ///     <item>Obtiene roles del usuario</item>
    ///     <li>Crea claims con información del usuario</item>
    ///     <li>Genera el token firmado</item>
    /// </list>
    /// 
    /// <para><b>Claims incluidos:</b></para>
    /// <list type="bullet">
    ///     <item>Sub: ID del usuario</item>
    ///     <item>Name: Nombre del usuario</item>
    ///     <item>Email: Email del usuario</item>
    ///     <item>Role: Rol del usuario (primer rol)</item>
    ///     <item>Jti: Identificador único del token</item>
    /// </list>
    /// </remarks>
    public async Task<string> GenerateTokenAsync(User user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key no configurada");
        var issuer = _configuration["Jwt:Issuer"] ?? "TiendaApi";
        var audience = _configuration["Jwt:Audience"] ?? "TiendaApi";
        var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var roles = await userManager.GetRolesAsync(user);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Name ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Role, roles[0]),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("Token JWT generado para usuario: {Username}", user.Name);
        
        return tokenString;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Valida un token JWT y extrae el nombre de usuario.
    /// </summary>
    /// <param name="token">Token JWT a validar.</param>
    /// <returns>Nombre de usuario del token o null si es inválido.</returns>
    /// <remarks>
    /// <para><b>Validaciones realizadas:</b></para>
    /// <list type="bullet">
    ///     <item>Validar firma del token</item>
    ///     <item>Validar issuer</item>
    ///     <item>Validar audience</item>
    ///     <item>Validar fecha de expiración</item>
    /// </list>
    /// 
    /// <para><b>Nota:</b> Utiliza ClockSkew = TimeSpan.Zero para validación estricta del tiempo.</para>
    /// </remarks>
    public string? ValidateToken(string token)
    {
        try
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key no configurada");
            var issuer = _configuration["Jwt:Issuer"] ?? "TiendaApi";
            var audience = _configuration["Jwt:Audience"] ?? "TiendaApi";

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var username = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Name).Value;

            _logger.LogDebug("Token JWT validado exitosamente");
            return username;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Validación de token JWT fallida");
            return null;
        }
    }
}
