using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class TipoServicioExterno
{
    public int IdTipoServicioExterno { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<ConsultaExterna> ConsultaExterna { get; set; } = new List<ConsultaExterna>();
}
