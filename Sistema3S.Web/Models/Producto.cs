using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public int IdElementoCatalogo { get; set; }

    public int IdCategoria { get; set; }

    public int? IdMarca { get; set; }

    public int? IdUnidadMedida { get; set; }

    public string CodigoProducto { get; set; } = null!;

    public string? FichaTecnicaPdf { get; set; }

    public bool AplicaInventario { get; set; }

    public virtual ICollection<AlertaStock> AlertaStock { get; set; } = new List<AlertaStock>();

    public virtual ICollection<DetalleCompra> DetalleCompra { get; set; } = new List<DetalleCompra>();

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual ElementoCatalogo IdElementoCatalogoNavigation { get; set; } = null!;

    public virtual Marca? IdMarcaNavigation { get; set; }

    public virtual UnidadMedida? IdUnidadMedidaNavigation { get; set; }

    public virtual Inventario? Inventario { get; set; }

    public virtual ICollection<MovimientoStock> MovimientoStock { get; set; } = new List<MovimientoStock>();
}
