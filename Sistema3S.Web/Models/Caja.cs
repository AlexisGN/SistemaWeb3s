using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Caja
{
    public int IdCaja { get; set; }

    public int IdUsuarioApertura { get; set; }

    public int? IdUsuarioCierre { get; set; }

    public int IdEstadoCaja { get; set; }

    public DateTime FechaApertura { get; set; }

    public DateTime? FechaCierre { get; set; }

    public decimal SaldoInicial { get; set; }

    public decimal? SaldoFinal { get; set; }

    public virtual EstadoCaja IdEstadoCajaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioAperturaNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioCierreNavigation { get; set; }

    public virtual ICollection<MovimientoCaja> MovimientoCaja { get; set; } = new List<MovimientoCaja>();
}
