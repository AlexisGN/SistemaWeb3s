namespace Sistema3S.Web.DTOs.Venta
{
    public class VentaCrearDto
    {
        public int IdCliente { get; set; }
        public int? IdCotizacion { get; set; }
        public int IdUsuarioRegistro { get; set; } = 1;

        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public DateTime? FechaEmision { get; set; }

        public string? Observacion { get; set; }

        public string TipoPago { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }

        public int? NumeroCuotas { get; set; }
        public DateTime? FechaPrimerVencimiento { get; set; }

        public List<VentaDetalleCrearDto> Detalles { get; set; } = new();

        // Cronograma editable de cuotas.
        // Se usa cuando TipoPago = "Cuotas".
        public List<CuotaVentaCrearDto> Cuotas { get; set; } = new();
    }

    public class CuotaVentaCrearDto
    {
        public int NumeroCuota { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        // Este monto se usa para validar/mostrar desde frontend.
        // El SQL recalcula los montos finales para evitar descuadres.
        public decimal MontoCuota { get; set; }
    }
}