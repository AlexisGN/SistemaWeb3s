using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class TipoDocumento
{
    public int IdTipoDocumento { get; set; }

    public string Nombre { get; set; } = null!;

    public int Longitud { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Cliente> Cliente { get; set; } = new List<Cliente>();
}
