namespace dawazonBackend.Cart.Dto;

public record FilterCartDto(
    long? managerId,
    bool? isAdmin,
    string? purchased,
    int Page = 0,
    int Size = 10,
    string SortBy = "id",
    string Direction = "asc" 
);