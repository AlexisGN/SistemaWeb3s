namespace Sistema3S.Web.DTOs.Compra
{
    public class ReporteCompraDto
    {
        public int IdCompra { get; set; }
        public DateTime FechaCompra { get; set; }

        public string TipoComprobanteProveedor { get; set; } = string.Empty;
        public string SerieComprobante { get; set; } = string.Empty;
        public string NumeroComprobante { get; set; } = string.Empty;
        public string DocumentoCompleto { get; set; } = string.Empty;

        public string RucProveedor { get; set; } = string.Empty;
        public string RazonSocialProveedor { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }

        public string EstadoCompra { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;

        public bool TieneGuia { get; set; }
    }
}