namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la vista de perfil del usuario.
/// </summary>
public class UserProfileViewModel
{
    public string Name  { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Avatar { get; set; } = "default.png";

    // Dirección
    public string Street     { get; set; } = string.Empty;
    public string City       { get; set; } = string.Empty;
    public string Province   { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country    { get; set; } = string.Empty;

    public int FavCount { get; set; }

    /// <summary>URL completa del avatar (fallback a imagen por defecto).</summary>
    public string AvatarUrl => string.IsNullOrWhiteSpace(Avatar) || Avatar == "default.png"
        ? "/uploads/users/default.png"
        : $"{Avatar}";
}
