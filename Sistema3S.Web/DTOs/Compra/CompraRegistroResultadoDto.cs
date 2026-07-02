namespace Sistema3S.Web.DTOs.Compra
{
    public class CompraRegistroResultadoDto
    {
        public int IdCompra { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}