using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Permiso
{
    public int IdPermiso { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Rol> IdRol { get; set; } = new List<Rol>();
}
