using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public int? IdUbigeo { get; set; }

    public string Ruc { get; set; } = null!;

    public string RazonSocial { get; set; } = null!;

    public string? NombreComercial { get; set; }

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<Compra> Compra { get; set; } = new List<Compra>();

    public virtual ICollection<ContactoProveedor> ContactoProveedor { get; set; } = new List<ContactoProveedor>();

    public virtual Ubigeo? IdUbigeoNavigation { get; set; }
}
