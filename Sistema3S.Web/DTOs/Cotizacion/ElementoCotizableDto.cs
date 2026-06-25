namespace Sistema3S.Web.DTOs.Cotizacion
{
    public class ElementoCotizableDto
    {
        public int IdElementoCatalogo { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string TipoElemento { get; set; } = string.Empty;

        public decimal? PrecioReferencial { get; set; }

        public string TextoMostrar
        {
            get
            {
                return $"{TipoElemento} - {Nombre}";
            }
        }
    }
}