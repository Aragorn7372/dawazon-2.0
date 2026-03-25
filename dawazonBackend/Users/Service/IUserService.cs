using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;

namespace dawazonBackend.Users.Service;

/// <summary>
/// Interfaz que define el contrato para el servicio de gestión de usuarios.
/// </summary>
/// <remarks>
/// Esta interfaz proporciona métodos para administrar usuarios en el sistema,
/// incluyendo operaciones de lectura, actualización, banneo y estadísticas.
/// 
/// <para><b>Características principales:</b></para>
/// <list type="bullet">
///     <item>Listado paginado de usuarios con filtros</item>
///     <item>Obtención de usuario por ID</item>
///     <item>Actualización de perfil de usuario</item>
///     <item>Banneo de usuarios (borrado lógico)</item>
///     <item>Estadísticas de usuarios</item>
/// </list>
/// 
/// <para><b>Uso típico:</b></para>
/// <code>
/// // Inyectar el servicio
/// IUserService userService = httpContext.RequestServices.GetRequiredService&lt;IUserService&gt;();
///
/// // Obtener usuarios paginados
/// var filters = new FilterDto { Page = 0, Size = 10, SortBy = "nombre" };
/// var users = await userService.GetAllAsync(filters);
///
/// // Obtener usuario por ID
/// var userResult = await userService.GetByIdAsync("123");
/// if (userResult.IsSuccess)
/// {
///     var user = userResult.Value;
/// }
/// </code>
/// </remarks>
public interface IUserService
{
    /// <summary>
    /// Obtiene una lista paginada de usuarios filtrados según los criterios especificados.
    /// </summary>
    /// <param name="filters">Objeto <see cref="FilterDto"/> que contiene los filtros de búsqueda, paginación y ordenamiento.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// El resultado es un <see cref="PageResponseDto{UserDto}"/> que contiene:
    /// <list type="bullet">
    ///     <item>Lista de usuarios en la página actual</item>
    ///     <item>Total de páginas disponibles</item>
    ///     <item>Total de elementos</item>
    ///     <item>Información de paginación</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Este método excluye automáticamente los usuarios eliminados (IsDeleted = true).
    /// Los filtros soportados incluyen:
    /// <list type="bullet">
    ///     <item><c>Page</c>: Número de página (0-indexed)</item>
    ///     <item><c>Size</c>: Tamaño de página</item>
    ///     <item><c>SortBy</c>: Campo de ordenamiento (actualmente solo "nombre")</item>
    ///     <item><c>Direction</c>: Dirección del ordenamiento ("asc" o "desc")</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var filters = new FilterDto
    /// {
    ///     Page = 0,
    ///     Size = 20,
    ///     SortBy = "nombre",
    ///     Direction = "asc"
    /// };
    /// var result = await userService.GetAllAsync(filters);
    /// Console.WriteLine($"Total usuarios: {result.TotalElements}");
    /// foreach (var user in result.Content)
    /// {
    ///     Console.WriteLine($"- {user.Nombre} ({user.Email})");
    /// }
    /// </code>
    /// </example>
    Task<PageResponseDto<UserDto>> GetAllAsync(FilterDto filters);

    /// <summary>
    /// Obtiene un usuario específico por su identificador único.
    /// </summary>
    /// <param name="id">El identificador único del usuario (tipo long como string).</param>
    /// <returns>
    /// Un <see cref="Result{UserDto, UserError}"/> que puede ser:
    /// <list type="bullet">
    ///     <item>Exitoso: contiene el <see cref="UserDto"/> del usuario encontrado</item>
    ///     <item>Fallido: contiene un <see cref="UserError"/> indicando el error</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// El método utiliza el patrón Result de CSharpFunctionalExtensions para manejar errores.
    /// Si el usuario no se encuentra, retorna un <see cref="UserNotFoundError"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await userService.GetByIdAsync("12345");
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine($"Usuario encontrado: {result.Value.Nombre}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Error: {result.Error.Message}");
    /// }
    /// </code>
    /// </example>
    Task<Result<UserDto, UserError>> GetByIdAsync(string id);

