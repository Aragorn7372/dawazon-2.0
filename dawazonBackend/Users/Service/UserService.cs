using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Storage;
using dawazonBackend.Users.Dto;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Mapper;
using dawazonBackend.Users.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace dawazonBackend.Users.Service;

/// <summary>
/// Implementación del servicio de gestión de usuarios.
/// </summary>
/// <remarks>
/// Esta clase proporciona la lógica de negocio para administrar usuarios en el sistema,
/// incluyendo operaciones de lectura, actualización, banneo y estadísticas.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item><see cref="ILogger{UserService}"/>: Para logging de operaciones</item>
///     <item><see cref="UserManager{User}"/>: Para operaciones de Identity</item>
///     <item><see cref="IStorage"/>: Para almacenamiento de imágenes</item>
/// </list>
///
/// <para><b>Patrones utilizados:</b></para>
/// <list type="bullet">
///     <item>Result Pattern: Para manejo de errores con CSharpFunctionalExtensions</item>
///     <item>Repository Pattern: A través de UserManager de Identity</item>
///     <item>Unit of Work: Integración con EF Core</item>
/// </list>
/// </remarks>
public class UserService(ILogger<UserService> logger,UserManager<User> userManager, IStorage storage): IUserService
{
    /// <inheritdoc/>
    /// <summary>
    /// Obtiene una lista paginada de usuarios activos con sus datos relacionados.
    /// </summary>
    /// <param name="filters">Filtros de búsqueda, paginación y ordenamiento.</param>
    /// <returns>
    /// Un <see cref="PageResponseDto{UserDto}"/> con los usuarios de la página solicitada.
    /// </returns>
    /// <remarks>
    /// Este método:
    /// <list type="bullet">
    ///     <item>Carga eager los datos del cliente y dirección asociados</item>
    ///     <item>Excluye usuarios eliminados (IsDeleted = true)</item>
    ///     <item>Aplica ordenamiento por nombre o ID</item>
    ///     <item>Convierte cada usuario a DTO usando UserMapper</item>
    /// </list>
    /// 
    /// <para><b>Complejidad:</b> O(n) donde n es el número de usuarios en la página.</para>
    /// </remarks>
    public async Task<PageResponseDto<UserDto>> GetAllAsync(FilterDto filters)
    {
        logger.LogInformation("Obteniendo todos los usuarios con filtros: Page={Page}, Size={Size}", 
            filters.Page, filters.Size);

        var query =  userManager.Users
            .Include(u => u.Client)
            .ThenInclude(c => c.Address)
            .AsQueryable();

        query=query.Where(u=>u.IsDeleted==false);
        var totalCount = await query.CountAsync();
        query= ApplySorting(query, filters.SortBy, filters.Direction);
        var items= await query.Skip(filters.Page * filters.Size)
            .Take(filters.Size)
            .ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in items)
        {
            userDtos.Add(await user.ToDtoAsync(userManager));
        }
        int totalPages = filters.Size > 0 ? (int)Math.Ceiling(totalCount/(double)filters.Size) : 0;
        
        logger.LogInformation("Se encontraron {Count} usuarios de {Total}", items.Count, totalCount);
        
