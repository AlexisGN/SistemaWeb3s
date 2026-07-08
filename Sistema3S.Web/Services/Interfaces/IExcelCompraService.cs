using Sistema3S.Web.DTOs.Compra;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IExcelCompraService
    {
        byte[] GenerarExcelReporteCompras(
            IEnumerable<ReporteCompraDto> compras,
            DateTime? fechaInicio,
            DateTime? fechaFin
        );
    }
}