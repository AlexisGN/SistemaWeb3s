namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class ClienteSelectorDto
    {
        public int IdCliente { get; set; }

        public string Cliente { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;

        public string TextoMostrar
        {
            get
            {
                return $"{Cliente} - {TipoDocumento} {NumeroDocumento}";
            }
        }
    }
}