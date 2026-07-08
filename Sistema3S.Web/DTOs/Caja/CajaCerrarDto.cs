namespace Sistema3S.Web.DTOs.Caja
{
    public class CajaCerrarDto
    {
        public int IdUsuarioCierre { get; set; } = 1;

        public int IdCaja { get; set; }

        public decimal SaldoContado { get; set; }

        public string? ObservacionCierre { get; set; }
    }
}