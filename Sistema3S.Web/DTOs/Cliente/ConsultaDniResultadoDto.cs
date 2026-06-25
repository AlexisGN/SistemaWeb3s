namespace Sistema3S.Web.DTOs.Cliente
{
    public class ConsultaDniResultadoDto
    {
        public bool Exitoso { get; set; }
        public bool ClienteYaExiste { get; set; }
        public int? IdClienteExistente { get; set; }

        public string NumeroDocumento { get; set; } = string.Empty;

        public string? Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? NombreCompleto { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}