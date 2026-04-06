using dawazonBackend.Users.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la vista de detalle de un usuario en el panel de Admin.
/// </summary>
/// <remarks>
/// Proporciona los datos completos de un usuario específico.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>UserDto: Modelo de usuario del backend</item>
/// </list>
/// </remarks>
public class AdminUserDetailViewModel
{
    /// <summary>Datos completos del usuario.</summary>
    public UserDto User { get; set; } = new();
}
