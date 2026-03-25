namespace dawazonBackend.Users.Models;

/// <summary>
/// Constantes de roles de usuario en el sistema.
/// </summary>
/// <remarks>
/// Define los roles disponibles para los usuarios:
/// <list type="bullet">
///     <item>Admin: Acceso completo al panel de administración</item>
///     <item>User: Usuario regular de la tienda</item>
///     <item>Manager: Gestor de productos (puede crear/gestionar productos)</item>
/// </list>
/// </remarks>
public static class UserRoles
{
    /// <summary>
    /// Rol de administrador con acceso completo.
    /// </summary>
    public const string ADMIN = "Admin";
    
    /// <summary>
    /// Rol de usuario regular.
    /// </summary>
    public const string USER = "User";
    
    /// <summary>
    /// Rol de gestor de productos.
    /// </summary>
    public const string MANAGER = "Manager";
}