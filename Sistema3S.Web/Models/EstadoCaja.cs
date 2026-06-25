using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class EstadoCaja
{
    public int IdEstadoCaja { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Caja> Caja { get; set; } = new List<Caja>();
}
