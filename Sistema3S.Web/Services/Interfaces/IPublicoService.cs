using Sistema3S.Web.DTOs.Publico;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IPublicoService
    {
        Task<InicioPublicoDto> ObtenerInicioAsync();
    }
}