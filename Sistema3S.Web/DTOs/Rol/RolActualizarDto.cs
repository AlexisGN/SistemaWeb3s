namespace Sistema3S.Web.DTOs.Rol
{
    public class RolActualizarDto
    {
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }
    }
}