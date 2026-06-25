using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class TipoElemento
{
    public int IdTipoElemento { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<ElementoCatalogo> ElementoCatalogo { get; set; } = new List<ElementoCatalogo>();
}
