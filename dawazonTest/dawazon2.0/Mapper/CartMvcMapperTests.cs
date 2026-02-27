using dawazon2._0.Mapper;
using dawazonBackend.Cart.Dto;
using dawazonBackend.Cart.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dawazonTest.dawazon2._0.Mapper;

[TestFixture]
public class CartMvcMapperTests
{
    private CartResponseDto CreateDummyCartResponseDto(int lineCount = 1)
    {
        var lines = new List<SaleLineDto>();
        for (int i = 0; i < lineCount; i++)
        {
            lines.Add(new SaleLineDto
            {
                ProductId = $"PROD-{i}",
                ProductName = $"Product {i}",
                Quantity = 2,
                ProductPrice = 10.0,
                TotalPrice = 20.0,
                ManagerName = "Manager",
                Status = Status.EnCarrito,
                CreateAt = new DateTime(2023, 1, 1).AddDays(i)
            });
        }

        var clientDto = new ClientDto
        {
            Name = "John Doe",
            City = "New York",
            Street = "Broadway",
            Number = 123,
            PostalCode = 10001,
            Province = "NY",
            Country = "USA"
        };

        return new CartResponseDto(
            "CART-123",
            1L,
            true,
            clientDto,
            lines,
            lineCount * 2,
            lineCount * 20.0
        );
    }

    [Test]
    public void ToOrderSummaryViewModel_ShouldMapFieldsCorrectly()
    {
        // Arrange
        var dto = CreateDummyCartResponseDto();

        // Act
        var result = dto.ToOrderSummaryViewModel();

        // Assert
        Assert.That(result.Id, Is.EqualTo("CART-123"));
        Assert.That(result.Total, Is.EqualTo(20.0));
        Assert.That(result.TotalItems, Is.EqualTo(2));
        Assert.That(result.ClientName, Is.EqualTo("John Doe"));
        Assert.That(result.ClientCity, Is.EqualTo("New York"));
        Assert.That(result.ClientPostalCode, Is.EqualTo(10001));
        Assert.That(result.CreatedAt, Is.EqualTo(new DateTime(2023, 1, 1)));
    }

    [Test]
    public void ToOrderSummaryViewModel_WithNoLines_ShouldUseDateTimeMinValue()
    {
        // Arrange
        var dto = CreateDummyCartResponseDto(0);

        // Act
        var result = dto.ToOrderSummaryViewModel();

        // Assert
        Assert.That(result.CreatedAt, Is.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void ToOrderDetailViewModel_ShouldMapFieldsAndLinesCorrectly()
    {
        // Arrange
        var dto = CreateDummyCartResponseDto(2);

        // Act
        var result = dto.ToOrderDetailViewModel();

        // Assert
        Assert.That(result.Id, Is.EqualTo("CART-123"));
        Assert.That(result.Total, Is.EqualTo(40.0));
        Assert.That(result.ClientStreet, Is.EqualTo("Broadway"));
        Assert.That(result.ClientNumber, Is.EqualTo(123));
        Assert.That(result.ClientProvince, Is.EqualTo("NY"));
        Assert.That(result.ClientCountry, Is.EqualTo("USA"));
        Assert.That(result.CreatedAt, Is.EqualTo(new DateTime(2023, 1, 1)));
        
        Assert.That(result.Lines, Has.Count.EqualTo(2));
        Assert.That(result.Lines[0].ProductId, Is.EqualTo("PROD-0"));
        Assert.That(result.Lines[0].ProductName, Is.EqualTo("Product 0"));
        Assert.That(result.Lines[0].ManagerName, Is.EqualTo("Manager"));
        Assert.That(result.Lines[0].Status, Is.EqualTo(Status.EnCarrito));
    }

    [Test]
    public void ToOrderDetailViewModel_WithNoLines_ShouldUseDateTimeMinValue()
    {
        // Arrange
        var dto = CreateDummyCartResponseDto(0);

        // Act
        var result = dto.ToOrderDetailViewModel();

        // Assert
        Assert.That(result.CreatedAt, Is.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void ToOrderListViewModel_ShouldMapCorrectly()
    {
        // Arrange
        var dtos = new List<CartResponseDto> { CreateDummyCartResponseDto() };

        // Act
        var result = dtos.ToOrderListViewModel(1, 10, 50);

        // Assert
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.TotalPages, Is.EqualTo(10));
        Assert.That(result.TotalElements, Is.EqualTo(50));
        Assert.That(result.Orders, Has.Count.EqualTo(1));
        Assert.That(result.Orders[0].Id, Is.EqualTo("CART-123"));
    }
}