    /// <summary>
    /// Actualiza los datos de un usuario existente, incluyendo su información de perfil,
    /// dirección y opcionalmente su imagen de avatar.
    /// </summary>
    /// <param name="id">El identificador único del usuario a actualizar.</param>
    /// <param name="userRequestDto">Objeto <see cref="UserRequestDto"/> con los nuevos datos del usuario.</param>
    /// <param name="image">Archivo de imagen opcional para actualizar el avatar del usuario.</param>
    /// <returns>
    /// Un <see cref="Result{UserDto, UserError}"/> que puede ser:
    /// <list type="bullet">
    ///     <item>Exitoso: contiene el <see cref="UserDto"/> con los datos actualizados</item>
    ///     <item>Fallido: contiene un <see cref="UserError"/> indicando el error</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>Esta operación actualiza los siguientes campos:</para>
    /// <list type="bullet">
    ///     <item>Nombre del usuario</item>
    ///     <item>Correo electrónico</item>
    ///     <item>Teléfono (si se proporciona)</item>
    ///     <item>Dirección completa (calle, ciudad, provincia, código postal)</item>
    ///     <item>Avatar (si se proporciona una imagen)</item>
    /// </list>
    /// 
    /// <para><b>Errores posibles:</b></para>
    /// <list type="bullet">
    ///     <item><see cref="UserNotFoundError"/>: Si el usuario no existe</item>
    ///     <item><see cref="UserUpdateError"/>: Si falla la actualización o la subida de imagen</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var updateDto = new UserRequestDto
    /// {
    ///     Nombre = "Nuevo Nombre",
    ///     Email = "nuevo@email.com",
    ///     Telefono = "612345678",
    ///     Calle = "Calle Principal",
    ///     Ciudad = "Madrid",
    ///     Provincia = "Madrid",
    ///     CodigoPostal = "28001"
    /// };
    /// 
    /// var result = await userService.UpdateByIdAsync(123, updateDto, avatarFile);
    /// if (result.IsSuccess)
    /// {
    ///     Console.WriteLine("Usuario actualizado correctamente");
    /// }
    /// </code>
    /// </example>
    Task<Result<UserDto, UserError>> UpdateByIdAsync(long id, UserRequestDto userRequestDto, IFormFile? image);

    /// <summary>
    /// Desactiva (bannea) a un usuario estableciendo la bandera de borrado lógico.
    /// </summary>
    /// <param name="banUserId">El identificador único del usuario a banear.</param>
    /// <returns>Una tarea asíncrona que representa la operación.</returns>
    /// <remarks>
    /// <para>Esta operación realiza un borrado lógico, estableciendo la propiedad <c>IsDeleted</c> 
    /// del usuario a <c>true</c>. El usuario no será eliminado de la base de datos 
    /// pero no aparecerá en las consultas regulares.</para>
    /// 
    /// <para><b>Nota:</b> Esta operación no elimina los datos asociados del usuario 
    /// (como pedidos, comentarios, etc.).</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Banear usuario por ID
    /// await userService.BanUserById("12345");
    /// </code>
    /// </example>
    Task BanUserById(string banUserId);

    /// <summary>
    /// Obtiene el número total de usuarios activos registrados en el sistema.
    /// </summary>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// Retorna el conteo total de usuarios donde <c>IsDeleted = false</c>.
    /// </returns>
    /// <remarks>
    /// Este método cuenta únicamente usuarios activos, excluyendo aquellos
    /// que han sido baneados (borrado lógico).
    /// </remarks>
    /// <example>
    /// <code>
    /// var totalUsers = await userService.GetTotalUsersCountAsync();
    /// Console.WriteLine($"Total de usuarios activos: {totalUsers}");
    /// </code>
    /// </example>
    Task<int> GetTotalUsersCountAsync();
}
