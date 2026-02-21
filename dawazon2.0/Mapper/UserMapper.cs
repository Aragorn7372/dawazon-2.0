using dawazon2._0.Models;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Models;

namespace dawazon2._0.Mapper;

public static class UserMapper
{
     public static LoginDto ToDto(this LoginModelView user)
     {
         return new LoginDto
         {
             UsernameOrEmail = user.UsernameOrEmail,
             Password = user.Password
         };
     }
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