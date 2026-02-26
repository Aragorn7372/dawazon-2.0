namespace dawazonBackend.Products.Models.Dto;

public record ProductResponseDto(
    string Id,
    string Name,
    double Price,
    int Stock,
    string Category,
    string Description,
    List<CommentDto> Comments,
    List<string> Images,
    long CreatorId = 0
);