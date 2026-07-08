namespace Sistema3S.Web.DTOs.Rol
{
    public class RolRespuestaListadoDto
    {
        public string Mensaje { get; set; } = string.Empty;

        public List<RolListadoDto> Roles { get; set; } = new();
    }
}