using dawazon2._0.Mapper;
using dawazon2._0.Models;
using dawazonBackend.Common.Dto;
using dawazonBackend.Products.Models.Dto;
using dawazonBackend.Products.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;
using System.Collections.Generic;

namespace dawazonTest.dawazon2._0.Mapper;

[TestFixture]
public class ProductMvcMapperTests
{
    private ProductResponseDto CreateProductResponseDto()
    {
        return new ProductResponseDto(
            "PROD-1",
            "Test Product",
            99.99,
            10,
            "Category",
            "Description",
            new List<CommentDto>(),
            new List<string> { "img1.png", "img2.png" }
        );
    }

    [Test]
    public void ToSummaryViewModel_ShouldMapFields()
    {
        var dto = CreateProductResponseDto();
        var result = dto.ToSummaryViewModel();

        Assert.That(result.Id, Is.EqualTo("PROD-1"));
        Assert.That(result.Name, Is.EqualTo("Test Product"));
        Assert.That(result.Price, Is.EqualTo(99.99));
        Assert.That(result.Stock, Is.EqualTo(10));
        Assert.That(result.Category, Is.EqualTo("Category"));
        Assert.That(result.FirstImage, Is.EqualTo("img1.png"));
    }

    [Test]
    public void ToSummaryViewModel_EmptyImages_ShouldUseEmptyString()
    {
        var dto = new ProductResponseDto("PROD-1", "Test", 10.0, 5, "Cat", "Desc", new List<CommentDto>(), new List<string>());
        var result = dto.ToSummaryViewModel();

        Assert.That(result.FirstImage, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToDetailViewModel_ShouldMapFields()
    {
        var dto = CreateProductResponseDto();
        var result = dto.ToDetailViewModel();

        Assert.That(result.Id, Is.EqualTo("PROD-1"));
        Assert.That(result.Name, Is.EqualTo("Test Product"));
        Assert.That(result.Price, Is.EqualTo(99.99));
        Assert.That(result.Description, Is.EqualTo("Description"));
        Assert.That(result.Category, Is.EqualTo("Category"));
        Assert.That(result.Stock, Is.EqualTo(10));
        Assert.That(result.Images, Has.Count.EqualTo(2));
        Assert.That(result.Comments, Is.Not.Null);
    }

    [Test]
    public void ToFormViewModel_WithCategories_ShouldMapFields()
    {
        var dto = CreateProductResponseDto();
        var categories = new List<SelectListItem> { new SelectListItem("Cat 1", "cat1") };
        var result = dto.ToFormViewModel(categories);

        Assert.That(result.Id, Is.EqualTo("PROD-1"));
        Assert.That(result.Name, Is.EqualTo("Test Product"));
        Assert.That(result.Price, Is.EqualTo(99.99));
        Assert.That(result.Description, Is.EqualTo("Description"));
        Assert.That(result.Category, Is.EqualTo("Category"));
        Assert.That(result.Stock, Is.EqualTo(10));
        Assert.That(result.CurrentImages, Has.Count.EqualTo(2));
        Assert.That(result.AvailableCategories, Has.Count.EqualTo(1));
    }

    [Test]
    public void ToFormViewModel_WithoutCategories_ShouldInitializeEmpty()
    {
        var dto = CreateProductResponseDto();
        var result = dto.ToFormViewModel();

        Assert.That(result.AvailableCategories, Is.Not.Null);
        Assert.That(result.AvailableCategories, Is.Empty);
    }

    [Test]
    public void ToListViewModel_ShouldMapFields()
    {
        var content = new List<ProductResponseDto> { CreateProductResponseDto() };
        var page = new PageResponseDto<ProductResponseDto>(content, 5, 100, 20, 10, 20, "price", "desc");

        var result = page.ToListViewModel("Test", "Category", "price", "desc");

        Assert.That(result.PageNumber, Is.EqualTo(10));
        Assert.That(result.TotalPages, Is.EqualTo(5));
        Assert.That(result.TotalElements, Is.EqualTo(100));
        Assert.That(result.SearchName, Is.EqualTo("Test"));
        Assert.That(result.SearchCategory, Is.EqualTo("Category"));
        Assert.That(result.SortBy, Is.EqualTo("price"));
        Assert.That(result.Direction, Is.EqualTo("desc"));
        Assert.That(result.Products, Has.Count.EqualTo(1));
    }

    [Test]
    public void ToRequestDto_ShouldMapFields()
    {
        var vm = new ProductFormViewModel
        {
            Id = "PROD-2",
            Name = "New Product",
            Price = 50.0,
            Category = "New Category",
            Description = "New Desc",
            CurrentImages = new List<string> { "img3.png" },
            Stock = 20
        };

        var result = vm.ToRequestDto(5L);

        Assert.That(result.Id, Is.EqualTo("PROD-2"));
        Assert.That(result.Name, Is.EqualTo("New Product"));
        Assert.That(result.Price, Is.EqualTo(50.0));
        Assert.That(result.Category, Is.EqualTo("New Category"));
        Assert.That(result.Description, Is.EqualTo("New Desc"));
        Assert.That(result.Stock, Is.EqualTo(20));
        Assert.That(result.Images, Has.Count.EqualTo(1));
        Assert.That(result.CreatorId, Is.EqualTo(5L));
    }
}