using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Mapper;

public static class UserMapper
{
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
            Roles = roles.ToHashSet()
        };
    }
}