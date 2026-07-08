namespace Sistema3S.Web.DTOs.Rol
{
    public class RolListadoDto
    {
        public int IdRol { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public int TotalPermisos { get; set; }

        public int TotalUsuarios { get; set; }
    }
}