using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Common.Error;
using dawazonBackend.Products.Models.Dto;

namespace dawazonBackend.Users.Service.Favs;

public interface IFavService
{
public Task<Result<bool, DomainError>> AddFav(string productId, long userId);

public Task<Result<bool, DomainError>> RemoveFav(string productId, long userId);

public Task<Result<PageResponseDto<ProductResponseDto>, DomainError>> GetFavs(long userId, FilterDto pageable);
}