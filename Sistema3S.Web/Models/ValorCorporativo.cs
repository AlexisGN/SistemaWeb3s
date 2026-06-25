using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class ValorCorporativo
{
    public int IdValor { get; set; }

    public int IdEmpresa { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual Empresa IdEmpresaNavigation { get; set; } = null!;
}
