using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Venta
{
    public int IdVenta { get; set; }

    public int IdCliente { get; set; }

    public int? IdCotizacion { get; set; }

    public int IdUsuarioRegistro { get; set; }

    public int IdEstadoVenta { get; set; }

    public DateTime FechaVenta { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Igv { get; set; }

    public decimal Total { get; set; }

    public virtual Comprobante? Comprobante { get; set; }

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Cotizacion? IdCotizacionNavigation { get; set; }

    public virtual EstadoVenta IdEstadoVentaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioRegistroNavigation { get; set; } = null!;

    public virtual ICollection<MovimientoCaja> MovimientoCaja { get; set; } = new List<MovimientoCaja>();

    public virtual ICollection<MovimientoStock> MovimientoStock { get; set; } = new List<MovimientoStock>();
}
