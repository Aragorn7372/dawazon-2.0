using System.ComponentModel.DataAnnotations;
using dawazonBackend.Cart.Models;

namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la edición del estado de una venta (línea de pedido).
/// </summary>
/// <remarks>
/// Permite modificar el estado de una línea de pedido específica.
/// 
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>DataAnnotations: Validaciones</item>
///     <item>Status: Enum del backend</item>
/// </list>
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Identificación: SaleId, ProductId, ProductName, ClientName</item>
///     <item>Información: Quantity, TotalPrice, CurrentStatus</item>
///     <item>Edición: NewStatus, Notes</item>
/// </list>
/// </remarks>
public class AdminSaleEditViewModel
{
    /// <summary>ID de la venta/línea de pedido.</summary>
    public string SaleId { get; set; } = string.Empty;
    
    /// <summary>ID del producto.</summary>
    public string ProductId { get; set; } = string.Empty;
    
    /// <summary>Nombre del producto.</summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>Nombre del cliente.</summary>
    public string ClientName { get; set; } = string.Empty;
    
    /// <summary>Cantidad comprada.</summary>
    public int Quantity { get; set; }
    
    /// <summary>Precio total de la línea.</summary>
    public double TotalPrice { get; set; }
    
    /// <summary>Estado actual de la venta.</summary>
    public Status CurrentStatus { get; set; }

    /// <summary>Nuevo estado a aplicar.</summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>Required: Obligatorio</item>
    /// </list>
    /// </remarks>
    [Required(ErrorMessage = "El nuevo estado es obligatorio")]
    public Status NewStatus { get; set; }
    
    /// <summary>Notas adicionales (opcional).</summary>
    public string? Notes { get; set; }
}
