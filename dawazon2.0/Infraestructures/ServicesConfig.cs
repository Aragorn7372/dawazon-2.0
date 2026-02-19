using dawazonBackend.Cart.Service;
using dawazonBackend.Products.Service;
using dawazonBackend.Stripe;
using dawazonBackend.Users.Service;
using Serilog;

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
            .AddScoped<IProductService, ProductService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<ICartService, CartService>()
            .AddScoped<IStripeService, StripeService>();
    }
}