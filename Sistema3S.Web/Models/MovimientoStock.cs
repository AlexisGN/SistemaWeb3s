using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class MovimientoStock
{
    public int IdMovimientoStock { get; set; }

    public int IdProducto { get; set; }

    public int IdUsuarioRegistro { get; set; }

    public int IdTipoMovimientoStock { get; set; }

    public int? IdVenta { get; set; }

    public int? IdCompra { get; set; }

    public int Cantidad { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public string? Motivo { get; set; }

    public virtual Compra? IdCompraNavigation { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual TipoMovimientoStock IdTipoMovimientoStockNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioRegistroNavigation { get; set; } = null!;

    public virtual Venta? IdVentaNavigation { get; set; }
}
