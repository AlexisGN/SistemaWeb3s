using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Producto;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductoController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? buscar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 10
)
        {
            var resultado = await _productoService.ListarAsync(
                buscar,
                pagina,
                tamanioPagina
            );

            return Ok(resultado);
        }

        [HttpGet("{idProducto:int}")]
        public async Task<IActionResult> ObtenerPorId(int idProducto)
        {
            var producto = await _productoService.ObtenerPorIdAsync(idProducto);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = "Producto no encontrado."
                });
            }

            return Ok(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ProductoCrearDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new { mensaje = "El nombre del producto es obligatorio." });
                }

                if (string.IsNullOrWhiteSpace(dto.CodigoProducto))
                {
                    return BadRequest(new { mensaje = "El código del producto es obligatorio." });
                }

                if (dto.StockInicial < 0)
                {
                    return BadRequest(new { mensaje = "El stock inicial no puede ser negativo." });
                }

                if (dto.StockMinimo < 0)
                {
                    return BadRequest(new { mensaje = "El stock mínimo no puede ser negativo." });
                }
                if (dto.StockInicial <= dto.StockMinimo)
                {
                    return BadRequest(new
                    {
                        mensaje = "El stock actual inicial debe ser mayor que el stock mínimo."
                    });
                }
                var producto = await _productoService.CrearAsync(dto);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { idProducto = producto.IdProducto },
                    producto
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

        [HttpPut("{idProducto:int}")]
        public async Task<IActionResult> Actualizar(
            int idProducto,
            [FromBody] ProductoActualizarDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                {
                    return BadRequest(new { mensaje = "El nombre del producto es obligatorio." });
                }

                if (string.IsNullOrWhiteSpace(dto.CodigoProducto))
                {
                    return BadRequest(new { mensaje = "El código del producto es obligatorio." });
                }

                if (dto.StockMinimo < 0)
                {
                    return BadRequest(new { mensaje = "El stock mínimo no puede ser negativo." });
                }

                var actualizado = await _productoService.ActualizarAsync(idProducto, dto);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "Producto no encontrado."
                    });
                }

                return Ok(new
                {
                    mensaje = "Producto actualizado correctamente."
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

        [HttpDelete("{idProducto:int}")]
        public async Task<IActionResult> EliminarLogico(int idProducto)
        {
            var eliminado = await _productoService.EliminarLogicoAsync(idProducto);

            if (!eliminado)
            {
                return NotFound(new
                {
                    mensaje = "Producto no encontrado."
                });
            }

            return Ok(new
            {
                mensaje = "Producto eliminado correctamente."
            });
        }

        [HttpGet("total-activos")]
        public async Task<IActionResult> ContarActivos()
        {
            var total = await _productoService.ContarActivosAsync();

            return Ok(new
            {
                total
            });
        }
    }
}