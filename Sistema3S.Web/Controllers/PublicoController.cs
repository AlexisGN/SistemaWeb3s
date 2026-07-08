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
    }
}