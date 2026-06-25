using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class TipoComprobante
{
    public int IdTipoComprobante { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Comprobante> Comprobante { get; set; } = new List<Comprobante>();

    public virtual ICollection<SerieComprobante> SerieComprobante { get; set; } = new List<SerieComprobante>();
}
