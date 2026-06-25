using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Rol
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Usuario> Usuario { get; set; } = new List<Usuario>();

    public virtual ICollection<Permiso> IdPermiso { get; set; } = new List<Permiso>();
}
