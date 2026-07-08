using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Compra;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface ICompraService
    {
        Task<CompraRegistroResultadoDto> RegistrarCompraCompletaAsync(CompraCrearDto dto);

        Task<ResultadoPaginadoDto<CompraListadoDto>> ListarAsync(
            string? buscar,
            string? estadoPago,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina,
            int tamanioPagina
        );

        Task<CompraDetalleCompletoDto?> ObtenerDetalleAsync(int idCompra);

        Task<CompraRegistroResultadoDto> RegistrarPagoAsync(PagoCompraCrearDto dto);

        Task<bool> AnularAsync(int idCompra, AnularCompraDto dto);

        Task<List<ReporteCompraDto>> ObtenerReporteAsync(
            string? buscar,
            string? estadoPago,
            DateTime? fechaInicio,
            DateTime? fechaFin
        );
    }
}