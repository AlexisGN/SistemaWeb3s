namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class CotizacionCrearDto
    {
        public int IdCliente { get; set; }
        public int IdEstadoCotizacion { get; set; }

        public int? IdUsuarioRegistro { get; set; }
        public int? IdUsuarioAtencion { get; set; }

        public string OrigenCotizacion { get; set; } = "Manual";

        public decimal Descuento { get; set; } = 0;

        public string? Observacion { get; set; }
        public string? ArchivoPdf { get; set; }
        public bool CorreoEnviado { get; set; }

        public List<CotizacionDetalleCrearDto> Detalles { get; set; } = new();
    }
}