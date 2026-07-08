using Sistema3S.Web.DTOs.Compra;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IPdfCompraService
    {
        Task<byte[]> GenerarPdfCompraAsync(int idCompra);

        byte[] GenerarPdfReporteCompras(
            IEnumerable<ReporteCompraDto> compras,
            DateTime? fechaInicio,
            DateTime? fechaFin
        );
    }
}