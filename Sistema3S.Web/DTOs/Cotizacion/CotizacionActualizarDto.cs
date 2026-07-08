namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class CotizacionActualizarDto
    {
        public int IdCliente { get; set; }

        public int? IdUsuarioAtencion { get; set; }

        public string OrigenCotizacion { get; set; } = "Manual";

        public decimal Descuento { get; set; } = 0;

        public string? Observacion { get; set; }

        public List<CotizacionDetalleCrearDto> Detalles { get; set; } = new();
    }
}