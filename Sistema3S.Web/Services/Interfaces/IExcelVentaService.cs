namespace Sistema3S.Web.Services.Interfaces
{
    public interface IExcelVentaService
    {
        byte[] GenerarExcelReporteVentas(
            IEnumerable<Sistema3S.Web.DTOs.Venta.ReporteVentaDto> ventas,
            DateTime? fechaInicio,
            DateTime? fechaFin
        );
    }
}