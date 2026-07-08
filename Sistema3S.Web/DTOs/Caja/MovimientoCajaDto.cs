namespace Sistema3S.Web.DTOs.Caja
{
    public class MovimientoCajaDto
    {
        public int IdMovimientoCaja { get; set; }

        public int IdCaja { get; set; }

        public int IdTipoMovimientoCaja { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;

        public int? IdVenta { get; set; }
        public int? IdCompra { get; set; }

        public int? IdPagoVenta { get; set; }
        public int? IdPagoCompra { get; set; }

        public int IdUsuarioRegistro { get; set; }
        public string UsuarioRegistro { get; set; } = string.Empty;

        public string? MetodoPago { get; set; }

        public decimal Monto { get; set; }

        public string? Descripcion { get; set; }

        public DateTime FechaMovimiento { get; set; }

        public string OrigenMovimiento { get; set; } = string.Empty;

        public bool EsAutomatico { get; set; }

        public bool Estado { get; set; }

        public decimal Ingreso { get; set; }

        public decimal Egreso { get; set; }
    }
}