using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class SerieComprobante
{
    public int IdSerieComprobante { get; set; }

    public int IdTipoComprobante { get; set; }

    public string Serie { get; set; } = null!;

    public int NumeroActual { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Comprobante> Comprobante { get; set; } = new List<Comprobante>();

    public virtual TipoComprobante IdTipoComprobanteNavigation { get; set; } = null!;
}
