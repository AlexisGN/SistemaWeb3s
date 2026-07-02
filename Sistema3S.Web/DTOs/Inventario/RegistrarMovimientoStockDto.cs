namespace Sistema3S.Web.DTOs.Inventario
{
    public class RegistrarMovimientoStockDto
    {
        public int IdProducto { get; set; }

        public string TipoMovimiento { get; set; } = string.Empty;

        public int? Cantidad { get; set; }

        public int? NuevoStock { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public int? IdUsuarioRegistro { get; set; }
    }
}