namespace Sistema3S.Web.DTOs.Proveedor
{
    public class ContactoProveedorDto
    {
        public int? IdContactoProveedor { get; set; }

        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }

        public string? Cargo { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }

        public bool Estado { get; set; } = true;
    }
}