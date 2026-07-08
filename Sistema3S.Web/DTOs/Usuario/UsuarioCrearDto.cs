namespace Sistema3S.Web.DTOs.Usuario
{
    public class UsuarioCrearDto
    {
        public int IdRol { get; set; }

        public string Correo { get; set; } = string.Empty;

        public string? ContrasenaInicial { get; set; }

        public string? Contrasena { get; set; }

        public string? Password { get; set; }

        public string ObtenerContrasenaInicial()
        {
            return (
                ContrasenaInicial ??
                Contrasena ??
                Password ??
                string.Empty
            ).Trim();
        }
    }
}