namespace Sistema3S.Web.DTOs.Venta
{
    public class VentaRegistroResultadoDto
    {
        public int IdVenta { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string EstadoPago { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }
}