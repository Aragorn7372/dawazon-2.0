using dawazon2._0.Models;

namespace dawazon2._0.Pdf;

/// <summary>
/// Interfaz para la generación de PDFs de pedidos.
/// </summary>
/// <remarks>
/// Define el contrato para generar documentos PDF con el resumen de un pedido.
///
/// <para><b>Dependencias:</b></para>
/// <list type="bullet">
///     <item>CartOrderDetailViewModel: Datos del pedido</item>
/// </list>
/// 
/// <para><b>Utilizado por:</b></para>
/// <list type="bullet">
///     <item>CartMvcController (descarga de PDF)</item>
/// </list>
/// </remarks>
public interface IOrderPdfService
{
    /// <summary>
    /// Genera el PDF del pedido y devuelve sus bytes.
    /// </summary>
    /// <param name="order">ViewModel con los datos del pedido.</param>
    /// <returns>Bytes del documento PDF generado.</returns>
    Task<byte[]> GenerateOrderPdfAsync(CartOrderDetailViewModel order);
}
