using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Products.Mapper;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Products.Repository.Productos;
using dawazonBackend.Users.Errors;
using dawazonBackend.Users.Models;
using HotChocolate.Data.Sorting.Expressions;
using Microsoft.AspNetCore.Identity;

namespace dawazonBackend.Users.Service.Favs;

public class FavService(ILogger<FavService> logger,UserManager<User> manager, IProductRepository products): IFavService
{
    public async Task<Result<bool,DomainError>> AddFav(string productId, long userId)
    {
        logger.LogInformation($"Añadiendo a favoritos prodicto con id: {productId}");
        var user = await manager.FindByIdAsync(userId.ToString());
        if(user== null) return Result.Failure<bool,DomainError>(new UserNotFoundError("no se encontro usuario con ese id"));
        if (user.ProductsFavs.Contains(productId)) return Result.Failure<bool,DomainError>(new UserHasThatProductError("Ese usuario tenia ya ese producto guardad"));
        user.ProductsFavs.Add(productId);
        var updated=await manager.UpdateAsync(user);
        if (!updated.Succeeded) return Result.Failure<bool,DomainError>(new UserError(string.Join(", ", updated.Errors.Select(x => x.Description))));
        return Result.Success<bool,DomainError>(updated.Succeeded);
    }

    public async Task<Result<bool,DomainError>> RemoveFav(string productId, long userId)
    {
        logger.LogInformation($"Quitando de favoritos producto con id {productId}");
        var user = await manager.FindByIdAsync(userId.ToString());
        if(user== null) return Result.Failure<bool,DomainError>(new UserNotFoundError("no se encontro usuario con ese id"));
        if (!user.ProductsFavs.Contains(productId)) return Result.Failure<bool,DomainError>(new UserHasThatProductError("Ese usuario tenia no ese producto guardad"));
        user.ProductsFavs.Remove(productId);
        var updated=await manager.UpdateAsync(user);
        if (!updated.Succeeded) return Result.Failure<bool,DomainError>(new UserError(string.Join(", ", updated.Errors.Select(x => x.Description))));
        return Result.Success<bool,DomainError>(updated.Succeeded);
    }

    public async Task<Result<PageResponseDto<ProductResponseDto>,DomainError>> GetFavs(long userId, FilterDto pageable)
    {
        logger.LogInformation(" buscando productos favoritos");
        var user = await manager.FindByIdAsync(userId.ToString());
        if(user== null) return Result.Failure<PageResponseDto<ProductResponseDto>,DomainError>(new UserNotFoundError("no se encontro usuario con ese id"));
        var productsList = (await Task.WhenAll(
                user.ProductsFavs.Select(it => products.GetProductAsync(it))
            ))
            .OfType<Product>()
            .Select(it=>it.ToDto())
            .ToList(); // oki
        productsList = ApplySorting(productsList, pageable.SortBy, pageable.Direction);
        var page = pageable.Page < 0 ? 0 : pageable.Page;
        var size = pageable.Size <= 0 ? 10 : pageable.Size;

        var response = productsList
            .Skip(page * size)
            .Take(size)
            .ToList();
        var totalCount = productsList.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / pageable.Size);
        return Result.Success<PageResponseDto<ProductResponseDto>,DomainError>(new PageResponseDto<ProductResponseDto>(
            Content: response,
            TotalPages: totalPages,
            TotalElements: totalCount,
            PageSize: pageable.Size,
            PageNumber: pageable.Page,
            TotalPageElements: response.Count,
            SortBy: pageable.SortBy,
            Direction: pageable.Direction));
    }
    private List<ProductResponseDto> ApplySorting(List<ProductResponseDto> product, string? sortBy, string? direction)
    {
        bool desc = direction?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "name" => desc 
                ? product.OrderByDescending(p => p.Name).ToList()
                : product.OrderBy(p => p.Name).ToList(),

            "price" => desc
                ? product.OrderByDescending(p => p.Price).ToList()
                : product.OrderBy(p => p.Price).ToList(),

            "stock" => desc
                ? product.OrderByDescending(p => p.Stock).ToList()
                : product.OrderBy(p => p.Stock).ToList(),

            _ => product.OrderBy(p => p.Id).ToList() 
        };
    }
}