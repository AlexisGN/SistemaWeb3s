namespace Sistema3S.Web.DTOs.Permiso
{
    public class PermisoDto
    {
        public int IdPermiso { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public bool Asignado { get; set; }
    }
}