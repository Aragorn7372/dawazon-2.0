namespace dawazonBackend.Cart.Dto;

/// <summary>
/// Data Transfer Object (DTO) utilizado para filtrar, paginar y ordenar consultas de carritos de compras.
/// </summary>
/// <remarks>
/// Este DTO es un record que define los parámetros de búsqueda y paginación
/// cuando se listan carritos desde la API o desde el panel de administración.
///
/// <para><b>Uso típico:</b></para>
/// Se utiliza como parámetro de consulta en endpoints como:
/// <list type="bullet">
///     <item>GET /cart/all - Listar todos los carritos con filtros</item>
///     <item>GET /cart/manager/{managerId} - Listar carritos de un manager</item>
/// </list>
/// 
/// <para><b>Valores por defecto:</b></para>
/// <list type="bullet">
///     <item>Page = 0 (primera página)</item>
///     <item>Size = 10 (10 elementos por página)</item>
///     <item>SortBy = "id" (ordenar por ID)</item>
///     <item>Direction = "asc" (orden ascendente)</item>
/// </list>
/// </remarks>
public record FilterCartDto(
    /// <summary>
    /// Filtrar por identificador del manager o vendedor.
    /// </summary>
    /// <value>Identificador numérico del manager o null para no filtrar.</value>
    /// <remarks>
    /// Cuando se proporciona, solo retorna los carritos asignados o procesados
    /// por el manager especificado. Útil para el panel de administración.
    /// </remarks>
    long? ManagerId,
    
    /// <summary>
    /// Indica si el usuario es administrador para ver todos los carritos.
    /// </summary>
    /// <value>True para incluir todos los carritos del sistema, false para filtrar por usuario.</value>
    /// <remarks>
    /// Cuando es true, se ignoran las restricciones de usuario y se muestran todos.
    /// Requiere que el usuario tenga permisos de administrador.
    /// </remarks>
    bool? IsAdmin,
    
    /// <summary>
    /// Filtrar por estado de compra del carrito.
    /// </summary>
    /// <value>True para solo carritos comprados, false para solo activos, null para todos.</value>
    /// <remarks>
    /// Permite separar carritos activos (en proceso) de historicos (ya comprados).
    /// </remarks>
    bool? Purchased,
    
    /// <summary>
    /// Número de página para paginación (0-indexed).
    /// </summary>
    /// <value>Entero indicando la página a recuperar. Por defecto 0 (primera página).</value>
    /// <remarks>
    /// Se combina con Size para implementar paginación. La primera página es 0.
    /// </remarks>
    int Page = 0,
    
    /// <summary>
    /// Cantidad de elementos por página.
    /// </summary>
    /// <value>Entero con el número de elementos a devolver. Por defecto 10.</value>
    /// <remarks>
    /// Controla el tamaño de la página de resultados. Valores típicos: 10, 25, 50, 100.
    /// </remarks>
    int Size = 10,
    
    /// <summary>
    /// Campo por el cual ordenar los resultados.
    /// </summary>
    /// <value>String con el nombre del campo de ordenación. Por defecto "id".</value>
    /// <remarks>
    /// Campos comunes: "id", "total", "createAt", "updateAt", "purchased".
    /// </remarks>
    string SortBy = "id",
    
    /// <summary>
    /// Dirección de ordenación de los resultados.
    /// </summary>
    /// <value>"asc" para orden ascendente, "desc" para orden descendente.</value>
    /// <remarks>
    /// Por defecto es ascendente (a-z, 0-9).
    /// </remarks>
    string Direction = "asc" 
);