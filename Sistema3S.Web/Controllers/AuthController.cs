using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Auth;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var resultado = await _authService.LoginAsync(dto);

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
                    mensaje = "No se pudo iniciar sesión."
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("cambiar-contrasena-inicial")]
        public async Task<IActionResult> CambiarContrasenaInicial(
            [FromBody] CambiarContrasenaInicialDto dto
        )
        {
            try
            {
                var resultado = await _authService.CambiarContrasenaInicialAsync(dto);

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
                    mensaje = "No se pudo actualizar la contraseña inicial."
                });
            }
        }

        [Authorize]
        [HttpGet("perfil")]
        public IActionResult Perfil()
        {
            var idUsuario = User.FindFirst("idUsuario")?.Value;
            var correo = User.FindFirst("correo")?.Value;
            var rol = User.FindFirst("rol")?.Value;
            var permisos = User.FindAll("permiso").Select(c => c.Value).ToList();

            return Ok(new
            {
                idUsuario,
                correo,
                rol,
                permisos
            });
        }
    }
}