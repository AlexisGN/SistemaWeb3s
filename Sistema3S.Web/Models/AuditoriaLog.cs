using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class AuditoriaLog
{
    public int IdLog { get; set; }

    public int? IdUsuario { get; set; }

    public string Accion { get; set; } = null!;

    public string TablaAfectada { get; set; } = null!;

    public int? IdRegistro { get; set; }

    public DateTime Fecha { get; set; }

    public string? Descripcion { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
