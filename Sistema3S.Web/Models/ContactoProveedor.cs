using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class ContactoProveedor
{
    public int IdContactoProveedor { get; set; }

    public int IdProveedor { get; set; }

    public string Nombres { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string? ApellidoMaterno { get; set; }

    public string? Cargo { get; set; }

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public bool Estado { get; set; }

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
