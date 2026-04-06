using dawazonBackend.Users.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la lista paginada de usuarios en el panel de Admin.
/// </summary>
/// <remarks>
/// Proporciona datos para renderizar el listado de usuarios con paginación.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>UserDto: Modelo de usuario del backend</item>
/// </list>
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Users: Lista de usuarios</item>
///     <item>Pagination: PageNumber, TotalPages, TotalElements, PageSize</item>
/// </list>
/// </remarks>
public class AdminUserListViewModel
{
    /// <summary>Lista de usuarios en la página actual.</summary>
    public List<UserDto> Users         { get; set; } = new();
    
    /// <summary>Número de página actual (0-indexed).</summary>
    public int           PageNumber    { get; set; }
    
    /// <summary>Número total de páginas.</summary>
    public int           TotalPages    { get; set; }
    
    /// <summary>Total de usuarios en todas las páginas.</summary>
    public long          TotalElements { get; set; }
    
    /// <summary>Tamaño de cada página.</summary>
    public int           PageSize      { get; set; }
}
