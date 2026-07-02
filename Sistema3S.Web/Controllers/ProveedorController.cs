using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Proveedor;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;

        public ProveedorController(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
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
                var resultado = await _proveedorService.ListarAsync(
                    buscar,
                    pagina,
                    tamanioPagina
                );

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudieron cargar los proveedores."
                });
            }
        }

        [HttpGet("{idProveedor:int}")]
        public async Task<IActionResult> ObtenerPorId(int idProveedor)
        {
            try
            {
                var proveedor = await _proveedorService.ObtenerPorIdAsync(idProveedor);

                if (proveedor == null)
                {
                    return NotFound(new
                    {
                        mensaje = "Proveedor no encontrado."
                    });
                }

                return Ok(proveedor);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo obtener el proveedor."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ProveedorCrearDto dto)
        {
            try
            {
                var proveedor = await _proveedorService.CrearAsync(dto);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { idProveedor = proveedor.IdProveedor },
                    proveedor
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
                    mensaje = "No se pudo registrar el proveedor."
                });
            }
        }

        [HttpPut("{idProveedor:int}")]
        public async Task<IActionResult> Actualizar(
            int idProveedor,
            [FromBody] ProveedorActualizarDto dto
        )
        {
            try
            {
                var actualizado = await _proveedorService.ActualizarAsync(idProveedor, dto);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "Proveedor no encontrado."
                    });
                }

                return Ok(new
                {
                    mensaje = "Proveedor actualizado correctamente."
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
                    mensaje = "No se pudo actualizar el proveedor."
                });
            }
        }

        [HttpDelete("{idProveedor:int}")]
        public async Task<IActionResult> EliminarLogico(int idProveedor)
        {
            try
            {
                var eliminado = await _proveedorService.EliminarLogicoAsync(idProveedor);

                if (!eliminado)
                {
                    return NotFound(new
                    {
                        mensaje = "Proveedor no encontrado."
                    });
                }

                return Ok(new
                {
                    mensaje = "Proveedor eliminado correctamente."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo eliminar el proveedor."
                });
            }
        }

        [HttpGet("consultar-ruc/{ruc}")]
        public async Task<IActionResult> ConsultarRuc(string ruc)
        {
            var resultado = await _proveedorService.ConsultarRucAsync(ruc);

            if (!resultado.Exitoso && !resultado.ProveedorYaExiste)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        [HttpGet("total-activos")]
        public async Task<IActionResult> ContarActivos()
        {
            try
            {
                var total = await _proveedorService.ContarActivosAsync();

                return Ok(new
                {
                    total
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo obtener el total de proveedores activos."
                });
            }
        }
    }
}