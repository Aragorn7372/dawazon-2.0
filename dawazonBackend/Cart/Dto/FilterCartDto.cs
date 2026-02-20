namespace dawazonBackend.Cart.Dto;

public record FilterCartDto(
    long? managerId,
    bool? isAdmin,
    bool? purchased,
    int Page = 0,
    int Size = 10,
    string SortBy = "id",
    string Direction = "asc" 
);