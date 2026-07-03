namespace Sistema3S.Web.DTOs.Venta
{
    public class VentaDetalleCrearDto
    {
        public int IdElementoCatalogo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public string? Codigo { get; set; }
        public string? Elemento { get; set; }
        public string? TipoElemento { get; set; }
    }
}