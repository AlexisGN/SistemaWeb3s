using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Proveedor;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IProveedorService
    {
        Task<ResultadoPaginadoDto<ProveedorListadoDto>> ListarAsync(
            string? buscar,
            int pagina,
            int tamanioPagina
        );

        Task<ProveedorListadoDto?> ObtenerPorIdAsync(int idProveedor);

        Task<ProveedorListadoDto> CrearAsync(ProveedorCrearDto dto);

        Task<bool> ActualizarAsync(int idProveedor, ProveedorActualizarDto dto);

        Task<bool> EliminarLogicoAsync(int idProveedor);

        Task<int> ContarActivosAsync();

        Task<ConsultaRucProveedorResultadoDto> ConsultarRucAsync(string ruc);
    }
}