namespace Sistema3S.Web.DTOs.Caja
{
    public class CajaActivaDto
    {
        public int IdCaja { get; set; }

        public int IdUsuarioApertura { get; set; }
        public string? UsuarioApertura { get; set; }

        public int? IdUsuarioCierre { get; set; }
        public string? UsuarioCierre { get; set; }

        public int IdEstadoCaja { get; set; }
        public string EstadoCaja { get; set; } = string.Empty;

        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }

        public decimal SaldoInicial { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal SaldoSistema { get; set; }

        public decimal? SaldoFinal { get; set; }
        public decimal? SaldoContado { get; set; }
        public decimal? Diferencia { get; set; }

        public string? ObservacionApertura { get; set; }
        public string? ObservacionCierre { get; set; }

        public string? Mensaje { get; set; }
    }
}