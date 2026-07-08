namespace Sistema3S.Web.DTOs.Venta
{
    public class PagoVentaCrearDto
    {
        public int IdVenta { get; set; }
        public int IdUsuarioRegistro { get; set; } = 1;

        public string MetodoPago { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }

        public string? Observacion { get; set; }

        // Para ventas en cuotas:
        // aquí llegan las cuotas seleccionadas desde el frontend.
        public List<int> IdsCuotasCobradas { get; set; } = new();
    }
}