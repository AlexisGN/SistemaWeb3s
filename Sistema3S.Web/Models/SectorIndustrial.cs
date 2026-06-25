using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class SectorIndustrial
{
    public int IdSectorIndustrial { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<ElementoCatalogo> IdElementoCatalogo { get; set; } = new List<ElementoCatalogo>();
}
