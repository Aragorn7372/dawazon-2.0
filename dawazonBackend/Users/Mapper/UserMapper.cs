using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Mapper;

/// <summary>
/// Clase de utilidad para mapear entre la entidad User y UserDto.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Convierte una instancia de User a UserDto de forma asíncrona, incluyendo sus roles.
    /// </summary>
    /// <param name="user">La entidad de usuario.</param>
    /// <param name="userManager">El gestor de usuarios de Identity.</param>
    /// <returns>Un DTO con la información del usuario.</returns>
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