using System.ComponentModel.DataAnnotations;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para agregar comentarios a un producto.
/// </summary>
/// <remarks>
/// Recoge los datos del formulario de comentarios.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>DataAnnotations: Validaciones</item>
/// </list>
/// 
/// <para><b>Características:</b></para>
/// <list type="bullet">
///     <item>Producto: ProductId</item>
///     <item>Comentario: CommentText (required)</item>
///     <item>Recomendación: Recommended (required)</item>
/// </list>
/// </remarks>
public class AddCommentViewModel
{
    /// <summary>ID del producto al que se agrega el comentario.</summary>
    public string ProductId { get; set; }=string.Empty;

    /// <summary>Texto del comentario.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    /// </list>
    /// </remarks>
    [Required]
    public string CommentText { get; set; }=string.Empty;

    /// <summary>Indica si el usuario recomienda el producto.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    /// </list>
    /// </remarks>
    [Required]
    public bool Recommended { get; set; }
}