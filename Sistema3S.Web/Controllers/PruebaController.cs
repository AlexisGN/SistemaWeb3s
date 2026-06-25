using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.Models;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PruebaController : ControllerBase
    {
        private readonly Bd3sContext _context;

        public PruebaController(Bd3sContext context)
        {
            _context = context;
        }

        [HttpGet("conexion")]
        public async Task<IActionResult> Conexion()
        {
            try
            {
                var puedeConectar = await _context.Database.CanConnectAsync();

                if (!puedeConectar)
                {
                    return StatusCode(503, new
                    {
                        estado = "inactivo",
                        mensaje = "No se pudo conectar con la base de datos."
                    });
                }

                return Ok(new
                {
                    estado = "activo",
                    mensaje = "Backend y base de datos conectados correctamente."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    estado = "inactivo",
                    mensaje = "Error de conexión.",
                    detalle = ex.Message
                });
            }
        }
    }
}