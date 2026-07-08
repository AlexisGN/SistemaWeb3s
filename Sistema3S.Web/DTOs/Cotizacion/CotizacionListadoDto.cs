namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class CotizacionListadoDto
    {
        public int IdCotizacion { get; set; }
        public string CodigoCotizacion { get; set; } = string.Empty;

        public int IdCliente { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string TipoDocumentoCliente { get; set; } = string.Empty;
        public string DocumentoCliente { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = string.Empty;

        public string? CorreoCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? DireccionCliente { get; set; }

        public int? IdUsuarioRegistro { get; set; }
        public int? IdUsuarioAtencion { get; set; }

        public int IdEstadoCotizacion { get; set; }
        public string EstadoCotizacion { get; set; } = string.Empty;

        public string OrigenCotizacion { get; set; } = string.Empty;

        public DateTime FechaCotizacion { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string? CanalRespuesta { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public decimal TotalReferencial { get; set; }

        public string? Observacion { get; set; }
        public string? ArchivoPdf { get; set; }

        public bool CorreoEnviado { get; set; }
        public bool WhatsappEnviado { get; set; }
        public bool PdfGenerado { get; set; }

        public int CantidadDetalles { get; set; }

        public int? IdVentaGenerada { get; set; }
        public int? IdVentaAsociada { get; set; }

        public bool PuedeConvertirVenta { get; set; }
        public bool PuedeGestionar { get; set; }

        public List<CotizacionDetalleDto> Detalles { get; set; } = new();
    }
}