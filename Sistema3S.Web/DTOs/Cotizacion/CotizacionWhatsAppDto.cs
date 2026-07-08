namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class CotizacionWhatsAppDto
    {
        public int IdCotizacion { get; set; }
        public string CodigoCotizacion { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public string? ArchivoPdf { get; set; }
        public bool RequiereConfirmacionRespondida { get; set; } = true;
    }
}