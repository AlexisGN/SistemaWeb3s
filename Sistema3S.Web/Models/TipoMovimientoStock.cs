using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class TipoMovimientoStock
{
    public int IdTipoMovimientoStock { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<MovimientoStock> MovimientoStock { get; set; } = new List<MovimientoStock>();
}
