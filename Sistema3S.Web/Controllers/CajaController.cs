using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Caja;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CajaController : ControllerBase
    {
        private readonly ICajaService _cajaService;

        public CajaController(ICajaService cajaService)
        {
            _cajaService = cajaService;
        }

        [HttpGet("activa")]
        public async Task<IActionResult> ObtenerCajaActiva([FromQuery] int idUsuario = 1)
        {
            try
            {
                var caja = await _cajaService.ObtenerCajaActivaAsync(idUsuario);

                return Ok(caja);
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
                    mensaje = "No se pudo obtener la caja activa."
                });
            }
        }

        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja([FromBody] CajaAbrirDto dto)
        {
            try
            {
                var caja = await _cajaService.AbrirCajaAsync(dto);

                return Ok(caja);
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
                    mensaje = "No se pudo abrir la caja."
                });
            }
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen(
            [FromQuery] int idUsuario = 1,
            [FromQuery] int? idCaja = null
        )
        {
            try
            {
                var resumen = await _cajaService.ObtenerResumenAsync(idUsuario, idCaja);

                return Ok(resumen);
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
                    mensaje = "No se pudo obtener el resumen de caja."
                });
            }
        }

        [HttpGet("movimientos")]
        public async Task<IActionResult> ListarMovimientos(
            [FromQuery] int idUsuario = 1,
            [FromQuery] int? idCaja = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null
        )
        {
            try
            {
                var movimientos = await _cajaService.ListarMovimientosAsync(
                    idUsuario,
                    idCaja,
                    fechaInicio,
                    fechaFin
                );

                return Ok(movimientos);
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
                    mensaje = "No se pudieron cargar los movimientos de caja."
                });
            }
        }

        [HttpPost("movimiento-manual")]
        public async Task<IActionResult> RegistrarMovimientoManual(
            [FromBody] MovimientoCajaManualDto dto
        )
        {
            try
            {
                var resultado = await _cajaService.RegistrarMovimientoManualAsync(dto);

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
                    mensaje = "No se pudo registrar el movimiento de caja."
                });
            }
        }

        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarCaja([FromBody] CajaCerrarDto dto)
        {
            try
            {
                var caja = await _cajaService.CerrarCajaAsync(dto);

                return Ok(caja);
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
                    mensaje = "No se pudo cerrar la caja."
                });
            }
        }

        [HttpGet("reporte")]
        public async Task<IActionResult> ObtenerReporte(
            [FromQuery] int idUsuario = 1,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null
        )
        {
            try
            {
                var reporte = await _cajaService.ObtenerReporteAsync(
                    idUsuario,
                    fechaInicio,
                    fechaFin
                );

                return Ok(reporte);
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
                    mensaje = "No se pudo obtener el reporte de caja."
                });
            }
        }
    }
}