using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Empresa
{
    public int IdEmpresa { get; set; }

    public string? Ruc { get; set; }

    public string RazonSocial { get; set; } = null!;

    public string NombreComercial { get; set; } = null!;

    public string? Rubro { get; set; }

    public string? ActividadPrincipal { get; set; }

    public string? Mision { get; set; }

    public string? Vision { get; set; }

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public string? SitioWeb { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<ValorCorporativo> ValorCorporativo { get; set; } = new List<ValorCorporativo>();
}
