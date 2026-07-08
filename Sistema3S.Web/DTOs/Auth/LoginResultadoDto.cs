namespace Sistema3S.Web.DTOs.Auth
{
    public class LoginResultadoDto
    {
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public string Correo { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public DateTime Expira { get; set; }

        public List<string> Permisos { get; set; } = new();

        public List<PermisoSesionDto> PermisosDetalle { get; set; } = new();
    }
}