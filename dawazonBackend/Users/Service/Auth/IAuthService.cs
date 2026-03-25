using CSharpFunctionalExtensions;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;

namespace dawazonBackend.Users.Service.Auth;

/// <summary>
/// Interfaz que define el contrato para el servicio de autenticación de usuarios.
/// </summary>
/// <remarks>
/// Proporciona métodos para el registro e inicio de sesión de usuarios en el sistema.
///
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Registro de nuevos usuarios</item>
///     <item>Inicio de sesión con credenciales</item>
///     <item>Generación de tokens JWT</item>
/// </list>
///
/// <para><b>Uso típico:</b></para>
/// <code>
/// IAuthService authService = httpContext.RequestServices.GetRequiredService&lt;IAuthService&gt;();
///
/// // Registro
/// var registerDto = new RegisterDto { Username = "john", Email = "john@example.com", Password = "password123" };
/// var signUpResult = await authService.SignUpAsync(registerDto);
///
/// // Inicio de sesión
/// var loginDto = new LoginDto { UsernameOrEmail = "john", Password = "password123" };
/// var signInResult = await authService.SignInAsync(loginDto);
/// </code>
/// </remarks>
public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="dto">Objeto <see cref="RegisterDto"/> con los datos de registro del usuario.</param>
    /// <returns>
    /// Un <see cref="Result{AuthResponseDto, UserError}"/> que puede ser:
    /// <list type="bullet">
    ///     <item>Exitoso: contiene <see cref="AuthResponseDto"/> con el token JWT</item>
    ///     <item>Fallido: contiene un <see cref="UserError"/> indicando el error</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para><b>Proceso de registro:</b></para>
    /// <list type="number">
    ///     <item>Valida que el username y email no estén en uso</item>
    ///     <item>Crea el usuario en Identity con hash de contraseña</item>
    ///     <item>Asigna el rol "User" por defecto</item>
    ///     <item>Crea un carrito de compras vacío</item>
    ///     <item>Genera un token JWT de autenticación</item>
    /// </list>
    /// 
    /// <para><b>Errores posibles:</b></para>
    /// <list type="bullet">
    ///     <item><see cref="UserConflictError"/>: Si el username o email ya existen</item>
    ///     <item><see cref="UserError"/>: Si falla la creación del usuario en Identity</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var registerDto = new RegisterDto
    /// {
    ///     Username = "juan Perez",
    ///     Email = "juan@example.com",
    ///     Password = "SecurePassword123"
    /// };
    ///
    /// var result = await authService.SignUpAsync(registerDto);
    /// if (result.IsSuccess)
    /// {
    ///     var token = result.Value.Token;
    ///     Console.WriteLine($"Usuario registrado. Token: {token}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Error: {result.Error.Message}");
    /// }
    /// </code>
    /// </example>
    Task<Result<AuthResponseDto, UserError>> SignUpAsync(RegisterDto dto);

    /// <summary>
    /// Inicia sesión con credenciales de usuario (username o email + password).
    /// </summary>
    /// <param name="dto">Objeto <see cref="LoginDto"/> con las credenciales de acceso.</param>
    /// <returns>
    /// Un <see cref="Result{AuthResponseDto, UserError}"/> que puede ser:
    /// <list type="bullet">
    ///     <item>Exitoso: contiene <see cref="AuthResponseDto"/> con el token JWT</item>
    ///     <item>Fallido: contiene un <see cref="UserError"/> indicando el error</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para><b>Proceso de login:</b></para>
    /// <list type="number">
    ///     <item>Busca el usuario por username o email</item>
    ///     <item>Valida la contraseña usando Identity</item>
    ///     <item>Genera un nuevo token JWT</item>
    /// </list>
    /// 
    /// <para><b>Errores posibles:</b></para>
    /// <list type="bullet">
    ///     <item><see cref="UnauthorizedError"/>: Si el usuario no existe o la contraseña es incorrecta</item>
    /// </list>
    /// 
    /// <para><b>Nota:</b> El método sanitiza el input eliminando caracteres de salto de línea.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var loginDto = new LoginDto
    /// {
    ///     UsernameOrEmail = "juan@example.com", // o puede ser el username
    ///     Password = "SecurePassword123"
    /// };
    ///
    /// var result = await authService.SignInAsync(loginDto);
    /// if (result.IsSuccess)
    /// {
    ///     var token = result.Value.Token;
    ///     Console.WriteLine($"Login exitoso. Token: {token}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Error: {result.Error.Message}");
    /// }
    /// </code>
    /// </example>
    Task<Result<AuthResponseDto, UserError>> SignInAsync(LoginDto dto);
}
