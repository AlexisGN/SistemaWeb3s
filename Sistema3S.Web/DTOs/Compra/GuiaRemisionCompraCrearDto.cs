namespace Sistema3S.Web.DTOs.Compra
{
    public class GuiaRemisionCompraCrearDto
    {
        public bool TieneGuia { get; set; }

        public string? NumeroGuia { get; set; }
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaTraslado { get; set; }

        public string? PuntoPartida { get; set; }
        public string? PuntoLlegada { get; set; }

        public string? Transportista { get; set; }
        public string? RucTransportista { get; set; }
        public string? PlacaVehiculo { get; set; }

        public string? Observacion { get; set; }
    }
}