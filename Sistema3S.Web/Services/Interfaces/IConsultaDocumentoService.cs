using Sistema3S.Web.DTOs.Cliente;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IConsultaDocumentoService
    {
        Task<ConsultaDniResultadoDto> ConsultarDniAsync(string dni, int? idUsuarioRegistro = null);
        Task<ConsultaRucResultadoDto> ConsultarRucAsync(string ruc, int? idUsuarioRegistro = null);
    }
}