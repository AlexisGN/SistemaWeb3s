using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class EnvioCorreo
{
    public int IdEnvioCorreo { get; set; }

    public int? IdCotizacion { get; set; }

    public int? IdComprobante { get; set; }

    public int? IdUsuarioRegistro { get; set; }

    public string Destinatario { get; set; } = null!;

    public string Asunto { get; set; } = null!;

    public string? Cuerpo { get; set; }

    public DateTime FechaEnvio { get; set; }

    public bool Exitoso { get; set; }

    public string? MensajeError { get; set; }

    public virtual Comprobante? IdComprobanteNavigation { get; set; }

    public virtual Cotizacion? IdCotizacionNavigation { get; set; }

    public virtual Usuario? IdUsuarioRegistroNavigation { get; set; }
}
