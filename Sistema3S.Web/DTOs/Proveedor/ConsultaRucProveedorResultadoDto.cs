namespace Sistema3S.Web.DTOs.Proveedor
{
    public class ConsultaRucProveedorResultadoDto
    {
        public bool Exitoso { get; set; }
        public bool ProveedorYaExiste { get; set; }
        public int? IdProveedorExistente { get; set; }

        public string NumeroDocumento { get; set; } = string.Empty;

        public string? RazonSocial { get; set; }
        public string? NombreComercial { get; set; }

        public string? EstadoSunat { get; set; }
        public string? CondicionSunat { get; set; }

        public string? Direccion { get; set; }

        public string? CodigoUbigeo { get; set; }
        public int? IdUbigeo { get; set; }
        public string? Ubicacion { get; set; }

        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}