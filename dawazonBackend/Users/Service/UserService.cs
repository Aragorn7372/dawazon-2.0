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

public class UserService(ILogger<UserService> logger,UserManager<User> userManager): IUserService
{
    public async Task<PageResponseDto<UserDto>> GetAllAsync(FilterDto filters)
    {
        logger.LogInformation("Getting all users");

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

    public async Task<Result<UserDto, UserError>> GetByIdAsync(string id)
    {
        logger.LogInformation($"Getting user with id {id}");
        return await userManager.FindByIdAsync(id) is { } user
            ? Result.Success<UserDto, UserError>(await user.ToDtoAsync(userManager))
                .Tap(_=>logger.LogInformation("todo good"))
        : Result.Failure<UserDto, UserError>(new UserNotFoundError($"No se encontro usuario con id {id}"))
            .TapError(_=>logger.LogWarning($"No se encontro usuario con id {id}"));
    }

    public async Task<Result<UserDto, UserError>> UpdateByIdAsync(long id, UserRequestDto userRequestDto)
    {
        var found = await userManager.Users.Include(u=>u.Client)
            .ThenInclude(cl => cl.Address).Where(u=>u.Id == id).FirstOrDefaultAsync();
        if (found == null)
        {
            logger.LogError($"User with id {id} not found");
            return Result.Failure<UserDto, UserError>(new UserNotFoundError($"No se encontro usuario con id {id}"));
        }

        found.Name = userRequestDto.Nombre;
        found.Client.Name = userRequestDto.Nombre;
        // Dirección
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
        var result = await userManager.UpdateAsync(found);

        if (!result.Succeeded)
        {
            logger.LogError("Error updating user: {Errors}", result.Errors);
            return Result.Failure<UserDto, UserError>(
                new UserUpdateError("Error actualizando usuario")
            );
        }

        logger.LogInformation("User updated successfully");

        return Result.Success<UserDto, UserError>(
            await found.ToDtoAsync(userManager)
        );
    }

    public async Task BanUserById(string banUserId)
    {
        var found = await userManager.FindByIdAsync(banUserId);
        if (found==null)
        {
            logger.LogError($"User with id {banUserId} not found");
            return;
        }
        found.IsDeleted = true;
        await userManager.UpdateAsync(found);
    }
}