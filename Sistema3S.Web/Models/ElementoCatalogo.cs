using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class ElementoCatalogo
{
    public int IdElementoCatalogo { get; set; }

    public int IdTipoElemento { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal? PrecioReferencial { get; set; }

    public string? ImagenUrl { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<DetalleCotizacion> DetalleCotizacion { get; set; } = new List<DetalleCotizacion>();

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual TipoElemento IdTipoElementoNavigation { get; set; } = null!;

    public virtual ICollection<ImagenElementoCatalogo> ImagenElementoCatalogo { get; set; } = new List<ImagenElementoCatalogo>();

    public virtual Producto? Producto { get; set; }

    public virtual Servicio? Servicio { get; set; }

    public virtual ICollection<SectorIndustrial> IdSectorIndustrial { get; set; } = new List<SectorIndustrial>();
}
