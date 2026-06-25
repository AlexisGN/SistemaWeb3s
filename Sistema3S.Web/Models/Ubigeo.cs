using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Ubigeo
{
    public int IdUbigeo { get; set; }

    public string Departamento { get; set; } = null!;

    public string Provincia { get; set; } = null!;

    public string Distrito { get; set; } = null!;

    public string? CodigoUbigeo { get; set; }

    public virtual ICollection<Cliente> Cliente { get; set; } = new List<Cliente>();

    public virtual ICollection<Proveedor> Proveedor { get; set; } = new List<Proveedor>();
}
