using ClosedXML.Excel;
using Sistema3S.Web.DTOs.Compra;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class ExcelCompraService : IExcelCompraService
    {
        public byte[] GenerarExcelReporteCompras(
            IEnumerable<ReporteCompraDto> compras,
            DateTime? fechaInicio,
            DateTime? fechaFin
        )
        {
            var lista = compras.ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte de compras");

            worksheet.Cell("A1").Value = "Sistema 3S";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 18;
            worksheet.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#D81920");

            worksheet.Cell("A2").Value = "Reporte de compras";
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
            worksheet.Cell(filaCabecera, 7).Value = "RUC proveedor";
            worksheet.Cell(filaCabecera, 8).Value = "Proveedor";
            worksheet.Cell(filaCabecera, 9).Value = "Subtotal sin IGV";
            worksheet.Cell(filaCabecera, 10).Value = "IGV";
            worksheet.Cell(filaCabecera, 11).Value = "Total";
            worksheet.Cell(filaCabecera, 12).Value = "Pagado";
            worksheet.Cell(filaCabecera, 13).Value = "Saldo";
            worksheet.Cell(filaCabecera, 14).Value = "Estado compra";
            worksheet.Cell(filaCabecera, 15).Value = "Estado pago";
            worksheet.Cell(filaCabecera, 16).Value = "Guía";

            var headerRange = worksheet.Range(filaCabecera, 1, filaCabecera, 16);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#07111B");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var fila = filaCabecera + 1;

            foreach (var compra in lista)
            {
                worksheet.Cell(fila, 1).Value = compra.IdCompra;
                worksheet.Cell(fila, 2).Value = compra.FechaCompra;
                worksheet.Cell(fila, 3).Value = compra.TipoComprobanteProveedor;
                worksheet.Cell(fila, 4).Value = compra.SerieComprobante;
                worksheet.Cell(fila, 5).Value = compra.NumeroComprobante;
                worksheet.Cell(fila, 6).Value = compra.DocumentoCompleto;
                worksheet.Cell(fila, 7).Value = compra.RucProveedor;
                worksheet.Cell(fila, 8).Value = compra.RazonSocialProveedor;
                worksheet.Cell(fila, 9).Value = compra.Subtotal;
                worksheet.Cell(fila, 10).Value = compra.Igv;
                worksheet.Cell(fila, 11).Value = compra.Total;
                worksheet.Cell(fila, 12).Value = compra.TotalPagado;
                worksheet.Cell(fila, 13).Value = compra.SaldoPendiente;
                worksheet.Cell(fila, 14).Value = compra.EstadoCompra;
                worksheet.Cell(fila, 15).Value = compra.EstadoPago;
                worksheet.Cell(fila, 16).Value = compra.TieneGuia ? "Sí" : "No";

                fila++;
            }

            var ultimaFila = Math.Max(fila - 1, filaCabecera);

            worksheet.Range(filaCabecera, 1, ultimaFila, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(filaCabecera, 1, ultimaFila, 16).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Column(2).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Range(filaCabecera + 1, 9, ultimaFila, 13).Style.NumberFormat.Format = "\"S/\" #,##0.00";

            var filaTotales = ultimaFila + 2;

            worksheet.Cell(filaTotales, 10).Value = "Totales";
            worksheet.Cell(filaTotales, 10).Style.Font.Bold = true;

            worksheet.Cell(filaTotales, 11).Value = lista.Sum(x => x.Total);
            worksheet.Cell(filaTotales, 12).Value = lista.Sum(x => x.TotalPagado);
            worksheet.Cell(filaTotales, 13).Value = lista.Sum(x => x.SaldoPendiente);

            worksheet.Range(filaTotales, 11, filaTotales, 13).Style.NumberFormat.Format = "\"S/\" #,##0.00";
            worksheet.Range(filaTotales, 10, filaTotales, 13).Style.Font.Bold = true;
            worksheet.Range(filaTotales, 10, filaTotales, 13).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");

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

            return "Todas las compras registradas";
        }
    }
}