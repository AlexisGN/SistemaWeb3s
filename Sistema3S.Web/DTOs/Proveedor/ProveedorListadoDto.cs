namespace Sistema3S.Web.DTOs.Proveedor
{
    public class ProveedorListadoDto
    {
        public int IdProveedor { get; set; }

        public int? IdUbigeo { get; set; }
        public string? Ubicacion { get; set; }
        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }

        public string Ruc { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }

        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public bool Estado { get; set; }

        public ContactoProveedorDto? ContactoPrincipal { get; set; }
    }
}