using Sistema3S.Web.DTOs.Permiso;
using Sistema3S.Web.DTOs.Rol;

namespace Sistema3S.Web.Services.Interfaces
{
    public interface IRolService
    {
        Task<List<RolListadoDto>> ListarAsync(bool? soloActivos);

        Task<RolRespuestaListadoDto> CrearAsync(RolCrearDto dto);

        Task<RolRespuestaListadoDto> ActualizarAsync(int idRol, RolActualizarDto dto);

        Task<RolOperacionResultadoDto> DesactivarAsync(int idRol);

        Task<List<PermisoDto>> ListarPermisosAsync();

        Task<List<PermisoDto>> ObtenerPermisosPorRolAsync(int idRol);

        Task<RolOperacionResultadoDto> AsignarPermisosAsync(
            int idRol,
            RolPermisosActualizarDto dto
        );
    }
}