using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class DetalleCotizacion
{
    public int IdDetalleCotizacion { get; set; }

    public int IdCotizacion { get; set; }

    public int IdElementoCatalogo { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public string? Observacion { get; set; }

    public virtual Cotizacion IdCotizacionNavigation { get; set; } = null!;

    public virtual ElementoCatalogo IdElementoCatalogoNavigation { get; set; } = null!;
}
