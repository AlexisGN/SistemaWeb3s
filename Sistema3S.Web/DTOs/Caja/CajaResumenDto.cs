namespace Sistema3S.Web.DTOs.Caja
{
    public class CajaResumenDto
    {
        public int IdCaja { get; set; }

        public int IdEstadoCaja { get; set; }
        public string EstadoCaja { get; set; } = string.Empty;

        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }

        public decimal SaldoInicial { get; set; }

        public decimal IngresosPorVenta { get; set; }
        public decimal IngresosManuales { get; set; }
        public decimal AjustesIngreso { get; set; }

        public decimal EgresosPorCompra { get; set; }
        public decimal EgresosManuales { get; set; }
        public decimal AjustesEgreso { get; set; }

        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal SaldoSistema { get; set; }

        public decimal? SaldoContado { get; set; }
        public decimal? Diferencia { get; set; }

        public string? ObservacionApertura { get; set; }
        public string? ObservacionCierre { get; set; }
    }
}