using dawazonBackend.Cart.Repository;
using dawazonBackend.Products.Repository.Categoria;
using dawazonBackend.Products.Repository.Productos;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace dawazon2._0.Infraestructures;

public static class RepositoriesConfig
{
    /// <summary>
    /// Registra todos los repositorios en el contenedor de dependencias.
    /// 
    /// <para>
    /// El repositorio de pedidos se elige según configuration["Pedidos:RepositoryType"]:
    /// <list type="bullet">
    ///   <item><b>MongoDbNative:</b> Usa PedidosNativeRepository (driver nativo, funcional)</item>
    ///   <item><b>MongoDbEfCore:</b> Usa PedidosEfCoreRepository (Entity Framework Core, tiene bug EF-272)</item>
    /// </list>
    /// </para>
    /// </summary>
    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        Log.Information(" Registrando repositorios...");

        // Repositorios que no dependen de MongoDB
        services.AddScoped<ICategoriaRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<UserManager<User>, UserManager<User>>();
        services.AddScoped<ICartRepository, CartRepository>();

        return services;
    }
}