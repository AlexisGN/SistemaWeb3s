namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class CotizacionDetalleCrearDto
    {
        public int IdElementoCatalogo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string? Observacion { get; set; }
    }
}