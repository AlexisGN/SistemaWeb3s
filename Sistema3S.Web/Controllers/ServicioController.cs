using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Servicio;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicioController : ControllerBase
    {
        private readonly IServicioService _servicioService;

        public ServicioController(IServicioService servicioService)
        {
            _servicioService = servicioService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? buscar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 5
        )
        {
            try
            {
                var resultado = await _servicioService.ListarAsync(
                    buscar,
                    pagina,
                    tamanioPagina
                );

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
                    mensaje = "No se pudieron cargar los servicios."
                });
            }
        }

        [HttpGet("{idServicio:int}")]
        public async Task<IActionResult> ObtenerPorId(int idServicio)
        {
            try
            {
                var servicio = await _servicioService.ObtenerPorIdAsync(idServicio);

                if (servicio == null)
                {
                    return NotFound(new
                    {
                        mensaje = "Servicio no encontrado."
                    });
                }

                return Ok(servicio);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo obtener el servicio."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ServicioCrearDto dto)
        {
            try
            {
                var servicio = await _servicioService.CrearAsync(dto);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { idServicio = servicio.IdServicio },
                    servicio
                );
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
                    mensaje = "No se pudo registrar el servicio."
                });
            }
        }

        [HttpPut("{idServicio:int}")]
        public async Task<IActionResult> Actualizar(
            int idServicio,
            [FromBody] ServicioActualizarDto dto
        )
        {
            try
            {
                var actualizado = await _servicioService.ActualizarAsync(idServicio, dto);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "Servicio no encontrado."
                    });
                }

                return Ok(new
                {
                    mensaje = "Servicio actualizado correctamente."
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
                    mensaje = "No se pudo actualizar el servicio."
                });
            }
        }

        [HttpDelete("{idServicio:int}")]
        public async Task<IActionResult> Eliminar(int idServicio)
        {
            try
            {
                var eliminado = await _servicioService.EliminarLogicoAsync(idServicio);

                if (!eliminado)
                {
                    return NotFound(new
                    {
                        mensaje = "Servicio no encontrado."
                    });
                }

                return Ok(new
                {
                    mensaje = "Servicio eliminado correctamente."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo eliminar el servicio."
                });
            }
        }

        [HttpGet("total-activos")]
        public async Task<IActionResult> ContarActivos()
        {
            try
            {
                var total = await _servicioService.ContarActivosAsync();

                return Ok(new
                {
                    total
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo obtener el total de servicios activos."
                });
            }
        }
    }
}