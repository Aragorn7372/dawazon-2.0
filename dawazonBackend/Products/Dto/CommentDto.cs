namespace dawazonBackend.Products.Models.Dto;

/// <summary>
/// DTO para representar un comentario en un producto.
/// </summary>
/// <remarks>
/// Se utiliza para mostrar los comentarios asociados a un producto.
/// </remarks>
public record CommentDto(
    /// <summary>
    /// Nombre del usuario que hizo el comentario.
    /// </summary>
    string userName,

    /// <summary>
    /// Contenido del comentario.
    /// </summary>
    string comment,

    /// <summary>
    /// Indica si el usuario recomienda el producto.
    /// </summary>
    bool recommended,

    /// <summary>
    /// Indica si el comentario está verificado (compra verificada).
    /// </summary>
    bool verified
    );