namespace Sistema3S.Web.DTOs.Caja
{
    public class CajaAbrirDto
    {
        public int IdUsuarioApertura { get; set; } = 1;

        public decimal SaldoInicial { get; set; }

        public string? ObservacionApertura { get; set; }
    }
}