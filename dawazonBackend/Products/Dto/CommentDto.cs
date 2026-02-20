namespace dawazonBackend.Products.Models.Dto;

public record CommentDto(
    string userName,
    string comment,
    bool recommended,
    bool verified
    );