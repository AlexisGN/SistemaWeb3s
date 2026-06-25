using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Models;

namespace Sistema3S.Web.Data;

public partial class Bd3sContext : DbContext
{
    public Bd3sContext()
    {
    }

    public Bd3sContext(DbContextOptions<Bd3sContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AceptacionDocumentoLegal> AceptacionDocumentoLegal { get; set; }

    public virtual DbSet<AlertaStock> AlertaStock { get; set; }

    public virtual DbSet<AuditoriaLog> AuditoriaLog { get; set; }

    public virtual DbSet<Caja> Caja { get; set; }

    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<Cliente> Cliente { get; set; }

    public virtual DbSet<ClienteEmpresa> ClienteEmpresa { get; set; }

    public virtual DbSet<ClientePersonaNatural> ClientePersonaNatural { get; set; }

    public virtual DbSet<Compra> Compra { get; set; }

    public virtual DbSet<Comprobante> Comprobante { get; set; }

    public virtual DbSet<ConsultaExterna> ConsultaExterna { get; set; }

    public virtual DbSet<ContactoCliente> ContactoCliente { get; set; }

    public virtual DbSet<ContactoProveedor> ContactoProveedor { get; set; }

    public virtual DbSet<Cotizacion> Cotizacion { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompra { get; set; }

    public virtual DbSet<DetalleCotizacion> DetalleCotizacion { get; set; }

    public virtual DbSet<DetalleVenta> DetalleVenta { get; set; }

    public virtual DbSet<DocumentoLegal> DocumentoLegal { get; set; }

    public virtual DbSet<ElementoCatalogo> ElementoCatalogo { get; set; }

    public virtual DbSet<Empresa> Empresa { get; set; }

    public virtual DbSet<EnvioCorreo> EnvioCorreo { get; set; }

    public virtual DbSet<EstadoAlertaStock> EstadoAlertaStock { get; set; }

    public virtual DbSet<EstadoCaja> EstadoCaja { get; set; }

    public virtual DbSet<EstadoCompra> EstadoCompra { get; set; }

    public virtual DbSet<EstadoComprobante> EstadoComprobante { get; set; }

    public virtual DbSet<EstadoCotizacion> EstadoCotizacion { get; set; }

    public virtual DbSet<EstadoVenta> EstadoVenta { get; set; }

    public virtual DbSet<ImagenElementoCatalogo> ImagenElementoCatalogo { get; set; }

    public virtual DbSet<Inventario> Inventario { get; set; }

    public virtual DbSet<Marca> Marca { get; set; }

    public virtual DbSet<MovimientoCaja> MovimientoCaja { get; set; }

    public virtual DbSet<MovimientoStock> MovimientoStock { get; set; }

    public virtual DbSet<Permiso> Permiso { get; set; }

    public virtual DbSet<PersonalInterno> PersonalInterno { get; set; }

    public virtual DbSet<Producto> Producto { get; set; }

    public virtual DbSet<Proveedor> Proveedor { get; set; }

    public virtual DbSet<Rol> Rol { get; set; }

    public virtual DbSet<SectorIndustrial> SectorIndustrial { get; set; }

    public virtual DbSet<SerieComprobante> SerieComprobante { get; set; }

    public virtual DbSet<Servicio> Servicio { get; set; }

    public virtual DbSet<TipoCliente> TipoCliente { get; set; }

    public virtual DbSet<TipoComprobante> TipoComprobante { get; set; }

    public virtual DbSet<TipoDocumento> TipoDocumento { get; set; }

    public virtual DbSet<TipoDocumentoLegal> TipoDocumentoLegal { get; set; }

    public virtual DbSet<TipoElemento> TipoElemento { get; set; }

    public virtual DbSet<TipoMovimientoCaja> TipoMovimientoCaja { get; set; }

    public virtual DbSet<TipoMovimientoStock> TipoMovimientoStock { get; set; }

    public virtual DbSet<TipoServicioExterno> TipoServicioExterno { get; set; }

    public virtual DbSet<Ubigeo> Ubigeo { get; set; }

    public virtual DbSet<UnidadMedida> UnidadMedida { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    public virtual DbSet<ValorCorporativo> ValorCorporativo { get; set; }

    public virtual DbSet<Venta> Venta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:BD_3S");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AceptacionDocumentoLegal>(entity =>
        {
            entity.HasKey(e => e.IdAceptacionDocumentoLegal).HasName("PK__Aceptaci__6018BFBC149E14FB");

            entity.HasIndex(e => new { e.IdUsuario, e.IdDocumentoLegal }, "UQ_AceptacionDocumentoLegal").IsUnique();

            entity.Property(e => e.FechaAceptacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IpAceptacion).HasMaxLength(50);

            entity.HasOne(d => d.IdDocumentoLegalNavigation).WithMany(p => p.AceptacionDocumentoLegal)
                .HasForeignKey(d => d.IdDocumentoLegal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AceptacionDocumentoLegal_DocumentoLegal");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AceptacionDocumentoLegal)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AceptacionDocumentoLegal_Usuario");
        });

        modelBuilder.Entity<AlertaStock>(entity =>
        {
            entity.HasKey(e => e.IdAlertaStock).HasName("PK__AlertaSt__34B26D8B8CDC32C0");

            entity.Property(e => e.FechaAlerta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Mensaje).HasMaxLength(300);

            entity.HasOne(d => d.IdEstadoAlertaStockNavigation).WithMany(p => p.AlertaStock)
                .HasForeignKey(d => d.IdEstadoAlertaStock)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AlertaStock_EstadoAlertaStock");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.AlertaStock)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AlertaStock_Producto");
        });

        modelBuilder.Entity<AuditoriaLog>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("PK__Auditori__0C54DBC6D1DB5626");

            entity.Property(e => e.Accion).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TablaAfectada).HasMaxLength(100);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AuditoriaLog)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_AuditoriaLog_Usuario");
        });

        modelBuilder.Entity<Caja>(entity =>
        {
            entity.HasKey(e => e.IdCaja).HasName("PK__Caja__3B7BF2C52ADE0B32");

            entity.Property(e => e.FechaApertura)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaCierre).HasColumnType("datetime");
            entity.Property(e => e.SaldoFinal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SaldoInicial).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdEstadoCajaNavigation).WithMany(p => p.Caja)
                .HasForeignKey(d => d.IdEstadoCaja)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Caja_EstadoCaja");

            entity.HasOne(d => d.IdUsuarioAperturaNavigation).WithMany(p => p.CajaIdUsuarioAperturaNavigation)
                .HasForeignKey(d => d.IdUsuarioApertura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Caja_UsuarioApertura");

            entity.HasOne(d => d.IdUsuarioCierreNavigation).WithMany(p => p.CajaIdUsuarioCierreNavigation)
                .HasForeignKey(d => d.IdUsuarioCierre)
                .HasConstraintName("FK_Caja_UsuarioCierre");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__A3C02A10DF6ADE5C");

            entity.HasIndex(e => e.Nombre, "UQ_Categoria_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__Cliente__D594664243A0CF53");

            entity.HasIndex(e => e.IdTipoCliente, "IX_Cliente_TipoCliente");

            entity.HasIndex(e => e.IdTipoDocumento, "IX_Cliente_TipoDocumento");

            entity.HasIndex(e => new { e.IdTipoDocumento, e.NumeroDocumento }, "UQ_Cliente_Documento").IsUnique();

            entity.HasIndex(e => e.IdUsuario, "UX_Cliente_IdUsuario")
                .IsUnique()
                .HasFilter("([IdUsuario] IS NOT NULL)");

            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NumeroDocumento).HasMaxLength(20);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.IdTipoClienteNavigation).WithMany(p => p.Cliente)
                .HasForeignKey(d => d.IdTipoCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_TipoCliente");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Cliente)
                .HasForeignKey(d => d.IdTipoDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_TipoDocumento");

            entity.HasOne(d => d.IdUbigeoNavigation).WithMany(p => p.Cliente)
                .HasForeignKey(d => d.IdUbigeo)
                .HasConstraintName("FK_Cliente_Ubigeo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Cliente)
                .HasForeignKey<Cliente>(d => d.IdUsuario)
                .HasConstraintName("FK_Cliente_Usuario");
        });

        modelBuilder.Entity<ClienteEmpresa>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__ClienteE__D5946642A89B68D5");

            entity.Property(e => e.IdCliente).ValueGeneratedNever();
            entity.Property(e => e.NombreComercial).HasMaxLength(200);
            entity.Property(e => e.RazonSocial).HasMaxLength(200);

            entity.HasOne(d => d.IdClienteNavigation).WithOne(p => p.ClienteEmpresa)
                .HasForeignKey<ClienteEmpresa>(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClienteEmpresa_Cliente");
        });

        modelBuilder.Entity<ClientePersonaNatural>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__ClienteP__D594664232BA4213");

            entity.Property(e => e.IdCliente).ValueGeneratedNever();
            entity.Property(e => e.ApellidoMaterno).HasMaxLength(100);
            entity.Property(e => e.ApellidoPaterno).HasMaxLength(100);
            entity.Property(e => e.Nombres).HasMaxLength(100);

            entity.HasOne(d => d.IdClienteNavigation).WithOne(p => p.ClientePersonaNatural)
                .HasForeignKey<ClientePersonaNatural>(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientePersonaNatural_Cliente");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra).HasName("PK__Compra__0A5CDB5C41CFD1D6");

            entity.HasIndex(e => e.IdEstadoCompra, "IX_Compra_Estado");

            entity.HasIndex(e => e.IdProveedor, "IX_Compra_Proveedor");

            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdEstadoCompraNavigation).WithMany(p => p.Compra)
                .HasForeignKey(d => d.IdEstadoCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_EstadoCompra");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Compra)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_Proveedor");

            entity.HasOne(d => d.IdUsuarioRegistroNavigation).WithMany(p => p.Compra)
                .HasForeignKey(d => d.IdUsuarioRegistro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_UsuarioRegistro");
        });

        modelBuilder.Entity<Comprobante>(entity =>
        {
            entity.HasKey(e => e.IdComprobante).HasName("PK__Comproba__BF4686EDB67B058D");

            entity.HasIndex(e => new { e.Serie, e.Numero }, "UQ_Comprobante_SerieNumero").IsUnique();

            entity.HasIndex(e => e.IdVenta, "UQ_Comprobante_Venta").IsUnique();

            entity.Property(e => e.ArchivoPdf).HasMaxLength(300);
            entity.Property(e => e.ArchivoXml).HasMaxLength(300);
            entity.Property(e => e.FechaEmision)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Numero).HasMaxLength(20);
            entity.Property(e => e.Serie).HasMaxLength(10);

            entity.HasOne(d => d.IdEstadoComprobanteNavigation).WithMany(p => p.Comprobante)
                .HasForeignKey(d => d.IdEstadoComprobante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comprobante_EstadoComprobante");

            entity.HasOne(d => d.IdSerieComprobanteNavigation).WithMany(p => p.Comprobante)
                .HasForeignKey(d => d.IdSerieComprobante)
                .HasConstraintName("FK_Comprobante_SerieComprobante");

            entity.HasOne(d => d.IdTipoComprobanteNavigation).WithMany(p => p.Comprobante)
                .HasForeignKey(d => d.IdTipoComprobante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comprobante_TipoComprobante");

            entity.HasOne(d => d.IdVentaNavigation).WithOne(p => p.Comprobante)
                .HasForeignKey<Comprobante>(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comprobante_Venta");
        });

        modelBuilder.Entity<ConsultaExterna>(entity =>
        {
            entity.HasKey(e => e.IdConsultaExterna).HasName("PK__Consulta__9EE2ECF1430C339F");

            entity.HasIndex(e => e.IdCliente, "IX_ConsultaExterna_Cliente");

            entity.HasIndex(e => e.IdTipoServicioExterno, "IX_ConsultaExterna_TipoServicio");

            entity.Property(e => e.FechaConsulta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MensajeError).HasMaxLength(500);
            entity.Property(e => e.NumeroConsultado).HasMaxLength(30);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ConsultaExterna)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK_ConsultaExterna_Cliente");

            entity.HasOne(d => d.IdTipoServicioExternoNavigation).WithMany(p => p.ConsultaExterna)
                .HasForeignKey(d => d.IdTipoServicioExterno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ConsultaExterna_TipoServicioExterno");

            entity.HasOne(d => d.IdUsuarioRegistroNavigation).WithMany(p => p.ConsultaExterna)
                .HasForeignKey(d => d.IdUsuarioRegistro)
                .HasConstraintName("FK_ConsultaExterna_UsuarioRegistro");
        });

        modelBuilder.Entity<ContactoCliente>(entity =>
        {
            entity.HasKey(e => e.IdContactoCliente).HasName("PK__Contacto__156F9F88E8A78E60");

            entity.Property(e => e.ApellidoMaterno).HasMaxLength(100);
            entity.Property(e => e.ApellidoPaterno).HasMaxLength(100);
            entity.Property(e => e.Cargo).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombres).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ContactoCliente)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContactoCliente_Cliente");
        });

        modelBuilder.Entity<ContactoProveedor>(entity =>
        {
            entity.HasKey(e => e.IdContactoProveedor).HasName("PK__Contacto__F3367065919CFA45");

            entity.Property(e => e.ApellidoMaterno).HasMaxLength(100);
            entity.Property(e => e.ApellidoPaterno).HasMaxLength(100);
            entity.Property(e => e.Cargo).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombres).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.ContactoProveedor)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContactoProveedor_Proveedor");
        });

        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.HasKey(e => e.IdCotizacion).HasName("PK__Cotizaci__9A6DA9EF8D49D72F");

            entity.HasIndex(e => e.IdCliente, "IX_Cotizacion_Cliente");

            entity.HasIndex(e => e.IdEstadoCotizacion, "IX_Cotizacion_Estado");

            entity.Property(e => e.ArchivoPdf).HasMaxLength(300);
            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FechaCotizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Igv).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Observacion).HasMaxLength(500);
            entity.Property(e => e.OrigenCotizacion)
                .HasMaxLength(30)
                .HasDefaultValue("Manual", "DF_Cotizacion_OrigenCotizacion");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalReferencial).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cotizacion)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cotizacion_Cliente");

            entity.HasOne(d => d.IdEstadoCotizacionNavigation).WithMany(p => p.Cotizacion)
                .HasForeignKey(d => d.IdEstadoCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cotizacion_EstadoCotizacion");

            entity.HasOne(d => d.IdUsuarioAtencionNavigation).WithMany(p => p.CotizacionIdUsuarioAtencionNavigation)
                .HasForeignKey(d => d.IdUsuarioAtencion)
                .HasConstraintName("FK_Cotizacion_UsuarioAtencion");

            entity.HasOne(d => d.IdUsuarioRegistroNavigation).WithMany(p => p.CotizacionIdUsuarioRegistroNavigation)
                .HasForeignKey(d => d.IdUsuarioRegistro)
                .HasConstraintName("FK_Cotizacion_UsuarioRegistro");
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCompra).HasName("PK__DetalleC__E046CCBB701B985B");

            entity.Property(e => e.PrecioCompra).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.DetalleCompra)
                .HasForeignKey(d => d.IdCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Compra");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleCompra)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Producto");
        });

        modelBuilder.Entity<DetalleCotizacion>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCotizacion).HasName("PK__DetalleC__952247792C7E0879");

            entity.Property(e => e.Observacion).HasMaxLength(300);
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.DetalleCotizacion)
                .HasForeignKey(d => d.IdCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCotizacion_Cotizacion");

            entity.HasOne(d => d.IdElementoCatalogoNavigation).WithMany(p => p.DetalleCotizacion)
                .HasForeignKey(d => d.IdElementoCatalogo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCotizacion_ElementoCatalogo");
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.IdDetalleVenta).HasName("PK__DetalleV__AAA5CEC244A0DFFF");

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdElementoCatalogoNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdElementoCatalogo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVenta_ElementoCatalogo");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVenta_Venta");
        });

        modelBuilder.Entity<DocumentoLegal>(entity =>
        {
            entity.HasKey(e => e.IdDocumentoLegal).HasName("PK__Document__B2373A9E10D10420");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaPublicacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Titulo).HasMaxLength(150);
            entity.Property(e => e.Version).HasMaxLength(20);

            entity.HasOne(d => d.IdTipoDocumentoLegalNavigation).WithMany(p => p.DocumentoLegal)
                .HasForeignKey(d => d.IdTipoDocumentoLegal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentoLegal_TipoDocumentoLegal");
        });

        modelBuilder.Entity<ElementoCatalogo>(entity =>
        {
            entity.HasKey(e => e.IdElementoCatalogo).HasName("PK__Elemento__DC418170E174579F");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImagenUrl).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.PrecioReferencial).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdTipoElementoNavigation).WithMany(p => p.ElementoCatalogo)
                .HasForeignKey(d => d.IdTipoElemento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ElementoCatalogo_TipoElemento");

            entity.HasMany(d => d.IdSectorIndustrial).WithMany(p => p.IdElementoCatalogo)
                .UsingEntity<Dictionary<string, object>>(
                    "ElementoCatalogoSector",
                    r => r.HasOne<SectorIndustrial>().WithMany()
                        .HasForeignKey("IdSectorIndustrial")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ElementoCatalogoSector_SectorIndustrial"),
                    l => l.HasOne<ElementoCatalogo>().WithMany()
                        .HasForeignKey("IdElementoCatalogo")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ElementoCatalogoSector_ElementoCatalogo"),
                    j =>
                    {
                        j.HasKey("IdElementoCatalogo", "IdSectorIndustrial");
                    });
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.IdEmpresa).HasName("PK__Empresa__5EF4033E98F8F9AA");

            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.NombreComercial).HasMaxLength(200);
            entity.Property(e => e.RazonSocial).HasMaxLength(200);
            entity.Property(e => e.Rubro).HasMaxLength(200);
            entity.Property(e => e.Ruc).HasMaxLength(20);
            entity.Property(e => e.SitioWeb).HasMaxLength(200);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<EnvioCorreo>(entity =>
        {
            entity.HasKey(e => e.IdEnvioCorreo).HasName("PK__EnvioCor__7DE6FD77646C0F75");

            entity.HasIndex(e => e.IdComprobante, "IX_EnvioCorreo_Comprobante");

            entity.HasIndex(e => e.IdCotizacion, "IX_EnvioCorreo_Cotizacion");

            entity.Property(e => e.Asunto).HasMaxLength(200);
            entity.Property(e => e.Destinatario).HasMaxLength(150);
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MensajeError).HasMaxLength(500);

            entity.HasOne(d => d.IdComprobanteNavigation).WithMany(p => p.EnvioCorreo)
                .HasForeignKey(d => d.IdComprobante)
                .HasConstraintName("FK_EnvioCorreo_Comprobante");

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.EnvioCorreo)
                .HasForeignKey(d => d.IdCotizacion)
                .HasConstraintName("FK_EnvioCorreo_Cotizacion");

            entity.HasOne(d => d.IdUsuarioRegistroNavigation).WithMany(p => p.EnvioCorreo)
                .HasForeignKey(d => d.IdUsuarioRegistro)
                .HasConstraintName("FK_EnvioCorreo_UsuarioRegistro");
        });

        modelBuilder.Entity<EstadoAlertaStock>(entity =>
        {
            entity.HasKey(e => e.IdEstadoAlertaStock).HasName("PK__EstadoAl__8AA649B489708AAC");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoAlertaStock_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<EstadoCaja>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCaja).HasName("PK__EstadoCa__D80D5A5D1CBECBC6");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoCaja_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<EstadoCompra>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCompra).HasName("PK__EstadoCo__0226D5A38D8851DD");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoCompra_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<EstadoComprobante>(entity =>
        {
            entity.HasKey(e => e.IdEstadoComprobante).HasName("PK__EstadoCo__96E183164FD39FF5");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoComprobante_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<EstadoCotizacion>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCotizacion).HasName("PK__EstadoCo__22ACB0A642792A6B");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoCotizacion_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<EstadoVenta>(entity =>
        {
            entity.HasKey(e => e.IdEstadoVenta).HasName("PK__EstadoVe__777FD9609CC9180A");

            entity.HasIndex(e => e.Nombre, "UQ_EstadoVenta_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<ImagenElementoCatalogo>(entity =>
        {
            entity.HasKey(e => e.IdImagenElementoCatalogo).HasName("PK__ImagenEl__A659BAF2A11BFC41");

            entity.HasIndex(e => e.IdElementoCatalogo, "IX_ImagenElementoCatalogo_Elemento");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TextoAlternativo).HasMaxLength(150);
            entity.Property(e => e.UrlImagen).HasMaxLength(300);

            entity.HasOne(d => d.IdElementoCatalogoNavigation).WithMany(p => p.ImagenElementoCatalogo)
                .HasForeignKey(d => d.IdElementoCatalogo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImagenElementoCatalogo_ElementoCatalogo");
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.IdInventario).HasName("PK__Inventar__1927B20CB12A3A65");

            entity.HasIndex(e => e.IdProducto, "IX_Inventario_Producto");

            entity.HasIndex(e => e.IdProducto, "UQ_Inventario_Producto").IsUnique();

            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdProductoNavigation).WithOne(p => p.Inventario)
                .HasForeignKey<Inventario>(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventario_Producto");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("PK__Marca__4076A8874C7771E1");

            entity.HasIndex(e => e.Nombre, "UQ_Marca_Nombre").IsUnique();

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.LogoUrl).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<MovimientoCaja>(entity =>
        {
            entity.HasKey(e => e.IdMovimientoCaja).HasName("PK__Movimien__D126CD2D78802869");

            entity.HasIndex(e => e.IdCaja, "IX_MovimientoCaja_Caja");

            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCajaNavigation).WithMany(p => p.MovimientoCaja)
                .HasForeignKey(d => d.IdCaja)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoCaja_Caja");

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.MovimientoCaja)
                .HasForeignKey(d => d.IdCompra)
                .HasConstraintName("FK_MovimientoCaja_Compra");

            entity.HasOne(d => d.IdTipoMovimientoCajaNavigation).WithMany(p => p.MovimientoCaja)
                .HasForeignKey(d => d.IdTipoMovimientoCaja)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoCaja_TipoMovimientoCaja");

            entity.HasOne(d => d.IdUsuarioRegistroNavigation).WithMany(p => p.MovimientoCaja)
                .HasForeignKey(d => d.IdUsuarioRegistro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoCaja_UsuarioRegistro");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.MovimientoCaja)
                .HasForeignKey(d => d.IdVenta)
                .HasConstraintName("FK_MovimientoCaja_Venta");
        });

        modelBuilder.Entity<MovimientoStock>(entity =>
        {
            entity.HasKey(e => e.IdMovimientoStock).HasName("PK__Movimien__EAF68814C696D3BB");

            entity.HasIndex(e => e.IdProducto, "IX_MovimientoStock_Producto");

            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Motivo).HasMaxLength(300);

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.MovimientoStock)
                .HasForeignKey(d => d.IdCompra)
                .HasConstraintName("FK_MovimientoStock_Compra");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.MovimientoStock)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoStock_Producto");

            entity.HasOne(d => d.IdTipoMovimientoStockNavigation).WithMany(p => p.MovimientoStock)
                .HasForeignKey(d => d.IdTipoMovimientoStock)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoStock_TipoMovimientoStock");

            entity.HasOne(d => d.IdUsuarioRegistroNavigation).WithMany(p => p.MovimientoStock)
                .HasForeignKey(d => d.IdUsuarioRegistro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoStock_UsuarioRegistro");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.MovimientoStock)
                .HasForeignKey(d => d.IdVenta)
                .HasConstraintName("FK_MovimientoStock_Venta");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("PK__Permiso__0D626EC8B25F4ED4");

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<PersonalInterno>(entity =>
        {
            entity.HasKey(e => e.IdPersonalInterno).HasName("PK__Personal__201A1647CC338A97");

            entity.HasIndex(e => e.IdUsuario, "UQ_PersonalInterno_Usuario").IsUnique();

            entity.Property(e => e.ApellidoMaterno).HasMaxLength(100);
            entity.Property(e => e.ApellidoPaterno).HasMaxLength(100);
            entity.Property(e => e.Cargo).HasMaxLength(100);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombres).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.PersonalInterno)
                .HasForeignKey<PersonalInterno>(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PersonalInterno_Usuario");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__Producto__09889210485167C9");

            entity.HasIndex(e => e.IdCategoria, "IX_Producto_Categoria");

            entity.HasIndex(e => e.IdMarca, "IX_Producto_Marca");

            entity.HasIndex(e => e.CodigoProducto, "UQ_Producto_Codigo").IsUnique();

            entity.HasIndex(e => e.IdElementoCatalogo, "UQ_Producto_ElementoCatalogo").IsUnique();

            entity.Property(e => e.AplicaInventario).HasDefaultValue(true);
            entity.Property(e => e.CodigoProducto).HasMaxLength(50);
            entity.Property(e => e.FichaTecnicaPdf).HasMaxLength(300);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Producto)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_Categoria");

            entity.HasOne(d => d.IdElementoCatalogoNavigation).WithOne(p => p.Producto)
                .HasForeignKey<Producto>(d => d.IdElementoCatalogo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_ElementoCatalogo");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Producto)
                .HasForeignKey(d => d.IdMarca)
                .HasConstraintName("FK_Producto_Marca");

            entity.HasOne(d => d.IdUnidadMedidaNavigation).WithMany(p => p.Producto)
                .HasForeignKey(d => d.IdUnidadMedida)
                .HasConstraintName("FK_Producto_UnidadMedida");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__Proveedo__E8B631AFBF8AA7E2");

            entity.HasIndex(e => e.Ruc, "UQ_Proveedor_Ruc").IsUnique();

            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.NombreComercial).HasMaxLength(200);
            entity.Property(e => e.RazonSocial).HasMaxLength(200);
            entity.Property(e => e.Ruc).HasMaxLength(20);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.IdUbigeoNavigation).WithMany(p => p.Proveedor)
                .HasForeignKey(d => d.IdUbigeo)
                .HasConstraintName("FK_Proveedor_Ubigeo");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__2A49584C5014AFB4");

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);

            entity.HasMany(d => d.IdPermiso).WithMany(p => p.IdRol)
                .UsingEntity<Dictionary<string, object>>(
                    "RolPermiso",
                    r => r.HasOne<Permiso>().WithMany()
                        .HasForeignKey("IdPermiso")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolPermiso_Permiso"),
                    l => l.HasOne<Rol>().WithMany()
                        .HasForeignKey("IdRol")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolPermiso_Rol"),
                    j =>
                    {
                        j.HasKey("IdRol", "IdPermiso");
                    });
        });

        modelBuilder.Entity<SectorIndustrial>(entity =>
        {
            entity.HasKey(e => e.IdSectorIndustrial).HasName("PK__SectorIn__7F04809D048E7AFB");

            entity.HasIndex(e => e.Nombre, "UQ_SectorIndustrial_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<SerieComprobante>(entity =>
        {
            entity.HasKey(e => e.IdSerieComprobante).HasName("PK__SerieCom__EB3327C51EBAB2AE");

            entity.HasIndex(e => new { e.IdTipoComprobante, e.Serie }, "UQ_SerieComprobante").IsUnique();

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Serie).HasMaxLength(10);

            entity.HasOne(d => d.IdTipoComprobanteNavigation).WithMany(p => p.SerieComprobante)
                .HasForeignKey(d => d.IdTipoComprobante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SerieComprobante_TipoComprobante");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PK__Servicio__2DCCF9A22346937C");

            entity.HasIndex(e => e.IdElementoCatalogo, "UQ_Servicio_ElementoCatalogo").IsUnique();

            entity.Property(e => e.MensajeWhatsApp).HasMaxLength(500);
            entity.Property(e => e.SectorAplicacion).HasMaxLength(200);

            entity.HasOne(d => d.IdElementoCatalogoNavigation).WithOne(p => p.Servicio)
                .HasForeignKey<Servicio>(d => d.IdElementoCatalogo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Servicio_ElementoCatalogo");
        });

        modelBuilder.Entity<TipoCliente>(entity =>
        {
            entity.HasKey(e => e.IdTipoCliente).HasName("PK__TipoClie__F173C7FAD73AE3AE");

            entity.HasIndex(e => e.Nombre, "UQ_TipoCliente_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoComprobante>(entity =>
        {
            entity.HasKey(e => e.IdTipoComprobante).HasName("PK__TipoComp__F8411DEBBCCC17B1");

            entity.HasIndex(e => e.Nombre, "UQ_TipoComprobante_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoDocumento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumento).HasName("PK__TipoDocu__3AB3332FD460E032");

            entity.HasIndex(e => e.Nombre, "UQ_TipoDocumento_Nombre").IsUnique();

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(30);
        });

        modelBuilder.Entity<TipoDocumentoLegal>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumentoLegal).HasName("PK__TipoDocu__3A6AA1F3D87607CE");

            entity.HasIndex(e => e.Nombre, "UQ_TipoDocumentoLegal_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<TipoElemento>(entity =>
        {
            entity.HasKey(e => e.IdTipoElemento).HasName("PK__TipoElem__B5E6A65D412C9BFC");

            entity.HasIndex(e => e.Nombre, "UQ_TipoElemento_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoMovimientoCaja>(entity =>
        {
            entity.HasKey(e => e.IdTipoMovimientoCaja).HasName("PK__TipoMovi__4E1408255EC04946");

            entity.HasIndex(e => e.Nombre, "UQ_TipoMovimientoCaja_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoMovimientoStock>(entity =>
        {
            entity.HasKey(e => e.IdTipoMovimientoStock).HasName("PK__TipoMovi__762AF45D3707B29A");

            entity.HasIndex(e => e.Nombre, "UQ_TipoMovimientoStock_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoServicioExterno>(entity =>
        {
            entity.HasKey(e => e.IdTipoServicioExterno).HasName("PK__TipoServ__BD15D3EBD68A2B8B");

            entity.HasIndex(e => e.Nombre, "UQ_TipoServicioExterno_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Ubigeo>(entity =>
        {
            entity.HasKey(e => e.IdUbigeo).HasName("PK__Ubigeo__F682D161440E469D");

            entity.HasIndex(e => new { e.Departamento, e.Provincia, e.Distrito }, "IX_Ubigeo_Busqueda");

            entity.HasIndex(e => e.CodigoUbigeo, "UQ_Ubigeo_CodigoUbigeo")
                .IsUnique()
                .HasFilter("([CodigoUbigeo] IS NOT NULL)");

            entity.Property(e => e.CodigoUbigeo).HasMaxLength(6);
            entity.Property(e => e.Departamento).HasMaxLength(100);
            entity.Property(e => e.Distrito).HasMaxLength(100);
            entity.Property(e => e.Provincia).HasMaxLength(100);
        });

        modelBuilder.Entity<UnidadMedida>(entity =>
        {
            entity.HasKey(e => e.IdUnidadMedida).HasName("PK__UnidadMe__18F83A93874E081F");

            entity.HasIndex(e => e.Nombre, "UQ_UnidadMedida_Nombre").IsUnique();

            entity.Property(e => e.Abreviatura).HasMaxLength(20);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(80);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__5B65BF9713488F8A");

            entity.HasIndex(e => e.IdRol, "IX_Usuario_Rol");

            entity.HasIndex(e => e.Correo, "UQ_Usuario_Correo").IsUnique();

            entity.Property(e => e.ContrasenaHash).HasMaxLength(255);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuario)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        modelBuilder.Entity<ValorCorporativo>(entity =>
        {
            entity.HasKey(e => e.IdValor).HasName("PK__ValorCor__D74976D3B5822E3E");

            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.ValorCorporativo)
                .HasForeignKey(d => d.IdEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ValorCorporativo_Empresa");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PK__Venta__BC1240BDB9CEFCB2");

            entity.HasIndex(e => e.IdCliente, "IX_Venta_Cliente");

            entity.HasIndex(e => e.IdEstadoVenta, "IX_Venta_Estado");

            entity.Property(e => e.FechaVenta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Igv).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Cliente");

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCotizacion)
                .HasConstraintName("FK_Venta_Cotizacion");

            entity.HasOne(d => d.IdEstadoVentaNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdEstadoVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_EstadoVenta");

            entity.HasOne(d => d.IdUsuarioRegistroNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuarioRegistro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_UsuarioRegistro");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
