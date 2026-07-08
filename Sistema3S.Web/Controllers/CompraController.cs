using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Compra;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompraController : ControllerBase
    {
        private readonly ICompraService _compraService;
        private readonly IPdfCompraService _pdfCompraService;
        private readonly IExcelCompraService _excelCompraService;

        public CompraController(
            ICompraService compraService,
            IPdfCompraService pdfCompraService,
            IExcelCompraService excelCompraService
        )
        {
            _compraService = compraService;
            _pdfCompraService = pdfCompraService;
            _excelCompraService = excelCompraService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? buscar,
            [FromQuery] string? estadoPago,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 8
        )
        {
            try
            {
                var resultado = await _compraService.ListarAsync(
                    buscar,
                    estadoPago,
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
                    mensaje = "No se pudieron cargar las compras."
                });
            }
        }

        [HttpGet("{idCompra:int}/detalle")]
        public async Task<IActionResult> ObtenerDetalle(int idCompra)
        {
            try
            {
                var compra = await _compraService.ObtenerDetalleAsync(idCompra);

                if (compra == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No se encontró la compra seleccionada."
                    });
                }

                return Ok(compra);
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
                    mensaje = "No se pudo obtener el detalle de la compra."
                });
            }
        }

        [HttpGet("{idCompra:int}/pdf")]
        public async Task<IActionResult> GenerarPdf(int idCompra)
        {
            try
            {
                var pdfBytes = await _pdfCompraService.GenerarPdfCompraAsync(idCompra);
                var nombreArchivo = $"registro-compra-{idCompra}.pdf";

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
                    mensaje = "No se pudo generar el PDF de la compra."
                });
            }
        }

        [HttpGet("reporte/pdf")]
        public async Task<IActionResult> GenerarReportePdf(
            [FromQuery] string? buscar,
            [FromQuery] string? estadoPago,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin
        )
        {
            try
            {
                var compras = await _compraService.ObtenerReporteAsync(
                    buscar,
                    estadoPago,
                    fechaInicio,
                    fechaFin
                );

                var pdfBytes = _pdfCompraService.GenerarPdfReporteCompras(
                    compras,
                    fechaInicio,
                    fechaFin
                );

                var nombreArchivo = $"reporte-compras-{DateTime.Now:yyyyMMddHHmmss}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo generar el reporte PDF de compras."
                });
            }
        }

        [HttpGet("reporte/excel")]
        public async Task<IActionResult> GenerarReporteExcel(
            [FromQuery] string? buscar,
            [FromQuery] string? estadoPago,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin
        )
        {
            try
            {
                var compras = await _compraService.ObtenerReporteAsync(
                    buscar,
                    estadoPago,
                    fechaInicio,
                    fechaFin
                );

                var excelBytes = _excelCompraService.GenerarExcelReporteCompras(
                    compras,
                    fechaInicio,
                    fechaFin
                );

                var nombreArchivo = $"reporte-compras-{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    nombreArchivo
                );
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo generar el reporte Excel de compras."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] CompraCrearDto dto)
        {
            try
            {
                var resultado = await _compraService.RegistrarCompraCompletaAsync(dto);

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
                    mensaje = "No se pudo registrar la compra."
                });
            }
        }

        [HttpPost("pago")]
        public async Task<IActionResult> RegistrarPago([FromBody] PagoCompraCrearDto dto)
        {
            try
            {
                var resultado = await _compraService.RegistrarPagoAsync(dto);

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
                    mensaje = "No se pudo registrar el pago de la compra."
                });
            }
        }

        [HttpPut("{idCompra:int}/anular")]
        public async Task<IActionResult> Anular(
            int idCompra,
            [FromBody] AnularCompraDto dto
        )
        {
            try
            {
                var resultado = await _compraService.AnularAsync(idCompra, dto);

                if (!resultado)
                {
                    return BadRequest(new
                    {
                        mensaje = "No se pudo anular la compra."
                    });
                }

                return Ok(new
                {
                    mensaje = "Compra anulada correctamente."
                });
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
                    mensaje = "No se pudo anular la compra."
                });
            }
        }
    }
}