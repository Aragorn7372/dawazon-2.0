using System.Globalization;
using dawazon2._0.Models;
using dawazonBackend.Cart.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace dawazon2._0.Pdf;

/// <summary>
/// Implementación de <see cref="IOrderPdfService"/> usando QuestPDF.
/// Construye un documento PDF estructurado con el resumen del pedido.
/// </summary>
public class OrderPdfService : IOrderPdfService
{
    private static readonly string AccentRed  = "#b12704";
    private static readonly string LightGrey  = "#f3f3f3";
    private static readonly string BorderGrey = "#dddddd";
    private static readonly string TextGrey   = "#555555";
    private static readonly string TextDark   = "#222222";

    static OrderPdfService()
    {
        // Licencia comunitaria — gratuita para proyectos open-source / personales
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc />
    public Task<byte[]> GenerateOrderPdfAsync(CartOrderDetailViewModel order)
    {
        var culture = new CultureInfo("es-ES");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32, Unit.Point);
                page.DefaultTextStyle(t => t.FontSize(11).FontColor(TextDark));

                page.Header().Column(col =>
                {
                    col.Item()
                       .Text("Dawazon — Resumen de pedido")
                       .FontSize(20).Bold().FontColor(AccentRed);

                    col.Item().PaddingTop(4).Text(txt =>
                    {
                        txt.Span("Nº pedido: ").Bold();
                        txt.Span(order.Id);
                        txt.Span("   |   ");
                        txt.Span("Fecha: ").Bold();
                        var date = order.CreatedAt != DateTime.MinValue
                            ? order.CreatedAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                            : "—";
                        txt.Span(date);
                    });

                    col.Item().PaddingTop(8)
                       .LineHorizontal(1).LineColor(BorderGrey);
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Item().PaddingBottom(4)
                       .Text("Productos").FontSize(13).Bold().FontColor(TextGrey);

                    // Tabla de líneas
                    col.Item().Table(table =>
                    {
                        // Definición de columnas
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);  // Producto
                            cols.RelativeColumn(1);  // Cantidad
                            cols.RelativeColumn(2);  // Precio unit.
                            cols.RelativeColumn(2);  // Subtotal
                            cols.RelativeColumn(2);  // Estado
                        });

                        // Encabezados
                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(LightGrey)
                             .BorderBottom(2).BorderColor(BorderGrey)
                             .Padding(6);

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell)
                                  .Text("Producto").Bold().FontSize(10).FontColor(TextGrey);
                            header.Cell().Element(HeaderCell)
                                  .AlignCenter().Text("Cantidad").Bold().FontSize(10).FontColor(TextGrey);
                            header.Cell().Element(HeaderCell)
                                  .AlignRight().Text("Precio unit.").Bold().FontSize(10).FontColor(TextGrey);
                            header.Cell().Element(HeaderCell)
                                  .AlignRight().Text("Subtotal").Bold().FontSize(10).FontColor(TextGrey);
                            header.Cell().Element(HeaderCell)
                                  .AlignCenter().Text("Estado").Bold().FontSize(10).FontColor(TextGrey);
                        });

                        foreach (var line in order.Lines)
                        {
                            var statusLabel = line.Status switch
                            {
                                Status.EnCarrito => "En carrito",
                                Status.Preparado => "Preparado",
                                Status.Enviado   => "Enviado",
                                Status.Recibido  => "Recibido",
                                Status.Cancelado => "Cancelado",
                                _                => line.Status.ToString()
                            };

                            static IContainer DataCell(IContainer c) =>
                                c.BorderBottom(1).BorderColor("#eeeeee").Padding(6);

                            table.Cell().Element(DataCell).Text(line.ProductName);
                            table.Cell().Element(DataCell).AlignCenter()
                                 .Text(line.Quantity.ToString());
                            table.Cell().Element(DataCell).AlignRight()
                                 .Text($"{line.ProductPrice.ToString("0.00", culture)} €");
                            table.Cell().Element(DataCell).AlignRight()
                                 .Text($"{line.TotalPrice.ToString("0.00", culture)} €");
                            table.Cell().Element(DataCell).AlignCenter()
                                 .Text(statusLabel);
                        }

                        var totalStr = order.Total.ToString("0.00", culture);

                        static IContainer TotalCell(IContainer c) =>
                            c.BorderTop(2).BorderColor(BorderGrey).Padding(8);

                        table.Cell().ColumnSpan(3).Element(TotalCell)
                             .AlignRight().Text("TOTAL").Bold().FontSize(12);
                        table.Cell().Element(TotalCell)
                             .AlignRight().Text($"{totalStr} €").Bold().FontSize(12).FontColor(AccentRed);
                        table.Cell().Element(TotalCell).Text(string.Empty);
                    });

                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(BorderGrey)
                           .Padding(12).Column(inner =>
                        {
                            inner.Item().Text("DIRECCIÓN DE ENVÍO")
                                 .Bold().FontSize(10).FontColor(TextGrey);
                            inner.Item().PaddingTop(6)
                                 .Text(order.ClientName).Bold();
                            inner.Item()
                                 .Text($"{order.ClientStreet} {order.ClientNumber}");
                            inner.Item()
                                 .Text($"{order.ClientCity}, {order.ClientProvince} {order.ClientPostalCode}");
                            inner.Item()
                                 .Text(order.ClientCountry);
                        });

                        row.ConstantItem(16); // spacer

                        row.RelativeItem().Border(1).BorderColor(BorderGrey)
                           .Padding(12).Column(inner =>
                        {
                            inner.Item().Text("RESUMEN ECONÓMICO")
                                 .Bold().FontSize(10).FontColor(TextGrey);
                            inner.Item().PaddingTop(6)
                                 .Text($"Artículos: {order.TotalItems}");
                            inner.Item().Text("Envío: Gratis");
                            inner.Item().PaddingTop(4)
                                 .Text($"Total: {order.Total.ToString("0.00", culture)} €").Bold();
                        });
                    });
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span($"Documento generado el {DateTime.Now:dd/MM/yyyy HH:mm} — Dawazon 2.0")
                       .FontSize(9).FontColor("#999999");
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }
}