        return new PageResponseDto<UserDto>(
            Content: userDtos,
            TotalPages: totalPages,
            TotalElements: totalCount,
            PageSize: filters.Size,
            PageNumber: filters.Page,
            TotalPageElements: items.Count,
            SortBy: filters.SortBy,
            Direction: filters.Direction

        );

    }

    /// <summary>
    /// Aplica el ordenamiento a la consulta de usuarios basándose en el campo y dirección especificados.
    /// </summary>
    /// <param name="query">La consulta de IQueryable de usuarios.</param>
    /// <param name="sortBy">El campo por el cual ordenar ("nombre" para nombre, cualquier otro para ID).</param>
    /// <param name="direction">La dirección del orden ("asc" o "desc").</param>
    /// <returns>La consulta con el ordenamiento aplicado.</returns>
    /// <remarks>
    /// <para>Campos de ordenamiento soportados:</para>
    /// <list type="bullet">
    ///     <item>"nombre" - Ordena por la propiedad Name del usuario</item>
    ///     <item>cualquier otro valor - Ordena por Id (comportamiento por defecto)</item>
    /// </list>
    /// </remarks>
    private IQueryable<User> ApplySorting(IQueryable<User> query, string sortBy, string direction)
    {
        var isDescending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        Expression<Func<User,object>> keySelector = sortBy.ToLower() switch
        {
            "nombre" => p => p.Name,
            _ => p => p.Id
        };
        return isDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Busca y retorna un usuario por su identificador único.
    /// </summary>
    /// <param name="id">El ID del usuario a buscar.</param>
    /// <returns>
    /// Un <see cref="Result{UserDto, UserError}"/> que contiene:
    /// <list type="bullet">
    ///     <item>Valor: El DTO del usuario si se encuentra</item>
    ///     <item>Error: <see cref="UserNotFoundError"/> si no existe</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// El método utiliza logging para registrar tanto exitosos como errores.
    /// Usa el patrón Result para un manejo de errores más funcional.
    /// </remarks>
    public async Task<Result<UserDto, UserError>> GetByIdAsync(string id)
    {
        logger.LogInformation("Buscando usuario con id: {Id}", id);
        
        if (string.IsNullOrWhiteSpace(id))
        {
            logger.LogWarning("Se intentó buscar usuario con ID vacío o nulo");
            return Result.Failure<UserDto, UserError>(new UserNotFoundError("El ID del usuario no puede estar vacío"));
        }
        
        return await userManager.FindByIdAsync(id) is { } user
            ? Result.Success<UserDto, UserError>(await user.ToDtoAsync(userManager))
                .Tap(_=>logger.LogInformation("Usuario encontrado exitosamente: {UserId}", id))
        : Result.Failure<UserDto, UserError>(new UserNotFoundError($"No se encontró usuario con id {id}"))
            .TapError(_=>logger.LogWarning("Usuario no encontrado con id: {Id}", id));
    }

    /// <inheritdoc/>
    /// <summary>
    /// Actualiza la información de un usuario existente, incluyendo datos personales,
    /// dirección y opcionalmente su imagen de avatar.
    /// </summary>
    /// <param name="id">El identificador único del usuario a actualizar.</param>
    /// <param name="userRequestDto">Objeto con los nuevos datos del usuario.</param>
    /// <param name="image">Archivo de imagen opcional para actualizar el avatar.</param>
    /// <returns>
    /// Un <see cref="Result{UserDto, UserError}"/> que contiene:
    /// <list type="bullet">
    ///     <item>Valor: El DTO del usuario con los datos actualizados</item>
    ///     <item>Error: <see cref="UserNotFoundError"/> o <see cref="UserUpdateError"/></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para><b>Operaciones realizadas:</b></para>
    /// <list type="number">
    ///     <item>Verifica la existencia del usuario</item>
    ///     <item>Si se proporciona imagen, la guarda en almacenamiento y actualiza el Avatar</item>
    ///     <item>Actualiza nombre, email y teléfono</item>
    ///     <item>Actualiza todos los campos de dirección</item>
    ///     <item>Persiste los cambios usando UserManager</item>
    /// </list>
    /// 
    /// <para><b>Errores posibles:</b></para>
    /// <list type="bullet">
    ///     <item>Usuario no encontrado</item>
    ///     <item>Error al subir la imagen</item>
    ///     <item>Error al actualizar en Identity</item>
    /// </list>
    /// </remarks>
    public async Task<Result<UserDto, UserError>> UpdateByIdAsync(long id, UserRequestDto userRequestDto, IFormFile? image)
    {
        logger.LogInformation("Iniciando actualización de usuario con id: {Id}", id);

        var found = await userManager.Users.Include(u=>u.Client)
            .ThenInclude(cl => cl.Address).Where(u=>u.Id == id).FirstOrDefaultAsync();
        
        if (found == null)
        {
            logger.LogError("Usuario no encontrado con id: {Id}", id);
            return Result.Failure<UserDto, UserError>(new UserNotFoundError($"No se encontró usuario con id {id}"));
        }

        if (image != null)
        {
            logger.LogInformation("Guardando imagen de avatar para usuario: {Id}", id);
            var img = await storage.SaveFileAsync(image, "users");
            if (img.IsSuccess) 
            {
                found.Avatar = img.Value;
                logger.LogInformation("Avatar actualizado exitosamente: {AvatarPath}", img.Value);
            }
            else 
            {
                logger.LogError("Error al subir imagen: {Error}", img.Error.Message);
                return Result.Failure<UserDto, UserError>(new UserUpdateError($"Error al subir imagen: {img.Error.Message}"));
            }
        }
            
        found.Name = userRequestDto.Nombre;
        found.Client.Name = userRequestDto.Nombre;
        
        found.Client.Address.City = userRequestDto.Ciudad;
        found.Client.Address.Country = userRequestDto.Ciudad;
        found.Client.Address.Province = userRequestDto.Provincia;
        found.Client.Address.PostalCode = Convert.ToInt32(userRequestDto.CodigoPostal);
        found.Client.Address.Street = userRequestDto.Calle;
        found.Client.Email = userRequestDto.Email;
        found.Email = userRequestDto.Email;
       
        if (userRequestDto.Telefono != null)
        {
            found.Client.Phone = userRequestDto.Telefono;
            found.PhoneNumber = userRequestDto.Telefono;
        }
        
        logger.LogInformation("Persistiendo cambios en usuario: {Id}", id);
        var result = await userManager.UpdateAsync(found);

        if (!result.Succeeded)
        {
            logger.LogError("Error actualizando usuario {Id}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Description)));
            return Result.Failure<UserDto, UserError>(
                new UserUpdateError("Error actualizando usuario")
            );
        }

        logger.LogInformation("Usuario actualizado correctamente: {Id}", id);

        return Result.Success<UserDto, UserError>(
            await found.ToDtoAsync(userManager)
        );
    }

    /// <inheritdoc/>
    /// <summary>
    /// Desactiva (bannea) a un usuario mediante borrado lógico.
    /// </summary>
    /// <param name="banUserId">El identificador único del usuario a banear.</param>
    /// <returns>Una tarea asíncrona que representa la operación.</returns>
    /// <remarks>
    /// <para>Esta operación:</para>
    /// <list type="bullet">
    ///     <item>Busca el usuario por su ID</item>
    ///     <item>Establece la propiedad IsDeleted a true</item>
    ///     <item>Persiste el cambio usando UserManager</item>
    /// </list>
    /// 
    /// <para><b>Nota:</b> El método no lanza excepción si el usuario no existe, 
    /// simplemente registra un error y retorna.</para>
    /// </remarks>
    public async Task BanUserById(string banUserId)
    {
        logger.LogInformation("Iniciando baneo de usuario: {UserId}", banUserId);
        
        var found = await userManager.FindByIdAsync(banUserId);
        if (found==null)
        {
            logger.LogError("Usuario no encontrado para baneo: {UserId}", banUserId);
            return;
        }
        
        found.IsDeleted = true;
        await userManager.UpdateAsync(found);
        
        logger.LogInformation("Usuario baneado exitosamente: {UserId}", banUserId);
    }

    /// <inheritdoc/>
    /// <summary>
    /// Cuenta el número total de usuarios activos en el sistema.
    /// </summary>
    /// <returns>
    /// El número de usuarios donde IsDeleted es false.
    /// </returns>
    /// <remarks>
    /// Este método es útil para estadísticas y paginación.
    /// Solo cuenta usuarios que no han sido baneados.
    /// </remarks>
    public async Task<int> GetTotalUsersCountAsync()
    {
        logger.LogDebug("Obteniendo conteo total de usuarios activos");
        return await userManager.Users.CountAsync(u => u.IsDeleted == false);
    }
}
