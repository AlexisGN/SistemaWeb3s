namespace Sistema3S.Web.DTOs.Producto
{
    public class ProductoListadoDto
    {
        public int IdProducto { get; set; }
        public int IdElementoCatalogo { get; set; }

        public int IdCategoria { get; set; }
        public int? IdMarca { get; set; }
        public int? IdUnidadMedida { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal? PrecioReferencial { get; set; }
        public string? ImagenUrl { get; set; }

        public string Categoria { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? UnidadMedida { get; set; }

        public string CodigoProducto { get; set; } = string.Empty;
        public string? FichaTecnicaPdf { get; set; }

        public int StockActual { get; set; }
        public int StockMinimo { get; set; }

        public bool Estado { get; set; }
    }
}