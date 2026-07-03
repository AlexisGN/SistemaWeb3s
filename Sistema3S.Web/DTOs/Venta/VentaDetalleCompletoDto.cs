namespace Sistema3S.Web.DTOs.Venta
{
    public class VentaDetalleCompletoDto
    {
        public int IdVenta { get; set; }
        public int IdCliente { get; set; }
        public int? IdCotizacion { get; set; }

        public DateTime FechaVenta { get; set; }

        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string DocumentoCompleto { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }

        public string TipoDocumentoCliente { get; set; } = string.Empty;
        public string DocumentoCliente { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string? CorreoCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? DireccionCliente { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }

        public string EstadoVenta { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public string OrigenVenta { get; set; } = string.Empty;

        public string? Observacion { get; set; }

        public List<VentaDetalleItemDto> Detalles { get; set; } = new();
        public List<PagoVentaDetalleDto> Pagos { get; set; } = new();
        public List<CuotaVentaDetalleDto> Cuotas { get; set; } = new();
    }

    public class VentaDetalleItemDto
    {
        public int IdDetalleVenta { get; set; }
        public int IdElementoCatalogo { get; set; }
        public string Elemento { get; set; } = string.Empty;
        public string TipoElemento { get; set; } = string.Empty;

        public int? IdProducto { get; set; }
        public string? CodigoProducto { get; set; }

        public int? IdServicio { get; set; }

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PagoVentaDetalleDto
    {
        public int IdPagoVenta { get; set; }
        public int IdVenta { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }
        public DateTime FechaPago { get; set; }
        public string? Observacion { get; set; }
    }

    public class CuotaVentaDetalleDto
    {
        public int IdCuotaVenta { get; set; }
        public int IdVenta { get; set; }
        public int NumeroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoCuota { get; set; }
        public decimal MontoPagado { get; set; }
        public string EstadoCuota { get; set; } = string.Empty;
    }
}