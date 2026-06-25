using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Comprobante
{
    public int IdComprobante { get; set; }

    public int IdVenta { get; set; }

    public int IdTipoComprobante { get; set; }

    public int IdEstadoComprobante { get; set; }

    public string Serie { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public DateTime FechaEmision { get; set; }

    public string? ArchivoPdf { get; set; }

    public string? ArchivoXml { get; set; }

    public int? IdSerieComprobante { get; set; }

    public virtual ICollection<EnvioCorreo> EnvioCorreo { get; set; } = new List<EnvioCorreo>();

    public virtual EstadoComprobante IdEstadoComprobanteNavigation { get; set; } = null!;

    public virtual SerieComprobante? IdSerieComprobanteNavigation { get; set; }

    public virtual TipoComprobante IdTipoComprobanteNavigation { get; set; } = null!;

    public virtual Venta IdVentaNavigation { get; set; } = null!;
}
