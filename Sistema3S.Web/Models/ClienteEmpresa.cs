using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class ClienteEmpresa
{
    public int IdCliente { get; set; }

    public string RazonSocial { get; set; } = null!;

    public string? NombreComercial { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
