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

        public CompraController(ICompraService compraService)
        {
            _compraService = compraService;
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