using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Mapper;

/// <summary>
/// Clase de utilidad estática para mapear entre la entidad User y DTOs.
/// </summary>
/// <remarks>
/// Proporciona métodos de extensión para convertir entre:
/// <list type="bullet">
///     <item>User (entidad de Identity) ↔ UserDto</item>
/// </list>
/// 
/// <para><b>Patrón:</b></para>
/// Métodos de extensión asíncronos que usan UserManager para obtener roles.
/// </remarks>
public static class UserMapper
{
    /// <summary>
    /// Convierte una entidad User a UserDto de forma asíncrona.
    /// </summary>
    /// <param name="user">Entidad de usuario de Identity.</param>
    /// <param name="userManager">UserManager para obtener roles.</param>
    /// <returns>UserDto con la información del usuario y sus roles.</returns>
    /// <remarks>
    /// Mapea:
    /// <list type="bullet">
    ///     <item>Id, Email, Nombre, Avatar</item>
    ///     <item>Dirección: Calle, Ciudad, CodigoPostal, Provincia</item>
    ///     <item>Teléfono desde PhoneNumber</item>
    ///     <item>Roles desde UserManager</item>
    /// </list>
    /// </remarks>
    public static async Task<UserDto> ToDtoAsync(this User user,UserManager<User> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Calle = user.Client.Address.Street,
            Ciudad = user.Client.Address.City,
            CodigoPostal = user.Client.Address.PostalCode.ToString(),
            Email = user.Email ?? "",
            Telefono = user.PhoneNumber?? "",
            Nombre =  user.Name,
            Provincia = user.Client.Address.Province,
            Avatar =  user.Avatar,
            Roles = roles.ToHashSet()
        };
    }
}