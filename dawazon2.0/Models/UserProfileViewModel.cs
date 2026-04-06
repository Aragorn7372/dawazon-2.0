namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la vista de perfil del usuario.
/// </summary>
/// <remarks>
/// Muestra los datos personales y dirección del usuario.
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Datos: Name, Email, Phone, Avatar</item>
///     <item>Dirección: Street, City, Province, PostalCode, Country</item>
///     <item>Estadísticas: FavCount</item>
/// </list>
/// 
/// <para><b>Propiedades calculadas:</b></para>
/// <list type="bullet">
///     <item>AvatarUrl: URL completa del avatar con fallback</item>
/// </list>
/// </remarks>
public class UserProfileViewModel
{
    /// <summary>Nombre completo del usuario.</summary>
    public string Name  { get; set; } = string.Empty;
    
    /// <summary>Correo electrónico del usuario.</summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>Teléfono de contacto.</summary>
    public string Phone { get; set; } = string.Empty;
    
    /// <summary>Nombre del archivo de avatar.</summary>
    public string Avatar { get; set; } = "default.png";

    /// <summary>Dirección - Calle y número.</summary>
    public string Street     { get; set; } = string.Empty;
    
    /// <summary>Dirección - Ciudad.</summary>
    public string City       { get; set; } = string.Empty;
    
    /// <summary>Dirección - Provincia.</summary>
    public string Province   { get; set; } = string.Empty;
    
    /// <summary>Dirección - Código postal.</summary>
    public string PostalCode { get; set; } = string.Empty;
    
    /// <summary>Dirección - País.</summary>
    public string Country    { get; set; } = string.Empty;

    /// <summary>Cantidad de productos en favoritos.</summary>
    public int FavCount { get; set; }

    /// <summary>
    /// URL completa del avatar con fallback a imagen por defecto.
    /// </summary>
    /// <remarks>
    /// Si Avatar está vacío o es "default.png", retorna "/uploads/users/default.png".
    /// </remarks>
    public string AvatarUrl => string.IsNullOrWhiteSpace(Avatar) || Avatar == "default.png"
        ? "/uploads/users/default.png"
        : $"{Avatar}";
}
