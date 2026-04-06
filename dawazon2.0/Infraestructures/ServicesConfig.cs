using dawazon2._0.Pdf;
using dawazonBackend.Cart.Service;
using dawazonBackend.Products.Service;
using dawazonBackend.Stripe;
using dawazonBackend.Users.Service;
using dawazonBackend.Users.Service.Auth;
using dawazonBackend.Users.Service.Jwt;
using Serilog;
using dawazonBackend.Users.Service.Favs;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Registro de servicios de negocio.
/// </summary>
/// <remarks>
/// Registra todos los servicios de negocio en el contenedor DI.
///
/// <para><b>Servicios registrados:</b></para>
/// <list type="bullet">
///     <item>IAuthService - Autenticación</item>
///     <item>IJwtService / JwtTokenExtractor - JWT</item>
///     <item>IProductService - Productos</item>
///     <item>IUserService - Usuarios</item>
///     <item>ICartService - Carrito/Ventas</item>
///     <item>IStripeService - Pagos</item>
///     <item>IFavService - Favoritos</item>
///     <item>IOrderPdfService - PDFs</item>
/// </list>
/// </remarks>
public static class ServicesConfig
{
    /// <summary>
    /// Registra todos los servicios de negocio.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        Log.Information("⚙️ Registrando servicios...");
        return services
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IJwtService, JwtService>()
            .AddScoped<IJwtTokenExtractor,  JwtTokenExtractor>()
            .AddScoped<IProductService, ProductService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<ICartService, CartService>()
            .AddScoped<IStripeService, StripeService>()
            .AddScoped<IFavService, FavService>()
            .AddScoped<IOrderPdfService, OrderPdfService>();
    }
}