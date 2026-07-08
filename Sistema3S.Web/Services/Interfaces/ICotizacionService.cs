using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Cotizacion;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface ICotizacionService
    {
        Task<ResultadoPaginadoDto<CotizacionListadoDto>> ListarAsync(
            string? buscar,
            string? estado,
            string? origen,
            int pagina,
            int tamanioPagina
        );

        Task<CotizacionListadoDto?> ObtenerPorIdAsync(int idCotizacion);

        Task<CotizacionListadoDto> CrearAsync(CotizacionCrearDto dto);

        Task<bool> CancelarAsync(int idCotizacion, int? idUsuarioAtencion);

        Task<bool> CambiarEstadoAsync(
            int idCotizacion,
            CotizacionCambiarEstadoDto dto
        );

        Task<string> GenerarPdfAsync(int idCotizacion);

        Task<bool> EnviarCorreoAsync(
            int idCotizacion,
            int? idUsuarioAtencion
        );

        Task<CotizacionWhatsAppDto> ObtenerWhatsAppAsync(int idCotizacion);

        Task<bool> MarcarRespondidaAsync(
            int idCotizacion,
            CotizacionMarcarRespondidaDto dto
        );

        Task<CotizacionListadoDto> PrepararParaVentaAsync(int idCotizacion);

        Task<CotizacionWhatsAppDto> EnviarWhatsAppAsync(
    int idCotizacion,
    int? idUsuarioAtencion
);

        Task<bool> MarcarConvertidaVentaAsync(
            int idCotizacion,
            CotizacionConvertirVentaDto dto
        );

        Task<int> ContarPendientesAsync();

        Task<List<ClienteSelectorDto>> ListarClientesAsync();

        Task<List<ElementoCotizableDto>> ListarElementosCotizablesAsync();

        Task<List<EstadoCotizacionDto>> ListarEstadosAsync();
    }
}