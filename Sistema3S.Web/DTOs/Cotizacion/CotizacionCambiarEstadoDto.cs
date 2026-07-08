namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class CotizacionCambiarEstadoDto
    {
        public int? IdEstadoCotizacion { get; set; }
        public string? NuevoEstado { get; set; }
        public int? IdUsuarioAtencion { get; set; }
    }
}