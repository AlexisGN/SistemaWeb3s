using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/publico")]
    public class PublicoController : ControllerBase
    {
        private readonly IPublicoService _publicoService;

        public PublicoController(IPublicoService publicoService)
        {
            _publicoService = publicoService;
        }

        [HttpGet("inicio")]
        public async Task<IActionResult> ObtenerInicio()
        {
            try
            {
                var resultado = await _publicoService.ObtenerInicioAsync();
                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo cargar la información pública de inicio."
                });
            }
        }

        [HttpGet("productos")]
        public async Task<IActionResult> ObtenerProductos(
            [FromQuery] string? q,
            [FromQuery] int? idCategoria,
            [FromQuery] int? idMarca,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 24)
        {
            try
            {
                var resultado = await _publicoService.ObtenerProductosAsync(
                    q,
                    idCategoria,
                    idMarca,
                    pagina,
                    tamanioPagina
                );

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo cargar el catálogo público de productos."
                });
            }
        }

        [HttpGet("productos/{id:int}")]
        public async Task<IActionResult> ObtenerProductoDetalle(int id)
        {
            try
            {
                var resultado = await _publicoService.ObtenerProductoDetalleAsync(id);

                if (resultado == null)
                {
                    return NotFound(new
                    {
                        mensaje = "El producto solicitado no existe o no está disponible para la web pública."
                    });
                }

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo cargar el detalle del producto."
                });
            }
        }

        [HttpGet("categorias")]
        public async Task<IActionResult> ObtenerCategorias()
        {
            try
            {
                var resultado = await _publicoService.ObtenerCategoriasPublicasAsync();
                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo cargar la lista de categorías públicas."
                });
            }
        }

        [HttpGet("marcas")]
        public async Task<IActionResult> ObtenerMarcas()
        {
            try
            {
                var resultado = await _publicoService.ObtenerMarcasPublicasAsync();
                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo cargar la lista de marcas públicas."
                });
            }
        }

        [HttpGet("servicios")]
        public async Task<IActionResult> ObtenerServicios()
        {
            try
            {
                var resultado = await _publicoService.ObtenerServiciosPublicosAsync();
                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo cargar la lista de servicios públicos."
                });
            }
        }

        [HttpGet("servicios/{id:int}")]
        public async Task<IActionResult> ObtenerServicioDetalle(int id)
        {
            try
            {
                var resultado = await _publicoService.ObtenerServicioDetalleAsync(id);

                if (resultado == null)
                {
                    return NotFound(new
                    {
                        mensaje = "El servicio solicitado no existe o no está disponible para la web pública."
                    });
                }

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo cargar el detalle del servicio."
                });
            }
        }
    }
}