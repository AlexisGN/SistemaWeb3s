namespace Sistema3S.Web.DTOs.Servicio
{
    public class ServicioListadoDto
    {
        public int IdServicio { get; set; }
        public int IdElementoCatalogo { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal? PrecioReferencial { get; set; }
        public string? ImagenUrl { get; set; }

        public string? SectorAplicacion { get; set; }
        public string? MensajeWhatsApp { get; set; }
        public bool RequiereVisitaTecnica { get; set; }

        public bool Estado { get; set; }
    }
}