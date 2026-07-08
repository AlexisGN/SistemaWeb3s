using Sistema3S.Web.DTOs.Usuario;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<UsuarioListadoDto>> ListarAsync(
            string? buscar,
            int? idRol,
            bool? estado
        );

        Task<UsuarioOperacionResultadoDto> CrearAsync(UsuarioCrearDto dto);

        Task<UsuarioOperacionResultadoDto> ActualizarAsync(
            int idUsuario,
            UsuarioActualizarDto dto
        );

        Task<UsuarioOperacionResultadoDto> CambiarContrasenaAsync(
            int idUsuario,
            UsuarioCambiarContrasenaDto dto
        );

        Task<UsuarioOperacionResultadoDto> DesactivarAsync(int idUsuario);

        Task<UsuarioOperacionResultadoDto> ActivarAsync(int idUsuario);
    }
}
