using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class DocumentoLegal
{
    public int IdDocumentoLegal { get; set; }

    public int IdTipoDocumentoLegal { get; set; }

    public string Titulo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public string Version { get; set; } = null!;

    public DateTime FechaPublicacion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<AceptacionDocumentoLegal> AceptacionDocumentoLegal { get; set; } = new List<AceptacionDocumentoLegal>();

    public virtual TipoDocumentoLegal IdTipoDocumentoLegalNavigation { get; set; } = null!;
}
