using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Cotizacion;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface ICotizacionService
    {
        Task<ResultadoPaginadoDto<CotizacionListadoDto>> ListarAsync(
            string? buscar,
            int pagina,
            int tamanioPagina
        );

        Task<CotizacionListadoDto?> ObtenerPorIdAsync(int idCotizacion);

        Task<CotizacionListadoDto> CrearAsync(CotizacionCrearDto dto);

        Task<bool> ActualizarAsync(int idCotizacion, CotizacionActualizarDto dto);

        Task<bool> CancelarAsync(int idCotizacion);

        Task<bool> CambiarEstadoAsync(int idCotizacion, int idEstadoCotizacion);

        Task<string> GenerarPdfAsync(int idCotizacion);

        Task<bool> MarcarCorreoEnviadoAsync(int idCotizacion);

        Task<CotizacionWhatsAppDto> ObtenerWhatsAppAsync(int idCotizacion);

        Task<int> ConvertirEnVentaAsync(int idCotizacion, int? idUsuarioRegistro);

        Task<int> ContarPendientesAsync();

        Task<List<ClienteSelectorDto>> ListarClientesAsync();

        Task<List<ElementoCotizableDto>> ListarElementosCotizablesAsync();

        Task<List<EstadoCotizacionDto>> ListarEstadosAsync();
    }
}