namespace Sistema3S.Web.DTOs.Auth
{
    public class PermisoSesionDto
    {
        public int IdPermiso { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Asignado { get; set; }
    }
}