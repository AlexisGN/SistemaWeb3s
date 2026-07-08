using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Rol;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolController : ControllerBase
    {
        private readonly IRolService _rolService;

        public RolController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] bool? soloActivos)
        {
            try
            {
                var roles = await _rolService.ListarAsync(soloActivos);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = $"No se pudieron cargar los roles: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] RolCrearDto dto)
        {
            try
            {
                var resultado = await _rolService.CrearAsync(dto);
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
                return StatusCode(500, new
                {
                    mensaje = $"No se pudo crear el rol: {ex.Message}"
                });
            }
        }

        [HttpPut("{idRol:int}")]
        public async Task<IActionResult> Actualizar(int idRol, [FromBody] RolActualizarDto dto)
        {
            try
            {
                var resultado = await _rolService.ActualizarAsync(idRol, dto);
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
                return StatusCode(500, new
                {
                    mensaje = $"No se pudo actualizar el rol: {ex.Message}"
                });
            }
        }

        [HttpPut("{idRol:int}/desactivar")]
        public async Task<IActionResult> Desactivar(int idRol)
        {
            try
            {
                var resultado = await _rolService.DesactivarAsync(idRol);
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
                return StatusCode(500, new
                {
                    mensaje = $"No se pudo desactivar el rol: {ex.Message}"
                });
            }
        }

        [HttpGet("permisos")]
        public async Task<IActionResult> ListarPermisos()
        {
            try
            {
                var permisos = await _rolService.ListarPermisosAsync();
                return Ok(permisos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = $"No se pudieron cargar los permisos: {ex.Message}"
                });
            }
        }

        [HttpGet("{idRol:int}/permisos")]
        public async Task<IActionResult> ObtenerPermisosPorRol(int idRol)
        {
            try
            {
                var permisos = await _rolService.ObtenerPermisosPorRolAsync(idRol);
                return Ok(permisos);
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
                return StatusCode(500, new
                {
                    mensaje = $"No se pudieron cargar los permisos del rol: {ex.Message}"
                });
            }
        }

        [HttpPut("{idRol:int}/permisos")]
        public async Task<IActionResult> AsignarPermisos(
            int idRol,
            [FromBody] RolPermisosActualizarDto dto
        )
        {
            try
            {
                var resultado = await _rolService.AsignarPermisosAsync(idRol, dto);
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
                return StatusCode(500, new
                {
                    mensaje = $"No se pudieron actualizar los permisos del rol: {ex.Message}"
                });
            }
        }
    }
}