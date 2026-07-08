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
            [FromQuery] string? estado,
            [FromQuery] string? origen,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 8
        )
        {
            var resultado = await _cotizacionService.ListarAsync(
                buscar,
                estado,
                origen,
                pagina,
                tamanioPagina
            );

            return Ok(resultado);
        }

        [HttpGet("{idCotizacion:int}")]
        public async Task<IActionResult> ObtenerPorId(int idCotizacion)
        {
            try
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
                    mensaje = "No se pudo obtener la cotización."
                });
            }
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
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{idCotizacion:int}/cancelar")]
        public async Task<IActionResult> Cancelar(
            int idCotizacion,
            [FromBody] CotizacionCambiarEstadoDto? dto
        )
        {
            try
            {
                var cancelada = await _cotizacionService.CancelarAsync(
                    idCotizacion,
                    dto?.IdUsuarioAtencion
                );

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
            catch (Exception ex)
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
                    dto
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPost("{idCotizacion:int}/enviar-correo")]
        public async Task<IActionResult> EnviarCorreo(
            int idCotizacion,
            [FromBody] CotizacionEnviarDto? dto
        )
        {
            try
            {
                var enviado = await _cotizacionService.EnviarCorreoAsync(
                    idCotizacion,
                    dto?.IdUsuarioAtencion
                );

                if (!enviado)
                {
                    return NotFound(new
                    {
                        mensaje = "Cotización no encontrada."
                    });
                }

                return Ok(new
                {
                    mensaje = "Cotización enviada por correo correctamente."
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
        [HttpPost("{idCotizacion:int}/enviar-whatsapp")]
        public async Task<IActionResult> EnviarWhatsApp(
    int idCotizacion,
    [FromBody] CotizacionEnviarDto? dto
)
        {
            try
            {
                var resultado = await _cotizacionService.EnviarWhatsAppAsync(
                    idCotizacion,
                    dto?.IdUsuarioAtencion
                );

                return Ok(new
                {
                    mensaje = resultado.RequiereConfirmacionRespondida
                        ? "Mensaje de WhatsApp preparado correctamente. Adjunta el PDF manualmente y confirma el envío."
                        : "Cotización enviada por WhatsApp correctamente.",
                    resultado
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
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpPut("{idCotizacion:int}/marcar-respondida")]
        public async Task<IActionResult> MarcarRespondida(
            int idCotizacion,
            [FromBody] CotizacionMarcarRespondidaDto dto
        )
        {
            try
            {
                var actualizado = await _cotizacionService.MarcarRespondidaAsync(
                    idCotizacion,
                    dto
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
                    mensaje = "Cotización marcada como respondida correctamente."
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

        [HttpGet("{idCotizacion:int}/preparar-venta")]
        public async Task<IActionResult> PrepararParaVenta(int idCotizacion)
        {
            try
            {
                var cotizacion = await _cotizacionService.PrepararParaVentaAsync(idCotizacion);

                return Ok(cotizacion);
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

        [HttpPut("{idCotizacion:int}/marcar-convertida-venta")]
        public async Task<IActionResult> MarcarConvertidaVenta(
            int idCotizacion,
            [FromBody] CotizacionConvertirVentaDto dto
        )
        {
            try
            {
                var actualizado = await _cotizacionService.MarcarConvertidaVentaAsync(
                    idCotizacion,
                    dto
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
                    mensaje = "Cotización marcada como convertida en venta correctamente."
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