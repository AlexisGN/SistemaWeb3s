namespace Sistema3S.Web.DTOs.Venta
{
    public class VentaListadoDto
    {
        public int IdVenta { get; set; }
        public int IdCliente { get; set; }
        public int? IdCotizacion { get; set; }

        public DateTime FechaVenta { get; set; }

        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string DocumentoCompleto { get; set; } = string.Empty;

        public string DocumentoCliente { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;

        public string OrigenVenta { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }

        public string EstadoVenta { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
    }
}