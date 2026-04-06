using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para los formularios de creación y edición de productos.
/// </summary>
/// <remarks>
/// Proporciona datos y validaciones para crear o editar productos.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>DataAnnotations: Validaciones de entrada</item>
///     <item>SelectListItem: Opciones de categorías</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Validaciones: Required, Range para números</item>
///     <item>Soporte para múltiples imágenes</item>
///     <item>Modo edición (IsEdit)</item>
/// </list>
/// </remarks>
public class ProductFormViewModel
{
    /// <summary>ID del producto (null al crear, relleno al editar).</summary>
    public string? Id { get; set; }

    /// <summary>Nombre del producto.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: El nombre es obligatorio</item>
    ///     <item>Display: Etiqueta "Nombre"</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Precio del producto en euros.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: El precio es obligatorio</item>
    ///     <item>Range: Mayor que 0</item>
    ///     <item>Display: Etiqueta "Precio (€)"</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0")]
    [Display(Name = "Precio (€)")]
    public double Price { get; set; }

    /// <summary>Categoría del producto.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: La categoría es obligatoria</item>
    ///     <item>Display: Etiqueta "Categoría"</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "La categoría es obligatoria")]
    [Display(Name = "Categoría")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Descripción detallada del producto.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: La descripción es obligatoria</item>
    ///     <item>Display: Etiqueta "Descripción"</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "La descripción es obligatoria")]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Cantidad disponible en stock.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Range: No puede ser negativo</item>
    ///     <item>Display: Etiqueta "Stock"</item>
    /// </list>
    /// </remarks>
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    [Display(Name = "Stock")]
    public int Stock { get; set; }

    /// <summary>Imágenes nuevas a subir (opcional, múltiples).</summary>
    /// <remarks>
    /// Solo se usa al crear o editar productos. Admite múltiples archivos.
    /// </remarks>
    [Display(Name = "Imágenes")]
    public List<IFormFile>? Images { get; set; }

    /// <summary>Imágenes actuales del producto (solo edición).</summary>
    /// <remarks>
    /// Muestra las imágenes existentes al editar para permitir eliminarlas.
    /// </remarks>
    public List<string> CurrentImages { get; set; } = [];

    /// <summary>Categorías disponibles para el select del formulario.</summary>
    /// <remarks>
    /// Se.popula desde el backend al cargar el formulario.
    /// </remarks>
    public List<SelectListItem> AvailableCategories { get; set; } = [];

    /// <summary>Indica si es formulario de edición.</summary>
    /// <remarks>
    /// True si Id no es null (editar), false si es null (crear).
    /// </remarks>
    public bool IsEdit => Id is not null;
}
