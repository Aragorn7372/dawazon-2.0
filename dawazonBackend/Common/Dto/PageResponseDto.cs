namespace dawazonBackend.Common.Dto;

public record PageResponseDto<T>(
    List<T> Content,
    int TotalPages,
    long TotalElements,
    int PageSize,
    int PageNumber,
    int TotalPageElements,
    string SortBy,
    string Direction
)
{
    public bool Empty => Content.Count == 0;
    public bool First => PageNumber == 0;
    public bool Last => PageNumber >= TotalPages - 1;
}