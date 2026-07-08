namespace Sistema3S.Web.DTOs.Usuario
{
    public class UsuarioCambiarContrasenaDto
    {
        public string? NuevaContrasena { get; set; }

        public string? Contrasena { get; set; }

        public string? ContrasenaInicial { get; set; }

        public string? Password { get; set; }

        public string ObtenerContrasena()
        {
            return (
                NuevaContrasena ??
                Contrasena ??
                ContrasenaInicial ??
                Password ??
                string.Empty
            ).Trim();
        }
    }
}