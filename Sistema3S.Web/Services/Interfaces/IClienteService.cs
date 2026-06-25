using Sistema3S.Web.DTOs.Cliente;
using Sistema3S.Web.DTOs.Comun;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IClienteService
    {
        Task<ResultadoPaginadoDto<ClienteListadoDto>> ListarAsync(
            string? buscar,
            int pagina,
            int tamanioPagina
        );

        Task<ClienteListadoDto?> ObtenerPorIdAsync(int idCliente);

        Task<ClienteListadoDto> CrearAsync(ClienteCrearDto dto);

        Task<bool> ActualizarAsync(int idCliente, ClienteActualizarDto dto);

        Task<bool> EliminarLogicoAsync(int idCliente);

        Task<int> ContarActivosAsync();

        Task<List<TipoClienteDto>> ListarTiposClienteAsync();

        Task<List<TipoDocumentoDto>> ListarTiposDocumentoAsync();

        Task<List<UbigeoDto>> ListarUbigeosAsync(string? buscar);
    }
}