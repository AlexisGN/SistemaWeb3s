namespace Sistema3S.Web.DTOs.Cliente
{
    public class UbigeoDto
    {
        public int IdUbigeo { get; set; }

        public string? CodigoUbigeo { get; set; }

        public string Departamento { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;

        public string Ubicacion { get; set; } = string.Empty;
    }
}