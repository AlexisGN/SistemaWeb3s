using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Compra
{
    public int IdCompra { get; set; }

    public int IdProveedor { get; set; }

    public int IdUsuarioRegistro { get; set; }

    public int IdEstadoCompra { get; set; }

    public DateTime FechaCompra { get; set; }

    public decimal Total { get; set; }

    public virtual ICollection<DetalleCompra> DetalleCompra { get; set; } = new List<DetalleCompra>();

    public virtual EstadoCompra IdEstadoCompraNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioRegistroNavigation { get; set; } = null!;

    public virtual ICollection<MovimientoCaja> MovimientoCaja { get; set; } = new List<MovimientoCaja>();

    public virtual ICollection<MovimientoStock> MovimientoStock { get; set; } = new List<MovimientoStock>();
}
