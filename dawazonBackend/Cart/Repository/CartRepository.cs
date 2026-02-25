using System.Linq.Expressions;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Exceptions;
using dawazonBackend.Cart.Models;
using dawazonBackend.Common.Database;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace dawazonBackend.Cart.Repository;

/// <summary>
/// Implementación del repositorio de carritos utilizando Entity Framework Core.
/// </summary>
public class CartRepository(
    DawazonDbContext context,
    ILogger<CartRepository> logger
    ): ICartRepository
{
    /// <inheritdoc/>
    public async Task<(IEnumerable<Models.Cart> Items, int TotalCount)> GetAllAsync(FilterCartDto filter)
    {
        var query = context.Carts.AsQueryable();

        // Aplicar filtros si vienen en el DTO
        if (filter.purchased != null)
        {
            query = query.Where(c => c.Purchased == filter.purchased);
        }

        // OBTENEMOS EL TOTAL ANTES DE PAGINAR
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
    public Task<IEnumerable<Models.Cart>> FindByUserIdAsync(long userId, FilterCartDto filter)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
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
    public async Task<Models.Cart> CreateCartAsync(Models.Cart cart)
    {
        logger.LogInformation($"creando carrito");
        var saved=await context.Carts.AddAsync(cart);
        await context.SaveChangesAsync();
        await context.Carts.Entry(cart).Reference(c=> c.Client).LoadAsync();
        await context.Carts.Entry(cart).Collection(c => c.CartLines).LoadAsync();
        await context.Entry(cart.Client).Reference(cl => cl.Address).LoadAsync();
        return saved.Entity;
    }

    /// <inheritdoc/>
    public async Task<Models.Cart?> UpdateCartAsync(string id, Models.Cart cart)
    {
        var oldCart = await context.Carts.Include(c => c.CartLines).Include(c => c.Client)
            .ThenInclude(cl => cl.Address).FirstOrDefaultAsync(c => c.Id == id);
        if (oldCart == null) return null;
        oldCart.Client=cart.Client;
        oldCart.CartLines.Clear();
        oldCart.CartLines.AddRange(cart.CartLines);
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
    public async Task UpdateCartScalarsAsync(string cartId, int totalItems, double total)
    {
        // FindAsync usa la clave primaria y devuelve la entidad SIN Include de navegación,
        // evitando el problema de Clear()+AddRange() sobre la misma referencia EF Core.
        var cart = await context.Carts.FindAsync(cartId);
        if (cart == null) return;

        cart.TotalItems = totalItems;
        cart.Total      = total;
        cart.UploadAt   = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteCartAsync(string id)
    {
        var cart = await context.Carts.FindAsync(id);

        if (cart == null)
            throw new CartNotFoundException("No se encontro carrito");

        context.Carts.Remove(cart);

        await context.SaveChangesAsync();
    }
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

    // Filtramos por permisos directamente en la consulta a la BBDD
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
}