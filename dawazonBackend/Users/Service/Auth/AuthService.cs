using CSharpFunctionalExtensions;
using dawazonBackend.Cart.Models;
using dawazonBackend.Cart.Repository;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using dawazonBackend.Users.Service.Jwt;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Service.Auth;

/// <summary>
/// Implementación del servicio de autenticación de usuarios.
/// </summary>
/// <remarks>
/// Proporciona la lógica de negocio para el registro e inicio de sesión de usuarios.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item><see cref="ILogger{AuthService}"/>: Para logging de operaciones</item>
///     <item><see cref="IJwtService"/>: Para generación de tokens JWT</item>
///     <item><see cref="UserManager{User}"/>: Para operaciones de usuario en Identity</item>
///     <item><see cref="ICartRepository"/>: Para crear carrito al registrar usuario</item>
/// </list>
///
/// <para><b>Patrones utilizados:</b></para>
/// <list type="bullet">
///     <item>Result Pattern: Para manejo de errores con CSharpFunctionalExtensions</item>
///     <item>Unit of Work: Integración con Identity</item>
/// </list>
/// </remarks>
public class AuthService(ILogger<AuthService> logger, IJwtService jwtService, UserManager<User> db, ICartRepository cartRepository) : IAuthService
{
    /// <inheritdoc/>
    /// <summary>
    /// Registra un nuevo usuario en el sistema con los datos proporcionados.
    /// </summary>
    /// <param name="dto">Objeto con los datos de registro del usuario.</param>
    /// <returns>
    /// Un <see cref="Result{AuthResponseDto, UserError}"/> con el token JWT si el registro es exitoso.
    /// </returns>
    /// <remarks>
    /// <para><b>Proceso completo de registro:</b></para>
    /// <list type="number">
    ///     <item>Sanitiza el username eliminando saltos de línea</item>
    ///     <item>Verifica que el username y email no estén en uso</item>
    ///     <item>Crea el usuario en Identity con contraseña hasheada</item>
    ///     <item>Busca el usuario creado por email</item>
    ///     <item>Asigna el rol "User" por defecto</item>
    ///     <item>Crea un carrito de compras vacío</item>
    ///     <item>Genera token JWT de autenticación</item>
    /// </list>
    /// 
    /// <para><b>Errores posibles:</b></para>
    /// <list type="bullet">
    ///     <item>Usuario ya existe (username)</item>
    ///     <item>Email ya en uso</item>
    ///     <item>Error al crear usuario en Identity</item>
    ///     <item>Usuario no encontrado tras creación</item>
    /// </list>
    /// </remarks>
    public async Task<Result<AuthResponseDto, UserError>> SignUpAsync(RegisterDto dto)
    {
        var sanitizedUsername = dto.Username.Replace("\n", "").Replace("\r", "");
        logger.LogInformation("Petición de signUp para el username: {Username}", sanitizedUsername);

        var duplicateCheck = await CheckDuplicatesAsync(dto);
        if (duplicateCheck.IsFailure)
        {
            logger.LogWarning("Registro fallido - duplicado: {Error}", duplicateCheck.Error.Message);
            return Result.Failure<AuthResponseDto, UserError>(duplicateCheck.Error);
        }

        var user = new User
        {
            Name = dto.Username,
            UserName = dto.Username,
            Email = dto.Email,
            IsDeleted = false
        };

        var savedUser = await db.CreateAsync(user, dto.Password);
        if (!savedUser.Succeeded)
        {
            var errors = string.Join(", ", savedUser.Errors.Select(x => x.Description));
            logger.LogError("Error al crear usuario en Identity: {Errors}", errors);
            return Result.Failure<AuthResponseDto, UserError>(new UserError(errors));
        }

        var userFound = await db.FindByEmailAsync(dto.Email);
        if (userFound == null)
        {
            logger.LogError("Usuario no encontrado tras creación con email: {Email}", dto.Email);
            return Result.Failure<AuthResponseDto, UserError>(
                new UserNotFoundError($"No se encuentra el usuario con email {dto.Email}"));
        }

        await db.AddToRoleAsync(userFound, "User");
        logger.LogInformation("Rol 'User' asignado a: {Username}", sanitizedUsername);

        var cart = new Cart.Models.Cart
        {
            Id = "CART" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            UserId = userFound.Id,
            Purchased = false
        };
        await cartRepository.CreateCartAsync(cart);
        logger.LogInformation("Carrito creado para usuario: {Username}", sanitizedUsername);

        var authResponse = await GenerateAuthResponseAsync(userFound);

        logger.LogInformation("Usuario registrado correctamente: {Username}", sanitizedUsername);

        return Result.Success<AuthResponseDto, UserError>(authResponse);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Inicia sesión con credenciales de usuario.
    /// </summary>
    /// <param name="dto">Credenciales de acceso (username o email + password).</param>
    /// <returns>
    /// Un <see cref="Result{AuthResponseDto, UserError}"/> con el token JWT si la autenticación es exitosa.
    /// </returns>
    /// <remarks>
    /// <para><b>Proceso de login:</b></para>
    /// <list type="number">
    ///     <item>Sanitiza el input eliminando saltos de línea</item>
    ///     <item>Determina si el input es email o username</item>
    ///     <item>Busca el usuario correspondiente</item>
    ///     <item>Valida la contraseña</item>
    ///     <item>Genera token JWT</item>
    /// </list>
    /// 
    /// <para><b>Errores posibles:</b></para>
    /// <list type="bullet">
    ///     <item>Usuario no encontrado</item>
    ///     <item>Contraseña incorrecta</item>
    /// </list>
    /// </remarks>
    public async Task<Result<AuthResponseDto, UserError>> SignInAsync(LoginDto dto)
    {
        var sanitizedUsername = dto.UsernameOrEmail.Replace("\n", "").Replace("\r", "");
        logger.LogInformation("SignIn request para username: {Username}", sanitizedUsername);
        
        if (dto.UsernameOrEmail.Contains("@"))
        {
            logger.LogDebug("SignIn usando email: {Email}", dto.UsernameOrEmail);
            var user = await db.FindByEmailAsync(dto.UsernameOrEmail);
            return await ValidateSignInAsync(dto, user, sanitizedUsername);
        }
        else
        {
            logger.LogDebug("SignIn usando username: {Username}", dto.UsernameOrEmail);
            var user = await db.FindByNameAsync(dto.UsernameOrEmail);
            return await ValidateSignInAsync(dto, user, sanitizedUsername);
        }
    }

    /// <summary>
    /// Valida las credenciales de inicio de sesión del usuario.
    /// </summary>
    /// <param name="dto">Credenciales de acceso.</param>
    /// <param name="user">Usuario encontrado (puede ser null).</param>
    /// <param name="sanitizedUsername">Username sanitizado para logging.</param>
    /// <returns>
    /// Un <see cref="Result{AuthResponseDto, UserError}"/> con el resultado de la validación.
    /// </returns>
    /// <remarks>
    /// Este método privado:
    /// <list type="bullet">
    ///     <item>Verifica que el usuario exista</item>
    ///     <item>Valida la contraseña usando Identity</item>
    ///     <item>Genera el token JWT si las credenciales son válidas</item>
    /// </list>
    /// </remarks>
    private async Task<Result<AuthResponseDto, UserError>> ValidateSignInAsync(LoginDto dto, User? user, string sanitizedUsername)
    {
        if (user is null)
        {
            logger.LogWarning("SignIn fallido: Usuario no encontrado - {Username}", sanitizedUsername);
            return Result.Failure<AuthResponseDto, UserError>(
                new UnauthorizedError("credenciales invalidas")
            );
        }

        var passwordValid = await db.CheckPasswordAsync(user, dto.Password);
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

    /// <summary>
    /// Verifica si el username o email ya están en uso.
    /// </summary>
    /// <param name="dto">Datos de registro a verificar.</param>
    /// <returns>
    /// <see cref="UnitResult{UserError}"/> que es exitoso si no hay duplicados,
    /// o falla con <see cref="UserConflictError"/> si el username o email existen.
    /// </returns>
    /// <remarks>
    /// Realiza dos verificaciones:
    /// <list type="number">
    ///     <item>Busca usuario por username</item>
    ///     <item>Busca usuario por email</item>
    /// </list>
    /// </remarks>
    private async Task<UnitResult<UserError>> CheckDuplicatesAsync(RegisterDto dto)
    {
        var existingUser = await db.FindByNameAsync(dto.Username);
        if (existingUser is not null)
        {
            logger.LogWarning("Username ya en uso: {Username}", dto.Username);
            return UnitResult.Failure<UserError>(new UserConflictError("username ya en uso:"+existingUser.Name));
        }

        var existingEmail = await db.FindByEmailAsync(dto.Email);
        if (existingEmail is not null)
        {
            logger.LogWarning("Email ya en uso: {Email}", dto.Email);
            return UnitResult.Failure<UserError>(new UserConflictError("email ya en uso"+existingEmail.Email));
        }
        
        return UnitResult.Success<UserError>();
    }
    
    /// <summary>
    /// Genera la respuesta de autenticación con el token JWT.
    /// </summary>
    /// <param name="user">Usuario para el cual generar el token.</param>
    /// <returns>
    /// Un <see cref="AuthResponseDto"/> contendo el token JWT.
    /// </returns>
    /// <remarks>
    /// Este método private usa <see cref="IJwtService.GenerateTokenAsync"/> para crear el token.
    /// </remarks>
    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
    {
        var token = await jwtService.GenerateTokenAsync(user);
        
        return new AuthResponseDto(token);
    }
}
