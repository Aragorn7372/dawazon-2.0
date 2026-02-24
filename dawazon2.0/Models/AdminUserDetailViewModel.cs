using dawazonBackend.Users.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la vista de detalle de un usuario en el panel de Admin.
/// </summary>
public class AdminUserDetailViewModel
{
    public UserDto User { get; set; } = new();
}
