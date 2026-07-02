namespace Sistema3S.Web.DTOs.Inventario
{
    public class InventarioListadoDto
    {
        public int IdInventario { get; set; }
        public int IdProducto { get; set; }
        public int IdElementoCatalogo { get; set; }

        public string CodigoProducto { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;

        public string? Categoria { get; set; }
        public string? Marca { get; set; }
        public string? UnidadMedida { get; set; }

        public int StockActual { get; set; }
        public int StockMinimo { get; set; }

        public string EstadoStock { get; set; } = string.Empty;
        public bool TieneAlertaPendiente { get; set; }
        public string? MensajeAlerta { get; set; }

        public DateTime FechaActualizacion { get; set; }
    }
}