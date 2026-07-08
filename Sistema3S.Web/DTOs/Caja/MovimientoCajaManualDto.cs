namespace Sistema3S.Web.DTOs.Caja
{
    public class MovimientoCajaManualDto
    {
        public int IdUsuarioRegistro { get; set; } = 1;

        public string TipoMovimiento { get; set; } = string.Empty;

        public string MetodoPago { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public string Descripcion { get; set; } = string.Empty;
    }
}