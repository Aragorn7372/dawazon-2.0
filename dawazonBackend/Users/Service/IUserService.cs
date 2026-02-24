using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;

namespace dawazonBackend.Users.Service;

/// <summary>
/// Interfaz para el servicio de gestión de usuarios.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Obtiene una lista paginada de usuarios filtrados.
    /// </summary>
    /// <param name="filters">Filtros de búsqueda y paginación.</param>
    /// <returns>Una respuesta paginada con los usuarios encontrados.</returns>
    Task<PageResponseDto<UserDto>> GetAllAsync(FilterDto filters);

    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    /// <param name="id">El ID del usuario.</param>
    /// <returns>Un resultado exitoso con el DTO del usuario o un error de usuario.</returns>
    Task<Result<UserDto, UserError>> GetByIdAsync(string id);

    /// <summary>
    /// Actualiza los datos de un usuario por su ID.
    /// </summary>
    /// <param name="id">El ID del usuario a actualizar.</param>
    /// <param name="userRequestDto">Los nuevos datos del usuario.</param>
    /// <returns>El DTO del usuario actualizado o un error si falló.</returns>
    Task<Result<UserDto, UserError>> UpdateByIdAsync(long id, UserRequestDto userRequestDto, IFormFile? image);

    /// <summary>
    /// Desactiva (banear) a un usuario por su ID (borrado lógico).
    /// </summary>
    /// <param name="banUserId">El ID del usuario a desactivar.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task BanUserById(string banUserId);
}