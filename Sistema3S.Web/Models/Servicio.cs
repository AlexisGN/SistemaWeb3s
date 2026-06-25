using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Servicio
{
    public int IdServicio { get; set; }

    public int IdElementoCatalogo { get; set; }

    public string? SectorAplicacion { get; set; }

    public string? MensajeWhatsApp { get; set; }

    public bool RequiereVisitaTecnica { get; set; }

    public virtual ElementoCatalogo IdElementoCatalogoNavigation { get; set; } = null!;
}
