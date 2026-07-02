namespace Sistema3S.Web.DTOs.Inventario
{
    public class MovimientoStockListadoDto
    {
        public int IdMovimientoStock { get; set; }

        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;

        public string TipoMovimiento { get; set; } = string.Empty;
        public int Cantidad { get; set; }

        public DateTime FechaMovimiento { get; set; }
        public string? Motivo { get; set; }

        public int? IdVenta { get; set; }
        public int? IdCompra { get; set; }

        public string UsuarioRegistro { get; set; } = string.Empty;
    }
}