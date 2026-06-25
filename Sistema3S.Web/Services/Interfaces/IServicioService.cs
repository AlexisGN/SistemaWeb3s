using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Servicio;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IServicioService
    {
        Task<ResultadoPaginadoDto<ServicioListadoDto>> ListarAsync(
            string? buscar,
            int pagina,
            int tamanioPagina
        );

        Task<ServicioListadoDto?> ObtenerPorIdAsync(int idServicio);

        Task<ServicioListadoDto> CrearAsync(ServicioCrearDto dto);

        Task<bool> ActualizarAsync(int idServicio, ServicioActualizarDto dto);

        Task<bool> EliminarLogicoAsync(int idServicio);
        Task<int> ContarActivosAsync();
    }
}