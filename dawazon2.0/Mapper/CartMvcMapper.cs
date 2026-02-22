using dawazonBackend.Cart.Dto;
using dawazon2._0.Models;

namespace dawazon2._0.Mapper;

/// <summary>
/// Métodos de extensión para mapear DTOs de carrito a ViewModels MVC.
/// </summary>
public static class CartMvcMapper
{
    /// <summary>
    /// Convierte un <see cref="CartResponseDto"/> en un <see cref="CartOrderSummaryViewModel"/>.
    /// </summary>
    public static CartOrderSummaryViewModel ToOrderSummaryViewModel(this CartResponseDto dto)
    {
        // CartResponseDto no incluye CreatedAt; usamos el SaleLineDto más antiguo como fecha aproximada
        // En su defecto dejamos DateTime.MinValue para que la vista lo trate.
        var createdAt = dto.CartLines
            .Select(l => l.CreateAt)
            .DefaultIfEmpty(DateTime.MinValue)
            .Min();

        return new CartOrderSummaryViewModel
        {
            Id            = dto.Id,
            CreatedAt     = createdAt,
            Total         = dto.Total,
            TotalItems    = dto.TotalItems,
            ClientName    = dto.Client.Name,
            ClientCity    = dto.Client.City,
            ClientPostalCode = dto.Client.PostalCode
        };
    }

    /// <summary>
    /// Convierte un <see cref="CartResponseDto"/> en un <see cref="CartOrderDetailViewModel"/>.
    /// </summary>
    public static CartOrderDetailViewModel ToOrderDetailViewModel(this CartResponseDto dto)
    {
        var createdAt = dto.CartLines
            .Select(l => l.CreateAt)
            .DefaultIfEmpty(DateTime.MinValue)
            .Min();

        return new CartOrderDetailViewModel
        {
            Id              = dto.Id,
            CreatedAt       = createdAt,
            Total           = dto.Total,
            TotalItems      = dto.TotalItems,
            ClientName      = dto.Client.Name,
            ClientStreet    = dto.Client.Street,
            ClientNumber    = dto.Client.Number,
            ClientCity      = dto.Client.City,
            ClientPostalCode= dto.Client.PostalCode,
            ClientProvince  = dto.Client.Province,
            ClientCountry   = dto.Client.Country,
            Lines           = dto.CartLines.Select(l => l.ToCartLineViewModel()).ToList()
        };
    }

    /// <summary>
    /// Convierte una lista paginada de <see cref="CartResponseDto"/> en un <see cref="CartOrderListViewModel"/>.
    /// </summary>
    public static CartOrderListViewModel ToOrderListViewModel(
        this IEnumerable<CartResponseDto> dtos,
        int pageNumber,
        int totalPages,
        long totalElements)
    {
        return new CartOrderListViewModel
        {
            Orders        = dtos.Select(d => d.ToOrderSummaryViewModel()).ToList(),
            PageNumber    = pageNumber,
            TotalPages    = totalPages,
            TotalElements = totalElements
        };
    }

    private static CartLineViewModel ToCartLineViewModel(this SaleLineDto line)
    {
        return new CartLineViewModel
        {
            ProductId   = line.ProductId,
            ProductName = line.ProductName,
            ManagerName = line.ManagerName,
            Quantity    = line.Quantity,
            ProductPrice= line.ProductPrice,
            TotalPrice  = line.TotalPrice,
            Status      = line.Status
        };
    }
}
