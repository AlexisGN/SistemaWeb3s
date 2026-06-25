using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class TipoDocumentoLegal
{
    public int IdTipoDocumentoLegal { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    public virtual ICollection<DocumentoLegal> DocumentoLegal { get; set; } = new List<DocumentoLegal>();
}
