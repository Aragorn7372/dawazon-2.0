namespace dawazonBackend.Cart.Dto;

public record CartResponseDto(
    string Id,
    long UserId,
    bool Purchased,
    ClientDto Client,
    List<SaleLineDto> CartLines,
    int TotalItems,
    double Total
    );