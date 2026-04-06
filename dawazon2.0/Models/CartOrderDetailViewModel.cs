namespace dawazon2._0.Models;

/// <summary>
/// ViewModel para la vista de detalle completo de un pedido.
/// </summary>
/// <remarks>
/// Proporciona todos los datos de un pedido específico.
/// 
/// <para><b>Propiedades:</b></para>
/// <list type="bullet">
///     <item>Identificación: Id, CreatedAt</item>
///     <item>Totales: Total, TotalItems</item>
///     <item>Productos: Lines (lista de CartLineViewModel)</item>
///     <item>Dirección: ClientName, ClientStreet, ClientNumber, ClientCity, ClientPostalCode, ClientProvince, ClientCountry</item>
/// </list>
/// </remarks>
public class CartOrderDetailViewModel
{
    /// <summary>Identificador único del pedido.</summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>Fecha de creación del pedido.</summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>Total monetario del pedido.</summary>
    public double Total { get; set; }
    
    /// <summary>Cantidad total de artículos.</summary>
    public int TotalItems { get; set; }

    /// <summary>Lista de productos comprados.</summary>
    public List<CartLineViewModel> Lines { get; set; } = [];

    /// <summary>Nombre del cliente.</summary>
    public string ClientName { get; set; } = string.Empty;
    
    /// <summary>Dirección - Calle.</summary>
    public string ClientStreet { get; set; } = string.Empty;
    
    /// <summary>Dirección - Número.</summary>
    public int ClientNumber { get; set; }
    
    /// <summary>Dirección - Ciudad.</summary>
    public string ClientCity { get; set; } = string.Empty;
    
    /// <summary>Dirección - Código postal.</summary>
    public int ClientPostalCode { get; set; }
    
    /// <summary>Dirección - Provincia.</summary>
    public string ClientProvince { get; set; } = string.Empty;
    
    /// <summary>Dirección - País.</summary>
    public string ClientCountry { get; set; } = string.Empty;
}
