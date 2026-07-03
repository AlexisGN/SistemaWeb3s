using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Venta;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IVentaService
    {
        Task<ResultadoPaginadoDto<VentaListadoDto>> ListarAsync(
            string? buscar,
            string? estadoPago,
            string? tipoComprobante,
            string? origenVenta,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina,
            int tamanioPagina
        );

        Task<VentaRegistroResultadoDto> RegistrarVentaCompletaAsync(VentaCrearDto dto);

        Task<VentaDetalleCompletoDto?> ObtenerDetalleAsync(int idVenta);

        Task<VentaRegistroResultadoDto> RegistrarPagoAsync(PagoVentaCrearDto dto);

        Task<bool> AnularAsync(int idVenta, AnularVentaDto dto);
        Task<SiguienteComprobanteDto> ObtenerSiguienteComprobanteAsync(string tipoComprobante);

        Task<List<ReporteVentaDto>> ObtenerReporteAsync(
            string? buscar,
            string? estadoPago,
            string? tipoComprobante,
            string? origenVenta,
            DateTime? fechaInicio,
            DateTime? fechaFin
        );
    }
}