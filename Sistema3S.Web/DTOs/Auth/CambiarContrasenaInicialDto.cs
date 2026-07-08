namespace Sistema3S.Web.DTOs.Auth
{
    public class CambiarContrasenaInicialDto
    {
        public int IdUsuario { get; set; } = 1;

        public string NuevaContrasena { get; set; } = string.Empty;

        public string ClaveSeguridad { get; set; } = string.Empty;
    }
}