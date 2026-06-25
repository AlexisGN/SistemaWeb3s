using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class ClientePersonaNatural
{
    public int IdCliente { get; set; }

    public string Nombres { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string? ApellidoMaterno { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
