using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Inventario;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioService _inventarioService;

        public InventarioController(IInventarioService inventarioService)
        {
            _inventarioService = inventarioService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? buscar,
            [FromQuery] string? estadoStock,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 6
        )
        {
            try
            {
                var resultado = await _inventarioService.ListarAsync(
                    buscar,
                    estadoStock,
                    pagina,
                    tamanioPagina
                );

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo cargar el inventario."
                });
            }
        }

        [HttpGet("{idProducto:int}")]
        public async Task<IActionResult> ObtenerPorProducto(int idProducto)
        {
            try
            {
                var inventario = await _inventarioService.ObtenerPorProductoAsync(idProducto);

                if (inventario == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No se encontró inventario para el producto seleccionado."
                    });
                }

                return Ok(inventario);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo obtener el inventario del producto."
                });
            }
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen()
        {
            try
            {
                var resumen = await _inventarioService.ObtenerResumenAsync();

                return Ok(resumen);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo obtener el resumen de inventario."
                });
            }
        }

        [HttpGet("{idProducto:int}/movimientos")]
        public async Task<IActionResult> ListarMovimientosPorProducto(int idProducto)
        {
            try
            {
                var movimientos = await _inventarioService.ListarMovimientosPorProductoAsync(idProducto);

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
                    mensaje = "No se pudieron cargar los movimientos del producto."
                });
            }
        }

        [HttpGet("movimientos-recientes")]
        public async Task<IActionResult> ListarMovimientosRecientes([FromQuery] int cantidad = 5)
        {
            try
            {
                var movimientos = await _inventarioService.ListarMovimientosRecientesAsync(cantidad);

                return Ok(movimientos);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudieron cargar los movimientos recientes."
                });
            }
        }

        [HttpPut("{idProducto:int}/stock-minimo")]
        public async Task<IActionResult> ActualizarStockMinimo(
            int idProducto,
            [FromBody] ActualizarStockMinimoDto dto
        )
        {
            try
            {
                var inventario = await _inventarioService.ActualizarStockMinimoAsync(
                    idProducto,
                    dto
                );

                return Ok(inventario);
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
                    mensaje = "No se pudo actualizar el stock mínimo."
                });
            }
        }

        [HttpPost("movimiento-manual")]
        public async Task<IActionResult> RegistrarMovimientoManual(
            [FromBody] RegistrarMovimientoStockDto dto
        )
        {
            try
            {
                var inventario = await _inventarioService.RegistrarMovimientoManualAsync(dto);

                return Ok(inventario);
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
                    mensaje = "No se pudo registrar el movimiento de stock."
                });
            }
        }
    }
}