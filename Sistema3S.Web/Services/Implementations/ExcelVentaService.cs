using ClosedXML.Excel;
using Sistema3S.Web.DTOs.Venta;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class ExcelVentaService : IExcelVentaService
    {
        public byte[] GenerarExcelReporteVentas(
            IEnumerable<ReporteVentaDto> ventas,
            DateTime? fechaInicio,
            DateTime? fechaFin
        )
        {
            var lista = ventas.ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte de ventas");

            worksheet.Cell("A1").Value = "Sistema 3S";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 18;
            worksheet.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#D81920");

            worksheet.Cell("A2").Value = "Reporte de ventas";
            worksheet.Cell("A2").Style.Font.Bold = true;
            worksheet.Cell("A2").Style.Font.FontSize = 14;

            worksheet.Cell("A3").Value = ObtenerTextoRango(fechaInicio, fechaFin);
            worksheet.Cell("A4").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            var filaCabecera = 6;

            worksheet.Cell(filaCabecera, 1).Value = "ID";
            worksheet.Cell(filaCabecera, 2).Value = "Fecha";
            worksheet.Cell(filaCabecera, 3).Value = "Tipo comprobante";
            worksheet.Cell(filaCabecera, 4).Value = "Serie";
            worksheet.Cell(filaCabecera, 5).Value = "Número";
            worksheet.Cell(filaCabecera, 6).Value = "Documento";
            worksheet.Cell(filaCabecera, 7).Value = "Doc. cliente";
            worksheet.Cell(filaCabecera, 8).Value = "Cliente";
            worksheet.Cell(filaCabecera, 9).Value = "Origen";
            worksheet.Cell(filaCabecera, 10).Value = "Subtotal sin IGV";
            worksheet.Cell(filaCabecera, 11).Value = "IGV";
            worksheet.Cell(filaCabecera, 12).Value = "Total";
            worksheet.Cell(filaCabecera, 13).Value = "Pagado";
            worksheet.Cell(filaCabecera, 14).Value = "Saldo";
            worksheet.Cell(filaCabecera, 15).Value = "Estado venta";
            worksheet.Cell(filaCabecera, 16).Value = "Estado pago";

            var headerRange = worksheet.Range(filaCabecera, 1, filaCabecera, 16);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#07111B");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            var fila = filaCabecera + 1;

            foreach (var venta in lista)
            {
                worksheet.Cell(fila, 1).Value = venta.IdVenta;
                worksheet.Cell(fila, 2).Value = venta.FechaVenta;
                worksheet.Cell(fila, 3).Value = venta.TipoComprobante;
                worksheet.Cell(fila, 4).Value = venta.Serie;
                worksheet.Cell(fila, 5).Value = venta.Numero;
                worksheet.Cell(fila, 6).Value = venta.DocumentoCompleto;
                worksheet.Cell(fila, 7).Value = venta.DocumentoCliente;
                worksheet.Cell(fila, 8).Value = venta.Cliente;
                worksheet.Cell(fila, 9).Value = venta.OrigenVenta;
                worksheet.Cell(fila, 10).Value = venta.Subtotal;
                worksheet.Cell(fila, 11).Value = venta.Igv;
                worksheet.Cell(fila, 12).Value = venta.Total;
                worksheet.Cell(fila, 13).Value = venta.TotalPagado;
                worksheet.Cell(fila, 14).Value = venta.SaldoPendiente;
                worksheet.Cell(fila, 15).Value = venta.EstadoVenta;
                worksheet.Cell(fila, 16).Value = venta.EstadoPago;

                fila++;
            }

            var ultimaFila = Math.Max(fila - 1, filaCabecera);

            var dataRange = worksheet.Range(filaCabecera, 1, ultimaFila, 16);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            worksheet.Column(2).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Range(filaCabecera + 1, 10, ultimaFila, 14)
                .Style.NumberFormat.Format = "\"S/\" #,##0.00";

            var filaTotales = ultimaFila + 2;

            worksheet.Cell(filaTotales, 11).Value = "Totales";
            worksheet.Cell(filaTotales, 11).Style.Font.Bold = true;

            worksheet.Cell(filaTotales, 12).Value = lista.Sum(x => x.Total);
            worksheet.Cell(filaTotales, 13).Value = lista.Sum(x => x.TotalPagado);
            worksheet.Cell(filaTotales, 14).Value = lista.Sum(x => x.SaldoPendiente);

            var totalRange = worksheet.Range(filaTotales, 11, filaTotales, 14);
            totalRange.Style.Font.Bold = true;
            totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
            totalRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            totalRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range(filaTotales, 12, filaTotales, 14)
                .Style.NumberFormat.Format = "\"S/\" #,##0.00";

            worksheet.SheetView.FreezeRows(filaCabecera);
            worksheet.Range(filaCabecera, 1, ultimaFila, 16).SetAutoFilter();

            worksheet.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);

            return memoryStream.ToArray();
        }

        private static string ObtenerTextoRango(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                return $"Del {fechaInicio.Value:dd/MM/yyyy} al {fechaFin.Value:dd/MM/yyyy}";
            }

            if (fechaInicio.HasValue)
            {
                return $"Desde {fechaInicio.Value:dd/MM/yyyy}";
            }

            if (fechaFin.HasValue)
            {
                return $"Hasta {fechaFin.Value:dd/MM/yyyy}";
            }

            return "Todas las ventas registradas";
        }
    }
}