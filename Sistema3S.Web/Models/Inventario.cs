using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Inventario
{
    public int IdInventario { get; set; }

    public int IdProducto { get; set; }

    public int StockActual { get; set; }

    public int StockMinimo { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
