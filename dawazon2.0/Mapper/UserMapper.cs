using dawazon2._0.Models;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Models;

namespace dawazon2._0.Mapper;

/// <summary>
/// Métodos de extensión para convertir entre ViewModels y DTOs de autenticación.
/// </summary>
/// <remarks>
/// <para><b>Conversiones:</b></para>
/// <list type="bullet">
///     <item>LoginModelView -> LoginDto</item>
///     <item>RegisterModelView -> RegisterDto</item>
/// </list>
/// </remarks>
public static class UserMapper
{
    /// <summary>
    /// Convierte el ViewModel de login a DTO del backend.
    /// </summary>
    /// <param name="user">ViewModel con credenciales.</param>
    /// <returns>LoginDto para el servicio.</returns>
    public static LoginDto ToDto(this LoginModelView user)
     {
         return new LoginDto
         {
             UsernameOrEmail = user.UsernameOrEmail,
             Password = user.Password
         };
     }
     /// <summary>
    /// Convierte el ViewModel de registro a DTO del backend.
    /// </summary>
    /// <param name="user">ViewModel con datos de registro.</param>
    /// <returns>RegisterDto para el servicio.</returns>
    public static RegisterDto ToDto(this RegisterModelView user)
     {
         return new RegisterDto
         {
             Username = user.Username,
             Password = user.Password,
             Email = user.Email
         };
     }
}