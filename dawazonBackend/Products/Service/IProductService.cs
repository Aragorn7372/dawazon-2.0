using CSharpFunctionalExtensions;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Errors;
using dawazonBackend.Products.Models;
using dawazonBackend.Products.Models.Dto;

namespace dawazonBackend.Products.Service;

public interface IProductService
{
    Task<Result<PageResponseDto<ProductResponseDto>, ProductError>> GetAllAsync(FilterDto filter);
    
    Task<Result<ProductResponseDto, ProductError>> GetByIdAsync(string id);
    
    Task<Result<long, ProductError>> GetUserProductIdAsync(string id);
    
    Task<Result<ProductResponseDto, ProductError>> CreateAsync(ProductRequestDto dto);
    
    Task<Result<ProductResponseDto, ProductError>> UpdateAsync(string id, ProductRequestDto dto);
    
    Task<Result<ProductResponseDto, ProductError>> PatchAsync(string id, ProductPatchRequestDto dto);
    
    Task<Result<ProductResponseDto, ProductError>> DeleteAsync(string id);

    Task<Result<ProductResponseDto, ProductError>> UpdateImageAsync(string id, List<IFormFile> images);

    Task<List<string>> GetAllCategoriesAsync();

    Task<Result<ProductResponseDto, ProductError>> AddCommentAsync(string id, Comment comment);
}