namespace Sistema3S.Web.Services.Interfaces
{
    public interface IPdfVentaService
    {
        Task<byte[]> GenerarPdfVentaAsync(int idVenta);

        byte[] GenerarPdfReporteVentas(
            IEnumerable<Sistema3S.Web.DTOs.Venta.ReporteVentaDto> ventas,
            DateTime? fechaInicio,
            DateTime? fechaFin
        );
    }
}