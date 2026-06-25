namespace Sistema3S.Web.DTOs.Producto
{
    public class ProductoCrearDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal? PrecioReferencial { get; set; }
        public string? ImagenUrl { get; set; }

        public int IdCategoria { get; set; }
        public int? IdMarca { get; set; }
        public int? IdUnidadMedida { get; set; }

        public string CodigoProducto { get; set; } = string.Empty;
        public string? FichaTecnicaPdf { get; set; }

        public int StockInicial { get; set; }
        public int StockMinimo { get; set; }
    }
}