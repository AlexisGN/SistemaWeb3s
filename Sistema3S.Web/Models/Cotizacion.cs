using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Cotizacion
{
    public int IdCotizacion { get; set; }

    public int IdCliente { get; set; }

    public int? IdUsuarioRegistro { get; set; }

    public int? IdUsuarioAtencion { get; set; }

    public int IdEstadoCotizacion { get; set; }

    public DateTime FechaCotizacion { get; set; }

    public decimal TotalReferencial { get; set; }

    public string? Observacion { get; set; }

    public string? ArchivoPdf { get; set; }

    public bool CorreoEnviado { get; set; }

    public string OrigenCotizacion { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Igv { get; set; }

    public decimal Total { get; set; }

    public virtual ICollection<DetalleCotizacion> DetalleCotizacion { get; set; } = new List<DetalleCotizacion>();

    public virtual ICollection<EnvioCorreo> EnvioCorreo { get; set; } = new List<EnvioCorreo>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual EstadoCotizacion IdEstadoCotizacionNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioAtencionNavigation { get; set; }

    public virtual Usuario? IdUsuarioRegistroNavigation { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
