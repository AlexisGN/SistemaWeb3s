namespace Sistema3S.Web.DTOs.Compra
{
    public class CompraDetalleCompletoDto
    {
        public int IdCompra { get; set; }

        public DateTime FechaCompra { get; set; }
        public DateTime? FechaEmisionComprobante { get; set; }

        public string TipoComprobanteProveedor { get; set; } = string.Empty;
        public string SerieComprobante { get; set; } = string.Empty;
        public string NumeroComprobante { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }

        public string? Observacion { get; set; }

        public string EstadoCompra { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;

        public int IdProveedor { get; set; }
        public string RucProveedor { get; set; } = string.Empty;
        public string RazonSocialProveedor { get; set; } = string.Empty;
        public string? NombreComercialProveedor { get; set; }
        public string? CorreoProveedor { get; set; }
        public string? TelefonoProveedor { get; set; }
        public string? DireccionProveedor { get; set; }

        public GuiaRemisionCompraDetalleDto? GuiaRemision { get; set; }

        public List<DetalleCompraDetalleDto> Detalles { get; set; } = new();
        public List<PagoCompraDetalleDto> Pagos { get; set; } = new();
        public List<CuotaCompraDetalleDto> Cuotas { get; set; } = new();
    }

    public class GuiaRemisionCompraDetalleDto
    {
        public int IdGuiaRemisionCompra { get; set; }
        public string NumeroGuia { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaTraslado { get; set; }
        public string PuntoPartida { get; set; } = string.Empty;
        public string PuntoLlegada { get; set; } = string.Empty;
        public string? Transportista { get; set; }
        public string? RucTransportista { get; set; }
        public string? PlacaVehiculo { get; set; }
        public string? Observacion { get; set; }
    }

    public class DetalleCompraDetalleDto
    {
        public int IdDetalleCompra { get; set; }
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PagoCompraDetalleDto
    {
        public int IdPagoCompra { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }
        public DateTime FechaPago { get; set; }
        public string? Observacion { get; set; }
    }

    public class CuotaCompraDetalleDto
    {
        public int IdCuotaCompra { get; set; }
        public int NumeroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoCuota { get; set; }
        public decimal MontoPagado { get; set; }
        public string EstadoCuota { get; set; } = string.Empty;
    }
}