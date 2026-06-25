using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class MovimientoCaja
{
    public int IdMovimientoCaja { get; set; }

    public int IdCaja { get; set; }

    public int IdTipoMovimientoCaja { get; set; }

    public int? IdVenta { get; set; }

    public int? IdCompra { get; set; }

    public int IdUsuarioRegistro { get; set; }

    public decimal Monto { get; set; }

    public string? Descripcion { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public virtual Caja IdCajaNavigation { get; set; } = null!;

    public virtual Compra? IdCompraNavigation { get; set; }

    public virtual TipoMovimientoCaja IdTipoMovimientoCajaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioRegistroNavigation { get; set; } = null!;

    public virtual Venta? IdVentaNavigation { get; set; }
}
