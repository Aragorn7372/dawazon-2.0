namespace dawazonBackend.Common.Dto;

//Filtros para el getAll con querys.
//Los que son nulables es porque pueden venir o no.
//El resto no son nullables pero se les da un valor por defecto
public record FilterDto(
    string? Nombre,
    string? Categoria,
    int Page = 0,
    int Size = 10,
    string SortBy = "id",
    string Direction = "asc"
);