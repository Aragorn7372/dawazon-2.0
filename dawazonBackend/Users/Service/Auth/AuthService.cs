using CSharpFunctionalExtensions;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service.Jwt;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Service.Auth;

public class AuthService(ILogger<AuthService> logger, IJwtService jwtService, UserManager<User> db) : IAuthService
{
    public async Task<Result<AuthResponseDto, UserError>> SignUpAsync(RegisterDto dto)
    {
        var sanitizedUsername = dto.Username.Replace("\n", "").Replace("\r", "");
        logger.LogInformation("SignUp request for username: {Username}", sanitizedUsername);


        var duplicateCheck = await CheckDuplicatesAsync(dto);
        if (duplicateCheck.IsFailure)
        {
            return Result.Failure<AuthResponseDto, UserError>(duplicateCheck.Error);
        }

        var user = new User
        {
            Name = dto.Username,
            UserName = dto.Username,
            Email = dto.Email,
            PasswordHash = dto.Password,
            IsDeleted = false
        };

        var savedUser = await db.CreateAsync(user);
        if (!savedUser.Succeeded)
        {
            var errors = savedUser.Errors.Select(x => x.Description).ToString();
            return Result.Failure<AuthResponseDto, UserError>(new UserError("/n" + errors!));
        }

        var userFound = await db.FindByEmailAsync(dto.Email);
        if (userFound == null)
        {
            return Result.Failure<AuthResponseDto, UserError>(
                new UserNotFoundError($"No se encuentra el usuario con email {dto.Email}"));
        }

        await db.AddToRoleAsync(userFound, "User");
        var authResponse = await GenerateAuthResponseAsync(userFound);

        logger.LogInformation("User registered successfully: {Username}", sanitizedUsername);

        return Result.Success<AuthResponseDto, UserError>(authResponse);
    }

    public async Task<Result<AuthResponseDto, UserError>> SignInAsync(LoginDto dto)
    {
        var sanitizedUsername = dto.UsernameOrEmail.Replace("\n", "").Replace("\r", "");
        logger.LogInformation("SignIn request for username: {Username}", sanitizedUsername);
        if (dto.UsernameOrEmail.Contains("@"))
        {
            var user = await db.FindByEmailAsync(dto.UsernameOrEmail);
            return await ValidateSignInAsync(dto, user, sanitizedUsername);
        }
        else
        {
            var user = await db.FindByNameAsync(dto.UsernameOrEmail);
            return await ValidateSignInAsync(dto, user, sanitizedUsername);
        }
    }

    private async Task<Result<AuthResponseDto, UserError>> ValidateSignInAsync(LoginDto dto, User? user, string sanitizedUsername)
    {
        if (user is null)
        {
            logger.LogWarning("SignIn fallido: Usuario no encontrado - {Username}", sanitizedUsername);
            return Result.Failure<AuthResponseDto, UserError>(
                new UnauthorizedError("credenciales invalidas")
            );
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!passwordValid)
        {
            logger.LogWarning("SignIn fallido: Password inválido - {Username}", sanitizedUsername);
            return Result.Failure<AuthResponseDto, UserError>(
                new UnauthorizedError("credenciales invalidas")
            );
        }

        var authResponse = await GenerateAuthResponseAsync(user);
        logger.LogInformation("Usuario inició sesión correctamente: {Username}", sanitizedUsername);

        return Result.Success<AuthResponseDto, UserError>(authResponse);
    }

    private async Task<UnitResult<UserError>> CheckDuplicatesAsync(RegisterDto dto)
    {
        var usernameCheckTask = db.FindByNameAsync(dto.Username);
        var emailCheckTask = db.FindByEmailAsync(dto.Email);

        await Task.WhenAll(usernameCheckTask, emailCheckTask);

        var existingUser = await usernameCheckTask;
        if (existingUser is not null)
        {
            return UnitResult.Failure<UserError>(new UserConflictError("username ya en uso:"+existingUser.Name));
        }

        var existingEmail = await emailCheckTask;
        return existingEmail is not null ? 
            UnitResult.Failure<UserError>(new UserConflictError("email ya en uso"+existingEmail.Email)) 
            : UnitResult.Success<UserError>();
    }
    
    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
    {
        var token = await jwtService.GenerateTokenAsync(user);
        
        return new AuthResponseDto(token);
    }
}