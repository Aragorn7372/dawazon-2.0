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

public static class ServicesConfig
{
    /// <summary>
    /// Registra todos los servicios de negocio en el contenedor de dependencias.
    /// </summary>
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