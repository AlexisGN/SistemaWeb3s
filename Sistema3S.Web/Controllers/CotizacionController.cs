using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Cotizacion;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionController : ControllerBase
    {
        private readonly ICotizacionService _cotizacionService;

        public CotizacionController(ICotizacionService cotizacionService)
        {
            _cotizacionService = cotizacionService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? buscar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 5
        )
        {
            var resultado = await _cotizacionService.ListarAsync(
                buscar,
                pagina,
                tamanioPagina
            );

            return Ok(resultado);
        }

        [HttpGet("{idCotizacion:int}")]
        public async Task<IActionResult> ObtenerPorId(int idCotizacion)
        {
            var cotizacion = await _cotizacionService.ObtenerPorIdAsync(idCotizacion);

            if (cotizacion == null)
            {
                return NotFound(new
                {
                    mensaje = "Cotización no encontrada."
                });
            }

            return Ok(cotizacion);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CotizacionCrearDto dto)
        {
            try
            {
                var cotizacion = await _cotizacionService.CrearAsync(dto);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { idCotizacion = cotizacion.IdCotizacion },
                    cotizacion
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{idCotizacion:int}")]
        public async Task<IActionResult> Actualizar(
            int idCotizacion,
            [FromBody] CotizacionActualizarDto dto
        )
        {
            try
            {
                var actualizado = await _cotizacionService.ActualizarAsync(idCotizacion, dto);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "Cotización no encontrada."
                    });
                }

                return Ok(new
                {
                    mensaje = "Cotización actualizada correctamente."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{idCotizacion:int}/cancelar")]
        public async Task<IActionResult> Cancelar(int idCotizacion)
        {
            try
            {
                var cancelada = await _cotizacionService.CancelarAsync(idCotizacion);

                if (!cancelada)
                {
                    return NotFound(new
                    {
                        mensaje = "Cotización no encontrada."
                    });
                }

                return Ok(new
                {
                    mensaje = "Cotización cancelada correctamente."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{idCotizacion:int}/estado")]
        public async Task<IActionResult> CambiarEstado(
            int idCotizacion,
            [FromBody] CotizacionCambiarEstadoDto dto
        )
        {
            try
            {
                var actualizado = await _cotizacionService.CambiarEstadoAsync(
                    idCotizacion,
                    dto.IdEstadoCotizacion
                );

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "Cotización no encontrada."
                    });
                }

                return Ok(new
                {
                    mensaje = "Estado de cotización actualizado correctamente."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPost("{idCotizacion:int}/generar-pdf")]
        public async Task<IActionResult> GenerarPdf(int idCotizacion)
        {
            try
            {
                var archivoPdf = await _cotizacionService.GenerarPdfAsync(idCotizacion);

                return Ok(new
                {
                    mensaje = "PDF generado correctamente.",
                    archivoPdf
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{idCotizacion:int}/marcar-correo-enviado")]
        public async Task<IActionResult> MarcarCorreoEnviado(int idCotizacion)
        {
            try
            {
                var actualizado = await _cotizacionService.MarcarCorreoEnviadoAsync(idCotizacion);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "Cotización no encontrada."
                    });
                }

                return Ok(new
                {
                    mensaje = "Cotización marcada como enviada por correo."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("{idCotizacion:int}/whatsapp")]
        public async Task<IActionResult> ObtenerWhatsApp(int idCotizacion)
        {
            try
            {
                var resultado = await _cotizacionService.ObtenerWhatsAppAsync(idCotizacion);

                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPost("{idCotizacion:int}/convertir-venta")]
        public async Task<IActionResult> ConvertirEnVenta(
            int idCotizacion,
            [FromBody] CotizacionConvertirVentaDto dto
        )
        {
            try
            {
                var idVenta = await _cotizacionService.ConvertirEnVentaAsync(
                    idCotizacion,
                    dto.IdUsuarioRegistro
                );

                return Ok(new
                {
                    mensaje = "Cotización convertida en venta correctamente.",
                    idVenta
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("total-pendientes")]
        public async Task<IActionResult> ContarPendientes()
        {
            var total = await _cotizacionService.ContarPendientesAsync();

            return Ok(new
            {
                total
            });
        }

        [HttpGet("clientes")]
        public async Task<IActionResult> ListarClientes()
        {
            var clientes = await _cotizacionService.ListarClientesAsync();

            return Ok(clientes);
        }

        [HttpGet("elementos-cotizables")]
        public async Task<IActionResult> ListarElementosCotizables()
        {
            var elementos = await _cotizacionService.ListarElementosCotizablesAsync();

            return Ok(elementos);
        }

        [HttpGet("estados")]
        public async Task<IActionResult> ListarEstados()
        {
            var estados = await _cotizacionService.ListarEstadosAsync();

            return Ok(estados);
        }
    }
}