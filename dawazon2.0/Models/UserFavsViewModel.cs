using dawazonBackend.Products.Models.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la lista de productos favoritos del usuario.
/// </summary>
/// <remarks>
/// Proporciona datos para renderizar la página de favoritos con paginación.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>ProductResponseDto: Modelo de producto del backend</item>
/// </list>
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Products: Lista de productos favoritos</item>
///     <item>Pagination: PageNumber, TotalPages, TotalElements</item>
/// </list>
/// 
/// <para><b>Propiedades calculadas:</b></para>
/// <list type="bullet">
///     <item>First/Last: Indican primera/última página</item>
///     <item>PrevPage/NextPage: Números de página anterior/siguiente</item>
/// </list>
/// </remarks>
public class UserFavsViewModel
{
    /// <summary>Lista de productos marcados como favoritos.</summary>
    public List<ProductResponseDto> Products { get; set; } = [];

    /// <summary>Número de página actual (0-indexed).</summary>
    public int PageNumber    { get; set; }
    
    /// <summary>Número total de páginas.</summary>
    public int TotalPages    { get; set; }
    
    /// <summary>Total de favoritos en todas las páginas.</summary>
    public long TotalElements { get; set; }

    /// <summary>Indica si es la primera página.</summary>
    public bool First => PageNumber == 0;
    
    /// <summary>Indica si es la última página.</summary>
    public bool Last  => PageNumber >= TotalPages - 1;
    
    /// <summary>Número de la página anterior.</summary>
    public int PrevPage => Math.Max(0, PageNumber - 1);
    
    /// <summary>Número de la siguiente página.</summary>
    public int NextPage => Math.Min(TotalPages - 1, PageNumber + 1);
}
