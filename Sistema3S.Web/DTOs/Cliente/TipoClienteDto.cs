namespace Sistema3S.Web.DTOs.Cliente
{
    public class TipoClienteDto
    {
        public int IdTipoCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool RequiereRuc { get; set; }
        public bool RequiereFactura { get; set; }
    }
}