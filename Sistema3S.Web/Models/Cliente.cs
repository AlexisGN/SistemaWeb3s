using System;
using System.Collections.Generic;

namespace Sistema3S.Web.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public int? IdUsuario { get; set; }

    public int IdTipoCliente { get; set; }

    public int IdTipoDocumento { get; set; }

    public int? IdUbigeo { get; set; }

    public string NumeroDocumento { get; set; } = null!;

    public string? Correo { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ClienteEmpresa? ClienteEmpresa { get; set; }

    public virtual ClientePersonaNatural? ClientePersonaNatural { get; set; }

    public virtual ICollection<ConsultaExterna> ConsultaExterna { get; set; } = new List<ConsultaExterna>();

    public virtual ICollection<ContactoCliente> ContactoCliente { get; set; } = new List<ContactoCliente>();

    public virtual ICollection<Cotizacion> Cotizacion { get; set; } = new List<Cotizacion>();

    public virtual TipoCliente IdTipoClienteNavigation { get; set; } = null!;

    public virtual TipoDocumento IdTipoDocumentoNavigation { get; set; } = null!;

    public virtual Ubigeo? IdUbigeoNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
