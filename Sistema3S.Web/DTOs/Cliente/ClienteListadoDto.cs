namespace Sistema3S.Web.DTOs.Cliente
{
    public class ClienteListadoDto
    {
        public int IdCliente { get; set; }

        public int IdTipoCliente { get; set; }
        public string TipoCliente { get; set; } = string.Empty;

        public int IdTipoDocumento { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;

        public int? IdUbigeo { get; set; }
        public string? Ubicacion { get; set; }

        public int? IdUsuario { get; set; }
        public bool CuentaWebVinculada { get; set; }

        public string NumeroDocumento { get; set; } = string.Empty;

        public string Cliente { get; set; } = string.Empty;

        public string? Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }

        public string? RazonSocial { get; set; }
        public string? NombreComercial { get; set; }

        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }

        public bool TieneContactoPrincipal { get; set; }
        public ClienteContactoDto? ContactoPrincipal { get; set; }
    }
}