using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class ImagenElementoCatalogo
{
    public int IdImagenElementoCatalogo { get; set; }

    public int IdElementoCatalogo { get; set; }

    public string UrlImagen { get; set; } = null!;

    public string? TextoAlternativo { get; set; }

    public bool EsPrincipal { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ElementoCatalogo IdElementoCatalogoNavigation { get; set; } = null!;
}
