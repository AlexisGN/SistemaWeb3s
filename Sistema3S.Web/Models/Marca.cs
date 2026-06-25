using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Marca
{
    public int IdMarca { get; set; }

    public string Nombre { get; set; } = null!;

    public string? LogoUrl { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Producto> Producto { get; set; } = new List<Producto>();
}
