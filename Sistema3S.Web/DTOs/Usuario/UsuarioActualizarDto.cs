namespace Sistema3S.Web.DTOs.Usuario
{
    public class UsuarioActualizarDto
    {
        public int IdRol { get; set; }

        public string Correo { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public string? NuevaContrasena { get; set; }

        public string? Contrasena { get; set; }

        public string? Password { get; set; }

        public string ObtenerNuevaContrasena()
        {
            return (
                NuevaContrasena ??
                Contrasena ??
                Password ??
                string.Empty
            ).Trim();
        }
    }
}