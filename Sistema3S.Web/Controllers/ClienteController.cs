using Microsoft.AspNetCore.Mvc;
using Sistema3S.Web.DTOs.Cliente;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly IConsultaDocumentoService _consultaDocumentoService;
        public ClienteController(
        IClienteService clienteService,
        IConsultaDocumentoService consultaDocumentoService
)
        {
            _clienteService = clienteService;
            _consultaDocumentoService = consultaDocumentoService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? buscar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanioPagina = 5
        )
        {
            var resultado = await _clienteService.ListarAsync(buscar, pagina, tamanioPagina);

            return Ok(resultado);
        }

        [HttpGet("{idCliente:int}")]
        public async Task<IActionResult> ObtenerPorId(int idCliente)
        {
            var cliente = await _clienteService.ObtenerPorIdAsync(idCliente);

            if (cliente == null)
            {
                return NotFound(new
                {
                    mensaje = "Cliente no encontrado."
                });
            }

            return Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ClienteCrearDto dto)
        {
            try
            {
                var cliente = await _clienteService.CrearAsync(dto);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { idCliente = cliente.IdCliente },
                    cliente
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

        [HttpPut("{idCliente:int}")]
        public async Task<IActionResult> Actualizar(
            int idCliente,
            [FromBody] ClienteActualizarDto dto
        )
        {
            try
            {
                var actualizado = await _clienteService.ActualizarAsync(idCliente, dto);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "Cliente no encontrado."
                    });
                }

                return Ok(new
                {
                    mensaje = "Cliente actualizado correctamente."
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

        [HttpDelete("{idCliente:int}")]
        public async Task<IActionResult> Eliminar(int idCliente)
        {
            var eliminado = await _clienteService.EliminarLogicoAsync(idCliente);

            if (!eliminado)
            {
                return NotFound(new
                {
                    mensaje = "Cliente no encontrado."
                });
            }

            return Ok(new
            {
                mensaje = "Cliente desactivado correctamente."
            });
        }

        [HttpGet("total-activos")]
        public async Task<IActionResult> ContarActivos()
        {
            var total = await _clienteService.ContarActivosAsync();

            return Ok(new
            {
                total
            });
        }

        [HttpGet("tipos-cliente")]
        public async Task<IActionResult> ListarTiposCliente()
        {
            var tipos = await _clienteService.ListarTiposClienteAsync();

            return Ok(tipos);
        }

        [HttpGet("tipos-documento")]
        public async Task<IActionResult> ListarTiposDocumento()
        {
            var tipos = await _clienteService.ListarTiposDocumentoAsync();

            return Ok(tipos);
        }

        [HttpGet("ubigeos")]
        public async Task<IActionResult> ListarUbigeos([FromQuery] string? buscar)
        {
            var ubigeos = await _clienteService.ListarUbigeosAsync(buscar);

            return Ok(ubigeos);
        }
        [HttpGet("consultar-dni/{dni}")]
        public async Task<IActionResult> ConsultarDni(string dni)
        {
            var resultado = await _consultaDocumentoService.ConsultarDniAsync(dni);

            if (!resultado.Exitoso && !resultado.ClienteYaExiste)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        [HttpGet("consultar-ruc/{ruc}")]
        public async Task<IActionResult> ConsultarRuc(string ruc)
        {
            var resultado = await _consultaDocumentoService.ConsultarRucAsync(ruc);

            if (!resultado.Exitoso && !resultado.ClienteYaExiste)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }
    }
}