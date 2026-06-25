using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public string Correo { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<AceptacionDocumentoLegal> AceptacionDocumentoLegal { get; set; } = new List<AceptacionDocumentoLegal>();

    public virtual ICollection<AuditoriaLog> AuditoriaLog { get; set; } = new List<AuditoriaLog>();

    public virtual ICollection<Caja> CajaIdUsuarioAperturaNavigation { get; set; } = new List<Caja>();

    public virtual ICollection<Caja> CajaIdUsuarioCierreNavigation { get; set; } = new List<Caja>();

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<Compra> Compra { get; set; } = new List<Compra>();

    public virtual ICollection<ConsultaExterna> ConsultaExterna { get; set; } = new List<ConsultaExterna>();

    public virtual ICollection<Cotizacion> CotizacionIdUsuarioAtencionNavigation { get; set; } = new List<Cotizacion>();

    public virtual ICollection<Cotizacion> CotizacionIdUsuarioRegistroNavigation { get; set; } = new List<Cotizacion>();

    public virtual ICollection<EnvioCorreo> EnvioCorreo { get; set; } = new List<EnvioCorreo>();

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual ICollection<MovimientoCaja> MovimientoCaja { get; set; } = new List<MovimientoCaja>();

    public virtual ICollection<MovimientoStock> MovimientoStock { get; set; } = new List<MovimientoStock>();

    public virtual PersonalInterno? PersonalInterno { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
