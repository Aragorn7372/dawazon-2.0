using dawazonBackend.Users.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la lista paginada de usuarios en el panel de Admin.
/// </summary>
public class AdminUserListViewModel
{
    public List<UserDto> Users         { get; set; } = new();
    public int           PageNumber    { get; set; }
    public int           TotalPages    { get; set; }
    public long          TotalElements { get; set; }
    public int           PageSize      { get; set; }
}
