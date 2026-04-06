using dawazonBackend.Cart.Repository;
using dawazonBackend.Products.Repository.Categoria;
using dawazonBackend.Products.Repository.Productos;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace dawazon2._0.Infraestructures;

/// <summary>
/// Registro de repositorios.
/// </summary>
/// <remarks>
/// Registra los repositorios de acceso a datos en el contenedor DI.
///
/// <para><b>Repositorios registrados:</b></para>
/// <list type="bullet">
///     <item>ICategoriaRepository - Categorías</item>
///     <item>IProductRepository - Productos</item>
///     <item>UserManager - Usuarios</item>
///     <item>ICartRepository - Carrito/Ventas</item>
/// </list>
/// </remarks>
public static class RepositoriesConfig
{
    /// <summary>
    /// Registra los repositorios.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>IServiceCollection.</returns>
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