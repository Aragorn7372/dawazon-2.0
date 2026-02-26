using dawazon2._0.Models;

namespace dawazon2._0.Pdf;

/// <summary>
/// Genera un PDF con el resumen de un pedido.
/// </summary>
public interface IOrderPdfService
{
    /// <summary>
    /// Genera el PDF del pedido y devuelve sus bytes.
    /// </summary>
    Task<byte[]> GenerateOrderPdfAsync(CartOrderDetailViewModel order);
}
