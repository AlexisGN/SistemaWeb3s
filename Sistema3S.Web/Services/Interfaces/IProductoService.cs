using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Producto;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IProductoService
    {
        Task<ResultadoPaginadoDto<ProductoListadoDto>> ListarAsync(
            string? buscar,
            int pagina,
            int tamanioPagina
        );

        Task<ProductoListadoDto?> ObtenerPorIdAsync(int idProducto);

        Task<ProductoListadoDto> CrearAsync(ProductoCrearDto dto);

        Task<bool> ActualizarAsync(int idProducto, ProductoActualizarDto dto);

        Task<bool> EliminarLogicoAsync(int idProducto);
        Task<int> ContarActivosAsync();
    }
}