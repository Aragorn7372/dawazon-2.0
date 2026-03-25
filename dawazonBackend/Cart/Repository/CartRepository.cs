using System.Linq.Expressions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Exceptions;
using dawazonBackend.Cart.Models;
using dawazonBackend.Common.Database;
using dawazonBackend.Common.Dto;
using Microsoft.EntityFrameworkCore;

namespace dawazonBackend.Cart.Repository;

/// <summary>
/// Implementación del repositorio de carritos utilizando Entity Framework Core.
/// </summary>
/// <remarks>
/// Esta clase implementa la interfaz <see cref="ICartRepository"/> y proporciona
/// todas las operaciones de acceso a datos para el módulo de carrito.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Utiliza <see cref="DawazonDbContext"/> para el acceso a datos</item>
///     <item>Implementa Unit of Work implícito mediante SaveChanges</item>
///     <item>Utiliza Include() para cargar relaciones</item>
///     <item>Soporta paginación, filtrado y ordenación</item>
/// </list>
/// 
/// <para><b>Patrones aplicados:</b></para>
/// <list type="bullet">
///     <item>Repository Pattern: Abstrae el acceso a datos</item>
///     <item>Specification Pattern: FilterCartDto para especificaciones</item>
///     <item>Lazy Loading: A través de Include() y ThenInclude()</item>
/// </list>
/// 
/// <para><b>Consideraciones de rendimiento:</b></para>
/// <list type="bullet">
///     <item>Usa AsNoTracking() para consultas de solo lectura</item>
///     <item>Calcula agregados en la base de datos (Sum, Count)</item>
///     <item>Pagina siempre los resultados grandes</item>
/// </list>
/// </remarks>
public class CartRepository(
    DawazonDbContext context,
    ILogger<CartRepository> logger
    ): ICartRepository
{
    /// <inheritdoc/>
    /// <summary>
    /// Obtiene todos los carritos aplicando filtros, ordenación y paginación.
    /// </summary>
    /// <remarks>
    /// Este método aplica los filtros definidos en <paramref name="filter"/>, incluyendo:
    /// <list type="bullet">
    ///     <item>Filtrado por estado de compra (purchased)</item>
    ///     <li>Ordenación por id, total, createdAt o userId</item>
    ///     <li>Paginación con Page y Size</item>
    /// </list>
    /// 
    /// <para><b> eager loading:</b></para>
    /// Carga las líneas de carrito y los productos relacionados.
    /// </remarks>
    public async Task<(IEnumerable<Models.Cart> Items, int TotalCount)> GetAllAsync(FilterCartDto filter)
    {
        var query = context.Carts.AsQueryable();

        // Aplicar filtros si vienen en el DTO
        if (filter.Purchased != null)
        {
            query = query.Where(c => c.Purchased == filter.Purchased);
        }

        var totalCount = await query.CountAsync();

        // Ordenación
        bool isDesc = filter.Direction.ToLower() == "desc";

        query = filter.SortBy.ToLower() switch
        {
            "total" => isDesc ? query.OrderByDescending(c => c.Total) : query.OrderBy(c => c.Total),
            "createdat" => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            "userid" => isDesc ? query.OrderByDescending(c => c.UserId) : query.OrderBy(c => c.UserId),
            _ => isDesc ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id)
        };

        // Paginación
        int skip = filter.Page * filter.Size;

        var items = await query
            .Include(c => c.CartLines)
                .ThenInclude(cl => cl.Product)
            .Skip(skip)
            .Take(filter.Size)
            .ToListAsync();

        return (items, totalCount);
    }
    
    /// <inheritdoc/>
    /// <summary>
    /// Calcula las ganancias totales de ventas, opcionalmente filtradas por manager.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Solo considera carritos comprados (Purchased = true)</item>
    ///     <item>Si es admin y tiene managerId, filtra por productos de ese manager</item>
    ///     <item>Si no es admin y no tiene managerId, retorna 0</item>
    ///     <li>Calcula Sum(ProductPrice * Quantity) en la base de datos</item>
    /// </list>
    /// 
    /// <para><b>Nota técnica:</b></b>
    /// No usa la propiedad calculada TotalPrice porque EF Core no puede
    /// traducirla a SQL directamente.
    /// </remarks>
    public async Task<double> CalculateTotalEarningsAsync(long? managerId, bool isAdmin)
    {
        logger.LogInformation($"Calculando ganancias totales - Manager: {managerId}, isAdmin: {isAdmin}");

        // Si no es admin y no manda managerId, no debe ver nada
        if (!isAdmin && !managerId.HasValue)
        {
            return 0;
        }

        // Navegamos de Carritos Comprados -> Líneas de Carrito
        var query = context.Carts
            .Where(c => c.Purchased == true)
            .SelectMany(c => c.CartLines)
            .AsQueryable();

        // Si hay managerId, filtramos por sus productos
        if (managerId.HasValue)
        {
            query = query.Where(cl => cl.Product != null && cl.Product.CreatorId == managerId.Value);
        }

        // Sumamos calculando Precio * Cantidad directamente en la BBDD. 
        // No usamos la propiedad calculada TotalPrice porque EF Core no la puede traducir a SQL.
        return await query.SumAsync(cl => cl.ProductPrice * cl.Quantity);
    }
    
    /// <inheritdoc/>
    /// <summary>
    /// Actualiza el estado de una línea específica dentro de un carrito.
    /// </summary>
    /// <param name="id">ID del carrito.</param>
    /// <param name="productId">ID del producto.</param>
    /// <param name="status">Nuevo estado.</param>
    /// <returns>True si se actualizó, false si no se encontró el carrito.</returns>
    /// <remarks>
    /// Busca el carrito por ID y actualiza solo la línea que coincide con el productId.
    /// </remarks>
    public async Task<bool> UpdateCartLineStatusAsync(string id, string productId, Status status)
    {
        logger.LogInformation($"Actualizando linea de carrito {id} con status {status}");
        
        var oldCart = await context.Carts.Include(c => c.CartLines)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (oldCart == null) return false;
        
        oldCart.CartLines.Find(cl => cl.ProductId == productId)!.Status = status;
        context.Carts.Update(oldCart);
        await context.SaveChangesAsync();
        
        return true;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Busca carritos por ID de usuario con filtros adicionales.
    /// </summary>
    /// <remarks>
    /// Método no implementado actualmente. Lanza <see cref="NotImplementedException"/>.
    /// </remarks>
    public Task<IEnumerable<Models.Cart>> FindByUserIdAsync(long userId, FilterCartDto filter)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    /// <summary>
    /// Añade o actualiza una línea de producto en el carrito.
    /// </summary>
    /// <param name="cartId">ID del carrito.</param>
    /// <param name="cartLine">Línea a añadir.</param>
    /// <returns>True si se añadió/actualizó, false si el carrito no existe.</returns>
    /// <remarks>
    /// Si ya existe una línea con el mismo ProductId, actualiza la cantidad.
    /// Si no existe, añade la nueva línea al carrito.
    /// </remarks>
    public async Task<bool> AddCartLineAsync(string cartId, CartLine cartLine)
    {
        logger.LogInformation($"Añadiendo línea de carrito al carrito con ID: {cartId}");
    
        var cart = await context.Carts.Include(c => c.CartLines)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    
        if(cart == null) return false;

        var existingLine = cart.CartLines
            .FirstOrDefault(cl => cl.ProductId == cartLine.ProductId);

        if (existingLine != null) existingLine.Quantity = cartLine.Quantity; 
        
        else cart.CartLines.Add(cartLine);
        
        context.Carts.Update(cart);
        await context.SaveChangesAsync();
    
        return true;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Elimina una línea de producto del carrito.
    /// </summary>
    /// <param name="cartId">ID del carrito.</param>
    /// <param name="cartLine">Línea a eliminar.</param>
    /// <returns>True si se eliminó, false si no se encontró el carrito o la línea.</returns>
    public async Task<bool> RemoveCartLineAsync(string cartId, CartLine cartLine)
    {
        logger.LogInformation($"Deletando linea de carrito {cartId}");
        var cart = await context.Carts.Include(c => c.CartLines)
            .FirstOrDefaultAsync(c => c.Id == cartId);
        if(cart == null) return false;
        var lineToRemove = cart.CartLines.FirstOrDefault(cl => cl.ProductId == cartLine.ProductId);
        if (lineToRemove != null) cart.CartLines.Remove(lineToRemove);        
        context.Carts.Update(cart);
        await context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Busca un carrito por usuario y estado de compra.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="purchased">Estado de compra.</param>
    /// <returns>Carrito encontrado o null.</returns>
    /// <remarks>
    /// Usa AsNoTracking() para optimización de memoria en consultas de solo lectura.
    /// Carga cliente, dirección y líneas con productos.
    /// </remarks>
    public async Task<Models.Cart?> FindByUserIdAndPurchasedAsync(long userId, bool purchased)
    {
        logger.LogInformation($"bucando carrito con  ID: {userId} y estatus {purchased}");
        return await context.Carts
            .AsNoTracking()
            .Include(c => c.CartLines)
                .ThenInclude(cl => cl.Product)
            .Include(c => c.Client)
                .ThenInclude(cl => cl.Address)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Purchased == purchased);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Busca un carrito por su identificador único.
    /// </summary>
    /// <param name="cartId">ID del carrito.</param>
    /// <returns>Carrito encontrado o null.</returns>
    /// <remarks>
    /// Carga automáticamente las líneas de carrito, productos y cliente.
    /// </remarks>
    public async Task<Models.Cart?> FindCartByIdAsync(string cartId)
    {
        logger.LogInformation($"cartId: {cartId}");
        return await context.Carts
            .Include(c => c.CartLines)
                .ThenInclude(cl => cl.Product)
            .Include(c => c.Client)
                .ThenInclude(cl => cl.Address)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Crea un nuevo carrito en la base de datos.
    /// </summary>
    /// <param name="cart">Carrito a crear.</param>
    /// <returns>El carrito creado con ID generado.</returns>
    public async Task<Models.Cart> CreateCartAsync(Models.Cart cart)
    {
        logger.LogInformation($"creando carrito");
        var saved=await context.Carts.AddAsync(cart);
        await context.SaveChangesAsync();
        return saved.Entity;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Actualiza los datos de un carrito existente.
    /// </summary>
    /// <param name="id">ID del carrito.</param>
    /// <param name="cart">Nuevos datos.</param>
    /// <returns>Carrito actualizado o null si no existe.</returns>
    /// <remarks>
    /// Solo actualiza campos específicos: Client, Total, TotalItems, Purchased,
    /// CheckoutInProgress, CheckoutStartedAt y UploadAt.
    /// </remarks>
    public async Task<Models.Cart?> UpdateCartAsync(string id, Models.Cart cart)
    {
        var oldCart = await context.Carts.Include(c => c.Client)
            .ThenInclude(cl => cl.Address).FirstOrDefaultAsync(c => c.Id == id);
        if (oldCart == null) return null;
        oldCart.Client=cart.Client;
        oldCart.Total=cart.Total;
        oldCart.TotalItems=cart.TotalItems;
        oldCart.Purchased=cart.Purchased;
        oldCart.CheckoutInProgress=cart.CheckoutInProgress;
        oldCart.CheckoutStartedAt=cart.CheckoutStartedAt;
        oldCart.UploadAt= DateTime.UtcNow;
        var saved=context.Carts.Update(oldCart);
        await context.SaveChangesAsync();
        return saved.Entity;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Actualiza solo los valores escalares del carrito.
    /// </summary>
    /// <remarks>
    /// Optimización para actualizar solo TotalItems y Total sin tocar las líneas.
    /// </remarks>
    public async Task UpdateCartScalarsAsync(string cartId, int totalItems, double total)
    {
        var cart = await context.Carts.FindAsync(cartId);
        if (cart == null) return;

        cart.TotalItems = totalItems;
        cart.Total      = total;
        cart.UploadAt   = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    /// <summary>
    /// Elimina un carrito de la base de datos.
    /// </summary>
    /// <exception cref="CartNotFoundException">Si el carrito no existe.</exception>
    public async Task DeleteCartAsync(string id)
    {
        var cart = await context.Carts.FindAsync(id);

        if (cart == null)
            throw new CartNotFoundException("No se encontro carrito");

        context.Carts.Remove(cart);

        await context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Aplica ordenación dinámica a una consulta de carritos.
    /// </summary>
    /// <remarks>
    /// Método auxiliar privado para ordenar por diferentes campos.
    /// Soporta: Purchased, Total, CreatedAt, UploadAt, Id.
    /// </remarks>
    private static IQueryable<Models.Cart> ApplySorting(IQueryable<Models.Cart> query, string sortBy, string direction)
    {
        var isDescending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        Expression<Func<Models.Cart,object>> keySelector = sortBy.ToLower() switch
        {
            "Comprado" => p => p.Purchased,
            "precio" => p => p.Total,
            "createdat" => p => p.CreatedAt,
            "ultima modificacion" => p => p.UploadAt,
            _ => p => p.Id!
        };
        return isDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
    
    /// <inheritdoc/>
    /// <summary>
    /// Obtiene las ventas como líneas individuales con información de manager.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Aplana las líneas de carritos comprados</item>
    ///     <li>Une con tabla de Usuarios para obtener nombre del manager</item>
    ///     <li>Aplica filtros por permisos (admin vs manager)</item>
    ///     <li>Pagina y ordena por fecha de creación</item>
    /// </list>
    /// 
    /// <para><b>Nota técnica:</b></para>
    /// El enum Status se extrae como string para evitar problemas de EF Core
    /// al traducir a SQL, y se parsea en memoria después.
    /// </remarks>
    public async Task<(List<SaleLineDto> Items, int TotalCount)> GetSalesAsLinesAsync(
    long? managerId, 
    bool isAdmin, 
    FilterDto filter)
    {
        // Aplanamos las líneas de los carritos comprados (SQL INNER JOIN implícito)
        var query = context.Carts
            .AsNoTracking()
            .Where(c => c.Purchased==true)
            .SelectMany(
                cart => cart.CartLines,
                (cart, line) => new { cart, line, product = line.Product }
            )
            // Unimos con la tabla Users para obtener al Manager (Creador del producto)
            .Join(
                context.Users,
                objetoAnonimo => objetoAnonimo.product!.CreatorId,
                u => u.Id,
                (objetoAnonimo, manager) => new { objetoAnonimo.cart, objetoAnonimo.line, objetoAnonimo.product, manager }
            );

        // Filtramos por permisos en la consulta a la BBDD
        if (!isAdmin && managerId.HasValue)
        {
            query = query.Where(x => x.product!.CreatorId == managerId.Value);
        }

        // Proyectamos a un tipo anónimo temporal. 
        // Hacemos esto porque Entity Framework no sabe traducir el Enum 'Status' a SQL de forma nativa si está guardado como string.
        var projection = query.Select(objetoAnonimo => new 
        {
            SaleId = objetoAnonimo.cart.Id,
            ProductId = objetoAnonimo.product!.Id,
            ProductName = objetoAnonimo.product.Name,
            Quantity = objetoAnonimo.line.Quantity,
            ProductPrice = objetoAnonimo.line.ProductPrice,
            StatusStr = objetoAnonimo.line.Status, // Extraemos el string tal cual de la BBDD
            ManagerId = objetoAnonimo.product.CreatorId,
            ManagerName = objetoAnonimo.manager.Name,
            Client = objetoAnonimo.cart.Client,
            UserId = objetoAnonimo.cart.UserId,
            CreateAt = objetoAnonimo.cart.CreatedAt,
            UpdateAt = objetoAnonimo.cart.UploadAt
        });

        // Contamos el total de elementos ANTES de paginar
        var totalCount = await projection.CountAsync();

        // Ordenación dinámica simple
        projection = filter.Direction.ToLower() == "desc" 
            ? projection.OrderByDescending(x => x.CreateAt)
            : projection.OrderBy(x => x.CreateAt);

        // Paginación y ejecución de la consulta (aquí es donde realmente ataca a la BBDD)
        var dbItems = await projection
            .Skip(filter.Page * filter.Size)
            .Take(filter.Size)
            .ToListAsync();

        // Por último, mapeamos en memoria al DTO final parseando el Enum
        var finalItems = dbItems.Select(x => new SaleLineDto
        {
            SaleId = x.SaleId,
            ProductId = x.ProductId!,
            ProductName = x.ProductName,
            Quantity = x.Quantity,
            ProductPrice = x.ProductPrice,
            TotalPrice = x.Quantity * x.ProductPrice,
            Status = x.StatusStr,
            ManagerId = x.ManagerId,
            ManagerName = x.ManagerName,
            Client = x.Client,
            UserId = x.UserId,
            CreateAt = x.CreateAt,
            UpdateAt = x.UpdateAt
        }).ToList();

        return (finalItems, totalCount);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Cuenta ventas nuevas de un manager desde una fecha.
    /// </summary>
    /// <remarks>
    /// Solo considera carritos comprados modificados después de <paramref name="since"/>
    /// y productos creados por el manager especificado.
    /// </remarks>
    public async Task<int> CountNewSalesAsync(long managerId, DateTime since)
    {
        return await context.Carts
            .AsNoTracking()
            .Where(c => c.Purchased == true && c.UploadAt > since)
            .SelectMany(cart => cart.CartLines)
            .Where(line => line.Product != null && line.Product.CreatorId == managerId)
            .CountAsync();
    }

    /// <inheritdoc/>
    /// <summary>
    /// Obtiene el total de líneas de pedido en carritos comprados.
    /// </summary>
    /// <returns>Count total de líneas.</returns>
    public async Task<int> GetTotalSalesCountAsync()
    {
        return await context.Carts
            .AsNoTracking()
            .Where(c => c.Purchased == true)
            .SelectMany(cart => cart.CartLines)
            .CountAsync();
    }
}