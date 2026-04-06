namespace dawazon2._0.Infraestructures;

/// <summary>
/// Configuración de sesiones HTTP.
/// </summary>
/// <remarks>
/// Configura el middleware de sesiones para almacenar datos del usuario.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>IdleTimeout: 30 minutos</item>
///     <item>Cookie: HttpOnly, IsEssential</li>
///     <item>SameSite: Lax</li>
/// </list>
/// </remarks>
public static class SessionConfig
{
    /// <summary>
    /// Configura los servicios de sesión.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la app.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddSession(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });
        
        return services;
    }
}