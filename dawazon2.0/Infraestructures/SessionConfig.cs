namespace dawazon2._0.Infraestructures;

public static class SessionConfig
{
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