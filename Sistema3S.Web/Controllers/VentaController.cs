using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Venta;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentaController : ControllerBase
    {
        private readonly IVentaService _ventaService;
        private readonly IPdfVentaService _pdfVentaService;
        private readonly IExcelVentaService _excelVentaService;

        public VentaController(
            IVentaService ventaService,
            IPdfVentaService pdfVentaService,
            IExcelVentaService excelVentaService
        )
        {
            _ventaService = ventaService;
            _pdfVentaService = pdfVentaService;
            _excelVentaService = excelVentaService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? buscar,
            [FromQuery] string? estadoPago,
            [FromQuery] string? tipoComprobante,
            [FromQuery] string? origenVenta,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 8
        )
        {
            try
            {
                var resultado = await _ventaService.ListarAsync(
                    buscar,
                    estadoPago,
                    tipoComprobante,
                    origenVenta,
                    fechaInicio,
                    fechaFin,
                    pagina,
                    tamanioPagina
                );

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudieron cargar las ventas."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] VentaCrearDto dto)
        {
            try
            {
                var resultado = await _ventaService.RegistrarVentaCompletaAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }
        [HttpGet("siguiente-comprobante")]
        public async Task<IActionResult> ObtenerSiguienteComprobante([FromQuery] string tipoComprobante)
        {
            try
            {
                var resultado = await _ventaService.ObtenerSiguienteComprobanteAsync(tipoComprobante);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo obtener el siguiente número de comprobante."
                });
            }
        }
        [HttpGet("{idVenta:int}/detalle")]
        public async Task<IActionResult> ObtenerDetalle(int idVenta)
        {
            try
            {
                var venta = await _ventaService.ObtenerDetalleAsync(idVenta);

                if (venta == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No se encontró la venta seleccionada."
                    });
                }

                return Ok(venta);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo obtener el detalle de la venta."
                });
            }
        }

        [HttpPost("pago")]
        public async Task<IActionResult> RegistrarPago([FromBody] PagoVentaCrearDto dto)
        {
            try
            {
                var resultado = await _ventaService.RegistrarPagoAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{idVenta:int}/anular")]
        public async Task<IActionResult> Anular(int idVenta, [FromBody] AnularVentaDto dto)
        {
            try
            {
                var resultado = await _ventaService.AnularAsync(idVenta, dto);

                if (!resultado)
                {
                    return BadRequest(new
                    {
                        mensaje = "No se pudo anular la venta."
                    });
                }

                return Ok(new
                {
                    mensaje = "Venta anulada correctamente."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("{idVenta:int}/pdf")]
        public async Task<IActionResult> GenerarPdfVenta(int idVenta)
        {
            try
            {
                var pdfBytes = await _pdfVentaService.GenerarPdfVentaAsync(idVenta);
                var nombreArchivo = $"venta-{idVenta}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo generar el PDF de la venta."
                });
            }
        }

        [HttpGet("reporte/pdf")]
        public async Task<IActionResult> GenerarReportePdf(
            [FromQuery] string? buscar,
            [FromQuery] string? estadoPago,
            [FromQuery] string? tipoComprobante,
            [FromQuery] string? origenVenta,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin
        )
        {
            try
            {
                var ventas = await _ventaService.ObtenerReporteAsync(
                    buscar,
                    estadoPago,
                    tipoComprobante,
                    origenVenta,
                    fechaInicio,
                    fechaFin
                );

                var pdfBytes = _pdfVentaService.GenerarPdfReporteVentas(ventas, fechaInicio, fechaFin);

                return File(pdfBytes, "application/pdf", "reporte-ventas.pdf");
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo generar el reporte PDF de ventas."
                });
            }
        }

        [HttpGet("reporte/excel")]
        public async Task<IActionResult> GenerarReporteExcel(
            [FromQuery] string? buscar,
            [FromQuery] string? estadoPago,
            [FromQuery] string? tipoComprobante,
            [FromQuery] string? origenVenta,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin
        )
        {
            try
            {
                var ventas = await _ventaService.ObtenerReporteAsync(
                    buscar,
                    estadoPago,
                    tipoComprobante,
                    origenVenta,
                    fechaInicio,
                    fechaFin
                );

                var excelBytes = _excelVentaService.GenerarExcelReporteVentas(ventas, fechaInicio, fechaFin);

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "reporte-ventas.xlsx"
                );
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo generar el reporte Excel de ventas."
                });
            }
        }
    }
}