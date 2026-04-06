namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para el resumen de un pedido en el listado "Mis Pedidos".
/// </summary>
/// <remarks>
/// Proporciona información resumida de cada pedido para mostrar en listas.
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Identificación: Id, CreatedAt</item>
///     <item>Totales: Total, TotalItems</item>
///     <item>Cliente: ClientName, ClientCity, ClientPostalCode</item>
/// </list>
/// </remarks>
public class CartOrderSummaryViewModel
{
    /// <summary>Identificador único del pedido.</summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>Fecha de creación del pedido.</summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>Total monetario del pedido.</summary>
    public double Total { get; set; }
    
    /// <summary>Cantidad total de artículos.</summary>
    public int TotalItems { get; set; }

    /// <summary>Nombre del cliente.</summary>
    /// <remarks>
    /// Usado para mostrar en la columna "Enviar a".
    /// </remarks>
    public string ClientName { get; set; } = string.Empty;
    
    /// <summary>Ciudad del cliente.</summary>
    public string ClientCity { get; set; } = string.Empty;
    
    /// <summary>Código postal del cliente.</summary>
    public int ClientPostalCode { get; set; }
}
