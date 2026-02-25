using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para los formularios de creación y edición de productos.
/// </summary>
public class ProductFormViewModel
{
    /// <summary>ID del producto (null al crear, relleno al editar).</summary>
    public string? Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0")]
    [Display(Name = "Precio (€)")]
    public double Price { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [Display(Name = "Categoría")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    [Display(Name = "Stock")]
    public int Stock { get; set; }

    /// <summary>Imágenes nuevas a subir (opcional, múltiples).</summary>
    [Display(Name = "Imágenes")]
    public List<IFormFile>? Images { get; set; }

    /// <summary>Imágenes actuales del producto (solo edición).</summary>
    public List<string> CurrentImages { get; set; } = [];

    /// <summary>Categorías disponibles para el select del formulario.</summary>
    public List<SelectListItem> AvailableCategories { get; set; } = [];

    /// <summary>True si es formulario de edición, false si es de creación.</summary>
    public bool IsEdit => Id is not null;
}
