using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class UnidadMedida
{
    public int IdUnidadMedida { get; set; }

    public string Nombre { get; set; } = null!;

    public string Abreviatura { get; set; } = null!;

    public bool Estado { get; set; }

    public virtual ICollection<Producto> Producto { get; set; } = new List<Producto>();
}
