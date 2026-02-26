using dawazon2._0.Models;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Models.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace dawazon2._0.Mapper;

/// <summary>
/// Convierte entre DTOs del backend de productos y los ViewModels de la capa MVC.
/// </summary>
public static class ProductMvcMapper
{

    /// <summary>Convierte a ViewModel ligero para tarjetas de lista.</summary>
    public static ProductSummaryViewModel ToSummaryViewModel(this ProductResponseDto dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category,
            FirstImage = dto.Images.FirstOrDefault() ?? string.Empty,
            CreatorId = dto.CreatorId
        };

    /// <summary>Convierte a ViewModel completo para la vista de detalle.</summary>
    public static ProductDetailViewModel ToDetailViewModel(this ProductResponseDto dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category,
            Description = dto.Description,
            Images = dto.Images,
            Comments = dto.Comments
        };

    /// <summary>Convierte a ViewModel de formulario para la vista de edición.</summary>
    public static ProductFormViewModel ToFormViewModel(this ProductResponseDto dto,
        List<SelectListItem>? categories = null) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category,
            Description = dto.Description,
            CurrentImages = dto.Images,
            AvailableCategories = categories ?? []
        };

    /// <summary>Convierte la página de respuesta del servicio a ProductListViewModel.</summary>
    public static ProductListViewModel ToListViewModel(
        this PageResponseDto<ProductResponseDto> page,
        string? searchName = null,
        string? searchCategory = null,
        string sortBy = "id",
        string direction = "asc") =>
        new()
        {
            Products = page.Content.Select(ToSummaryViewModel).ToList(),
            TotalPages = page.TotalPages,
            PageNumber = page.PageNumber,
            TotalElements = page.TotalElements,
            SearchName = searchName,
            SearchCategory = searchCategory,
            SortBy = sortBy,
            Direction = direction
        };


    /// <summary>Convierte el formulario MVC a ProductRequestDto para el servicio.</summary>
    public static ProductRequestDto ToRequestDto(this ProductFormViewModel vm, long creatorId) =>
        new(
            Id: vm.Id,
            Name: vm.Name,
            Price: vm.Price,
            Category: vm.Category,
            Description: vm.Description,
            Images: vm.CurrentImages,
            Stock: vm.Stock,
            CreatorId: creatorId
        );
}
