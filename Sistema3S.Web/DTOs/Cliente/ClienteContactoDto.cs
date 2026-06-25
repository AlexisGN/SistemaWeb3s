namespace Sistema3S.Web.DTOs.Cliente
{
    public class ClienteContactoDto
    {
        public int? IdContactoCliente { get; set; }

        public string? Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }

        public string? Cargo { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }

        public bool Estado { get; set; } = true;
    }
}