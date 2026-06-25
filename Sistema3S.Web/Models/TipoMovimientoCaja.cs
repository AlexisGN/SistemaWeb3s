using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class TipoMovimientoCaja
{
    public int IdTipoMovimientoCaja { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<MovimientoCaja> MovimientoCaja { get; set; } = new List<MovimientoCaja>();
}
