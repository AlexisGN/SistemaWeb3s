namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class CotizacionMarcarRespondidaDto
    {
        public int? IdUsuarioAtencion { get; set; }
        public string CanalEnvio { get; set; } = "WhatsApp";
    }
}