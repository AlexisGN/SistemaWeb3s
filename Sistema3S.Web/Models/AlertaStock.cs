using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class AlertaStock
{
    public int IdAlertaStock { get; set; }

    public int IdProducto { get; set; }

    public int IdEstadoAlertaStock { get; set; }

    public DateTime FechaAlerta { get; set; }

    public string Mensaje { get; set; } = null!;

    public virtual EstadoAlertaStock IdEstadoAlertaStockNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
