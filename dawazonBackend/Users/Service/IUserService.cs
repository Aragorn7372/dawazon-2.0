using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;

namespace dawazonBackend.Users.Service;

public interface IUserService
{
    Task<PageResponseDto<UserDto>> GetAllAsync(FilterDto filters);
    Task<Result<UserDto, UserError>> GetByIdAsync(string id);
    Task<Result<UserDto, UserError>> UpdateByIdAsync(long id, UserRequestDto userRequestDto);
    Task BanUserById(string banUserId);
}