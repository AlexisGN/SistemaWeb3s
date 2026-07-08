namespace Sistema3S.Web.DTOs.Compra
{
    public class PagoCompraCrearDto
    {
        public int IdCompra { get; set; }
        public int IdUsuarioRegistro { get; set; } = 1;

        public string MetodoPago { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }

        public string? Observacion { get; set; }

        // Para compras en cuotas:
        // aquí se envían las cuotas seleccionadas desde el frontend.
        public List<int> IdsCuotasPagadas { get; set; } = new();
    }
}