using Sistema3S.Web.DTOs.Caja;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface ICajaService
    {
        Task<CajaActivaDto?> ObtenerCajaActivaAsync(int idUsuario);

        Task<CajaActivaDto> AbrirCajaAsync(CajaAbrirDto dto);

        Task<CajaResumenDto> ObtenerResumenAsync(int idUsuario, int? idCaja);

        Task<List<MovimientoCajaDto>> ListarMovimientosAsync(
            int idUsuario,
            int? idCaja,
            DateTime? fechaInicio,
            DateTime? fechaFin
        );

        Task<CajaOperacionResultadoDto> RegistrarMovimientoManualAsync(
            MovimientoCajaManualDto dto
        );

        Task<CajaActivaDto> CerrarCajaAsync(CajaCerrarDto dto);

        Task<List<CajaReporteDto>> ObtenerReporteAsync(
            int idUsuario,
            DateTime? fechaInicio,
            DateTime? fechaFin
        );
    }
}