using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class AceptacionDocumentoLegal
{
    public int IdAceptacionDocumentoLegal { get; set; }

    public int IdUsuario { get; set; }

    public int IdDocumentoLegal { get; set; }

    public DateTime FechaAceptacion { get; set; }

    public string? IpAceptacion { get; set; }

    public virtual DocumentoLegal IdDocumentoLegalNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
