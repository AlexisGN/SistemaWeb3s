namespace Sistema3S.Web.DTOs.Compra
{
    public class CompraCrearDto
    {
        public int IdProveedor { get; set; }
        public int IdUsuarioRegistro { get; set; } = 1;

        public string TipoComprobanteProveedor { get; set; } = string.Empty;
        public string SerieComprobante { get; set; } = string.Empty;
        public string NumeroComprobante { get; set; } = string.Empty;
        public DateTime? FechaEmisionComprobante { get; set; }

        public string? ObservacionCompra { get; set; }

        public GuiaRemisionCompraCrearDto GuiaRemision { get; set; } = new();

        public string TipoPago { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }

        public int? NumeroCuotas { get; set; }
        public DateTime? FechaPrimerVencimiento { get; set; }

        public string? ObservacionPago { get; set; }

        public List<CompraDetalleCrearDto> Detalles { get; set; } = new();

        // NUEVO:
        // Cronograma editable de cuotas.
        // Si viene vacío, el procedure usará la lógica anterior:
        // generar fechas mensuales desde FechaPrimerVencimiento.
        public List<CuotaCompraCrearDto> Cuotas { get; set; } = new();
    }

    public class CuotaCompraCrearDto
    {
        public int NumeroCuota { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        // Este monto sirve para mostrar/validar en frontend.
        // El SQL calculará los montos reales para evitar descuadres.
        public decimal MontoCuota { get; set; }
    }
}