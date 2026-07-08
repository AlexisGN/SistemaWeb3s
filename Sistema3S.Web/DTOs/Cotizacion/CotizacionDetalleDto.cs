namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class CotizacionDetalleDto
    {
        public int IdDetalleCotizacion { get; set; }
        public int IdCotizacion { get; set; }
        public int IdElementoCatalogo { get; set; }

        public string ElementoNombre { get; set; } = string.Empty;
        public string Elemento { get; set; } = string.Empty;
        public string TipoElemento { get; set; } = string.Empty;

        public int? IdProducto { get; set; }
        public string? CodigoProducto { get; set; }

        public int? IdServicio { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        public string? Observacion { get; set; }
    }
}