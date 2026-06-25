using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class PersonalInterno
{
    public int IdPersonalInterno { get; set; }

    public int IdUsuario { get; set; }

    public string Nombres { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string? ApellidoMaterno { get; set; }

    public string? Telefono { get; set; }

    public string? Cargo { get; set; }

    public bool Estado { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
