using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Inventario;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IInventarioService
    {
        Task<ResultadoPaginadoDto<InventarioListadoDto>> ListarAsync(
            string? buscar,
            string? estadoStock,
            int pagina,
            int tamanioPagina
        );

        Task<InventarioListadoDto?> ObtenerPorProductoAsync(int idProducto);

        Task<InventarioResumenDto> ObtenerResumenAsync();

        Task<List<MovimientoStockListadoDto>> ListarMovimientosPorProductoAsync(int idProducto);

        Task<List<MovimientoStockListadoDto>> ListarMovimientosRecientesAsync(int cantidad);

        Task<InventarioListadoDto> ActualizarStockMinimoAsync(
            int idProducto,
            ActualizarStockMinimoDto dto
        );

        Task<InventarioListadoDto> RegistrarMovimientoManualAsync(
            RegistrarMovimientoStockDto dto
        );
    }
}