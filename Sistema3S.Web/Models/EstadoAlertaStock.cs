using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class EstadoAlertaStock
{
    public int IdEstadoAlertaStock { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<AlertaStock> AlertaStock { get; set; } = new List<AlertaStock>();
}
