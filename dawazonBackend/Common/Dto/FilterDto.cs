namespace dawazonBackend.Common.Dto;

/// <summary>
/// Record para filtros de paginación y ordenación en consultas genéricas.
/// </summary>
/// <remarks>
/// Se utiliza como parámetro en endpoints que soportan filtrado, paginación y ordenación.
/// 
/// <para><b>Valores por defecto:</b></para>
/// <list type="bullet">
///     <item>Page = 0 (primera página)</item>
///     <item>Size = 10 (10 elementos por página)</item>
///     <item>SortBy = "id" (ordenar por ID)</item>
///     <item>Direction = "asc" (orden ascendente)</item>
/// </list>
/// 
/// <para><b>Campos nullable:</b></para>
/// Nombre y Categoria pueden ser null si no se aplica el filtro.
/// </remarks>
public record FilterDto(
    /// <summary>
    /// Filtrar por nombre de producto (contiene).
    /// </summary>
    string? Nombre,
    
    /// <summary>
    /// Filtrar por nombre de categoría.
    /// </summary>
    string? Categoria,
    
    /// <summary>
    /// Número de página (0-indexed).
    /// </summary>
    int Page = 0,
    
    /// <summary>
    /// Cantidad de elementos por página.
    /// </summary>
    int Size = 10,
    
    /// <summary>
    /// Campo por el cual ordenar.
    /// </summary>
    string SortBy = "id",
    
    /// <summary>
    /// Dirección de ordenación ("asc" o "desc").
    /// </summary>
    string Direction = "asc"
);