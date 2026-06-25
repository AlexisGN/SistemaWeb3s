using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class EstadoCompra
{
    public int IdEstadoCompra { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Compra> Compra { get; set; } = new List<Compra>();
}
