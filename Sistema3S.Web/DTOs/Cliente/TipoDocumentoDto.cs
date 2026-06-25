namespace Sistema3S.Web.DTOs.Cliente
{
    public class TipoDocumentoDto
    {
        public int IdTipoDocumento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Longitud { get; set; }
    }
}