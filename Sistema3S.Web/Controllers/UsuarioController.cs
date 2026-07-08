using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Usuario;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? buscar,
            [FromQuery] int? idRol,
            [FromQuery] bool? estado
        )
        {
            try
            {
                var usuarios = await _usuarioService.ListarAsync(
                    buscar,
                    idRol,
                    estado
                );

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = $"No se pudieron cargar los usuarios: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] UsuarioCrearDto dto)
        {
            try
            {
                var resultado = await _usuarioService.CrearAsync(dto);
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
                    mensaje = $"No se pudo crear el usuario: {ex.Message}"
                });
            }
        }

        [HttpPut("{idUsuario:int}")]
        public async Task<IActionResult> Actualizar(
            int idUsuario,
            [FromBody] UsuarioActualizarDto dto
        )
        {
            try
            {
                var resultado = await _usuarioService.ActualizarAsync(idUsuario, dto);
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
                    mensaje = $"No se pudo actualizar el usuario: {ex.Message}"
                });
            }
        }

        [HttpPut("{idUsuario:int}/contrasena")]
        public async Task<IActionResult> CambiarContrasena(
            int idUsuario,
            [FromBody] UsuarioCambiarContrasenaDto dto
        )
        {
            try
            {
                var resultado = await _usuarioService.CambiarContrasenaAsync(idUsuario, dto);
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
                    mensaje = $"No se pudo actualizar la contraseña: {ex.Message}"
                });
            }
        }

        [HttpPut("{idUsuario:int}/desactivar")]
        public async Task<IActionResult> Desactivar(int idUsuario)
        {
            try
            {
                var resultado = await _usuarioService.DesactivarAsync(idUsuario);
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
                    mensaje = $"No se pudo desactivar el usuario: {ex.Message}"
                });
            }
        }

        [HttpPut("{idUsuario:int}/activar")]
        public async Task<IActionResult> Activar(int idUsuario)
        {
            try
            {
                var resultado = await _usuarioService.ActivarAsync(idUsuario);
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
                    mensaje = $"No se pudo activar el usuario: {ex.Message}"
                });
            }
        }
    }
}