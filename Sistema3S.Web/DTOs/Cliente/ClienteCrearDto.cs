namespace Sistema3S.Web.DTOs.Cliente
{
    public class ClienteCrearDto
    {
        public int IdTipoCliente { get; set; }
        public int IdTipoDocumento { get; set; }
        public int? IdUbigeo { get; set; }

        public string NumeroDocumento { get; set; } = string.Empty;

        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        // Persona Natural
        public string? Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }

        // Empresa
        public string? RazonSocial { get; set; }
        public string? NombreComercial { get; set; }

        // Contacto opcional para empresa
        public bool RegistrarContactoPrincipal { get; set; }
        public ClienteContactoDto? ContactoPrincipal { get; set; }
    }
}