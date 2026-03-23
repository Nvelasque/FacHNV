using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using FacturacionHN.DTOs;

namespace FacturacionHN.Services;

public class FacturaPdfService
{
    public byte[] GenerarPdf(FacturaDto factura)
    {
        var colorPrimario = factura.ColorPrimario ?? "#1B5E20";
        var colorSecundario = factura.ColorSecundario ?? "#E8F5E9";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginHorizontal(30);
                page.MarginVertical(20);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComposeHeader(c, factura, colorPrimario, colorSecundario));
                page.Content().Element(c => ComposeContent(c, factura, colorPrimario, colorSecundario));
                page.Footer().Element(c => ComposeFooter(c, factura, colorPrimario));
            });
        }).GeneratePdf();
    }

    private void ComposeHeader(IContainer container, FacturaDto f, string primary, string secondary)
    {
        container.Column(col =>
        {
            // Barra superior de color
            col.Item().Height(4).Background(primary);

            // Encabezado principal
            col.Item().Border(1).BorderColor(primary).Row(row =>
            {
                // Datos del emisor (izquierda)
                row.RelativeItem(3).Background(secondary).Padding(10).Column(c =>
                {
                    c.Item().Text(f.EmisorRazonSocial).Bold().FontSize(14).FontColor(primary);
                    if (!string.IsNullOrEmpty(f.EmisorNombreComercial))
                        c.Item().Text(f.EmisorNombreComercial).FontSize(11).FontColor(primary);
                    c.Item().Height(3);
                    c.Item().Text($"RTN: {f.EmisorRTN}").Bold().FontSize(10);
                    c.Item().Text(f.EmisorDireccion).FontSize(9);
                    if (!string.IsNullOrEmpty(f.EmisorTelefono))
                        c.Item().Text($"Tel: {f.EmisorTelefono}").FontSize(9);
                    if (!string.IsNullOrEmpty(f.EmisorCorreo))
                        c.Item().Text(f.EmisorCorreo).FontSize(9);
                });

                // Datos de la factura (derecha)
                row.RelativeItem(2).Padding(10).Column(c =>
                {
                    c.Item().AlignCenter().Text("FACTURA").Bold().FontSize(16).FontColor(primary);
                    c.Item().AlignCenter().Text(f.NumeroFactura).Bold().FontSize(11);
                    c.Item().Height(6);
                    c.Item().Background(secondary).Padding(5).Column(info =>
                    {
                        info.Item().Text($"CAI: {f.NumeroCai}").FontSize(7);
                        info.Item().Text($"Rango: {f.RangoAutorizado}").FontSize(7);
                        info.Item().Text($"Fecha Límite: {f.FechaLimiteEmision:dd/MM/yyyy}").FontSize(7);
                        info.Item().Text($"Modalidad: {f.Modalidad}").FontSize(8).Bold();
                    });
                });
            });

            col.Item().Height(8);

            // Datos del cliente
            col.Item().Border(1).BorderColor(primary).Row(row =>
            {
                row.RelativeItem().Padding(8).Column(c =>
                {
                    c.Item().Text("CLIENTE").FontSize(7).FontColor(primary).Bold();
                    c.Item().Text(f.ClienteNombre).Bold().FontSize(10);
                    c.Item().Text($"RTN: {f.ClienteRTN}").FontSize(9);
                });
                row.RelativeItem().Padding(8).Column(c =>
                {
                    c.Item().Text("EMISIÓN").FontSize(7).FontColor(primary).Bold();
                    c.Item().Text($"{f.FechaEmision:dd/MM/yyyy HH:mm}").Bold().FontSize(10);
                    c.Item().Text($"Estado: {f.Estado}").FontSize(9);
                });
            });

            // Exoneración
            if (!string.IsNullOrEmpty(f.NumeroOrdenCompraExenta))
            {
                col.Item().Height(4);
                col.Item().Border(1).BorderColor("#F57F17").Background("#FFF9C4").Padding(6).Column(c =>
                {
                    c.Item().Text("CLIENTE EXONERADO").Bold().FontSize(8).FontColor("#F57F17");
                    c.Item().Text($"Orden Compra Exenta: {f.NumeroOrdenCompraExenta}").FontSize(8);
                    if (!string.IsNullOrEmpty(f.NumeroConstanciaRegistroExonerados))
                        c.Item().Text($"Constancia Registro Exonerados: {f.NumeroConstanciaRegistroExonerados}").FontSize(8);
                    if (!string.IsNullOrEmpty(f.NumeroRegistroSAG))
                        c.Item().Text($"Registro SAG: {f.NumeroRegistroSAG}").FontSize(8);
                });
            }

            col.Item().Height(8);
        });
    }

    private void ComposeContent(IContainer container, FacturaDto factura, string primary, string secondary)
    {
        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);    // #
                    columns.ConstantColumn(65);    // Código
                    columns.RelativeColumn(4);     // Descripción (mucho más ancha)
                    columns.ConstantColumn(35);    // Cant
                    columns.ConstantColumn(75);    // P. Unit
                    columns.ConstantColumn(60);    // Desc
                    columns.ConstantColumn(75);    // SubTotal
                    columns.ConstantColumn(65);    // ISV
                    columns.ConstantColumn(75);    // Total
                });

                // Header con color de la empresa
                table.Header(header =>
                {
                    var style = TextStyle.Default.Bold().FontSize(8).FontColor("#FFFFFF");
                    void HeaderCell(IContainer c, string text, bool right = false)
                    {
                        var cell = c.Background(primary).Padding(4);
                        if (right) cell.AlignRight().Text(text).Style(style);
                        else cell.Text(text).Style(style);
                    }
                    HeaderCell(header.Cell().Border(0.5f).BorderColor(primary), "#");
                    HeaderCell(header.Cell().Border(0.5f).BorderColor(primary), "Código");
                    HeaderCell(header.Cell().Border(0.5f).BorderColor(primary), "Descripción");
                    HeaderCell(header.Cell().Border(0.5f).BorderColor(primary), "Cant.", true);
                    HeaderCell(header.Cell().Border(0.5f).BorderColor(primary), "P. Unit.", true);
                    HeaderCell(header.Cell().Border(0.5f).BorderColor(primary), "Desc.", true);
                    HeaderCell(header.Cell().Border(0.5f).BorderColor(primary), "SubTotal", true);
                    HeaderCell(header.Cell().Border(0.5f).BorderColor(primary), "ISV", true);
                    HeaderCell(header.Cell().Border(0.5f).BorderColor(primary), "Total", true);
                });

                // Filas con colores alternos
                var i = 1;
                foreach (var d in factura.Detalles)
                {
                    var bg = i % 2 == 0 ? secondary : "#FFFFFF";
                    var fs = TextStyle.Default.FontSize(8);

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(3).Text($"{i++}").Style(fs);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(3).Text(d.ProductoCodigo).Style(fs);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(3).Text(d.ProductoDescripcion).Style(fs);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(3).AlignRight().Text($"{d.Cantidad}").Style(fs);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(3).AlignRight().Text($"L {d.PrecioUnitario:N2}").Style(fs);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(3).AlignRight().Text($"L {d.Descuento:N2}").Style(fs);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(3).AlignRight().Text($"L {d.SubTotal:N2}").Style(fs);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(3).AlignRight().Text($"L {d.ISV:N2}").Style(fs);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(3).AlignRight().Text($"L {d.Total:N2}").Style(fs);
                }
            });

            col.Item().Height(10);

            // Totales
            col.Item().AlignRight().Width(300).Column(totales =>
            {
                void LineaTotal(string label, decimal valor, bool bold = false)
                {
                    totales.Item().Row(r =>
                    {
                        var style = bold ? TextStyle.Default.Bold() : TextStyle.Default;
                        r.RelativeItem().AlignRight().Padding(3).Text(label).Style(style);
                        r.ConstantItem(100).AlignRight().Padding(3).Text($"L {valor:N2}").Style(style);
                    });
                }

                LineaTotal("Sub Total:", factura.SubTotal);
                LineaTotal("Descuento:", factura.Descuento);
                LineaTotal("Importe Exento:", factura.ImporteExento);
                if (factura.ImporteExonerado > 0)
                    LineaTotal("Importe Exonerado:", factura.ImporteExonerado);
                LineaTotal("Importe Gravado 15%:", factura.ImporteGravado15);
                LineaTotal("ISV 15%:", factura.ISV15);

                totales.Item().Background(primary).Padding(5).Row(r =>
                {
                    var style = TextStyle.Default.Bold().FontSize(12).FontColor("#FFFFFF");
                    r.RelativeItem().AlignRight().Text("TOTAL:").Style(style);
                    r.ConstantItem(110).AlignRight().Text($"L {factura.Total:N2}").Style(style);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container, FacturaDto factura, string primary)
    {
        container.Column(col =>
        {
            col.Item().Height(2).Background(primary);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Original: Cliente").FontSize(7).Bold().FontColor(primary);
                    c.Item().Text("Copia: Emisor").FontSize(7).Bold().FontColor(primary);
                });
                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text("La factura es beneficio de todos. Exíjala.").FontSize(7).Italic();
                    c.Item().AlignRight().Text(text =>
                    {
                        text.Span("Página ").FontSize(7);
                        text.CurrentPageNumber().FontSize(7);
                        text.Span(" de ").FontSize(7);
                        text.TotalPages().FontSize(7);
                    });
                });
            });
        });
    }
}
