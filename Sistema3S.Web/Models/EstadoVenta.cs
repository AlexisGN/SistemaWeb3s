using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class EstadoVenta
{
    public int IdEstadoVenta { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
