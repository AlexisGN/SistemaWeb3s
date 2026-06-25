using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class ConsultaExterna
{
    public int IdConsultaExterna { get; set; }

    public int IdTipoServicioExterno { get; set; }

    public int? IdUsuarioRegistro { get; set; }

    public int? IdCliente { get; set; }

    public string NumeroConsultado { get; set; } = null!;

    public DateTime FechaConsulta { get; set; }

    public bool Exitoso { get; set; }

    public string? ResultadoJson { get; set; }

    public string? MensajeError { get; set; }

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual TipoServicioExterno IdTipoServicioExternoNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioRegistroNavigation { get; set; }
}
