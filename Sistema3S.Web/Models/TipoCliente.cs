using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class TipoCliente
{
    public int IdTipoCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool RequiereRuc { get; set; }

    public bool RequiereFactura { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Cliente> Cliente { get; set; } = new List<Cliente>();
}
