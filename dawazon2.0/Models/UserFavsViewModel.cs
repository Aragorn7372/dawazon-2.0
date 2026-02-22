using dawazonBackend.Products.Models.Dto;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la lista de productos favoritos del usuario.
/// </summary>
public class UserFavsViewModel
{
    public List<ProductResponseDto> Products { get; set; } = [];

    public int PageNumber    { get; set; }
    public int TotalPages    { get; set; }
    public long TotalElements { get; set; }

    public bool First => PageNumber == 0;
    public bool Last  => PageNumber >= TotalPages - 1;
    public int PrevPage => Math.Max(0, PageNumber - 1);
    public int NextPage => Math.Min(TotalPages - 1, PageNumber + 1);
}
