namespace Sistema3S.Web.DTOs.Proveedor
{
    public class ProveedorCrearDto
    {
        public int? IdUbigeo { get; set; }

        public string Ruc { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }

        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        public bool RegistrarContactoPrincipal { get; set; }
        public ContactoProveedorDto? ContactoPrincipal { get; set; }
    }
}