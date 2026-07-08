using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Publico;
using Sistema3S.Web.Models;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class PublicoService : IPublicoService
    {
        private readonly Bd3sContext _context;

        public PublicoService(Bd3sContext context)
        {
            _context = context;
        }

        public async Task<InicioPublicoDto> ObtenerInicioAsync()
        {
            var categorias = await ObtenerCategoriasPublicasAsync();
            var marcas = await ObtenerMarcasPublicasAsync();
            var productosNuevos = await ObtenerProductosNuevosAsync();
            var productos = await ObtenerProductosInicioAsync();
            var servicios = await ObtenerServiciosInicioAsync();

            return new InicioPublicoDto
            {
                Categorias = categorias,
                Marcas = marcas,
                ProductosNuevos = productosNuevos,
                Productos = productos,
                Servicios = servicios
            };
        }

        public async Task<List<CategoriaPublicaDto>> ObtenerCategoriasPublicasAsync()
        {
            var categorias = await _context.Categoria
                .AsNoTracking()
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    c.IdCategoria,
                    c.Nombre,
                    c.Descripcion,
                    CantidadProductos = c.Producto.Count(p =>
                        p.IdElementoCatalogoNavigation.Estado &&
                        (
                            p.IdMarcaNavigation == null ||
                            p.IdMarcaNavigation.Estado
                        )
                    )
                })
                .ToListAsync();

            return categorias.Select(c => new CategoriaPublicaDto
            {
                Id = c.IdCategoria,
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion ?? "Productos y soluciones industriales disponibles para cotización.",
                Icono = ObtenerIconoCategoria(c.Nombre),
                CantidadProductos = c.CantidadProductos
            }).ToList();
        }

        public async Task<List<MarcaPublicaDto>> ObtenerMarcasPublicasAsync()
        {
            return await _context.Marca
                .AsNoTracking()
                .Where(m => m.Estado)
                .OrderBy(m => m.Nombre)
                .Select(m => new MarcaPublicaDto
                {
                    Id = m.IdMarca,
                    IdMarca = m.IdMarca,
                    Nombre = m.Nombre,
                    LogoUrl = m.LogoUrl ?? string.Empty,
                    CantidadProductos = m.Producto.Count(p =>
                        p.IdElementoCatalogoNavigation.Estado &&
                        p.IdCategoriaNavigation.Estado
                    )
                })
                .ToListAsync();
        }

        public async Task<ProductoPublicoListadoDto> ObtenerProductosAsync(
            string? busqueda,
            int? idCategoria,
            int? idMarca,
            int pagina,
            int tamanioPagina)
        {
            pagina = pagina <= 0 ? 1 : pagina;
            tamanioPagina = tamanioPagina <= 0 ? 24 : tamanioPagina;
            tamanioPagina = tamanioPagina > 60 ? 60 : tamanioPagina;

            var query = ConsultaProductosBase();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(p =>
                    EF.Functions.Like(p.IdElementoCatalogoNavigation.Nombre, $"%{texto}%") ||
                    EF.Functions.Like(p.CodigoProducto, $"%{texto}%") ||
                    EF.Functions.Like(p.IdCategoriaNavigation.Nombre, $"%{texto}%") ||
                    (
                        p.IdMarcaNavigation != null &&
                        EF.Functions.Like(p.IdMarcaNavigation.Nombre, $"%{texto}%")
                    )
                );
            }

            if (idCategoria.HasValue && idCategoria.Value > 0)
            {
                query = query.Where(p => p.IdCategoria == idCategoria.Value);
            }

            if (idMarca.HasValue && idMarca.Value > 0)
            {
                query = query.Where(p => p.IdMarca == idMarca.Value);
            }

            var totalRegistros = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.IdCategoriaNavigation.Nombre)
                .ThenBy(p => p.IdElementoCatalogoNavigation.Nombre)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(p => new ProductoPublicoDto
                {
                    Id = p.IdProducto,
                    IdProducto = p.IdProducto,
                    IdElementoCatalogo = p.IdElementoCatalogo,
                    IdCategoria = p.IdCategoria,
                    IdMarca = p.IdMarca,

                    Codigo = p.CodigoProducto,
                    Nombre = p.IdElementoCatalogoNavigation.Nombre,
                    Categoria = p.IdCategoriaNavigation.Nombre,
                    Marca = p.IdMarcaNavigation != null ? p.IdMarcaNavigation.Nombre : null,
                    Descripcion = p.IdElementoCatalogoNavigation.Descripcion ?? string.Empty,

                    ImagenUrl =
                        p.IdElementoCatalogoNavigation.ImagenElementoCatalogo
                            .Where(i => i.Estado)
                            .OrderByDescending(i => i.EsPrincipal)
                            .ThenByDescending(i => i.IdImagenElementoCatalogo)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                        ?? p.IdElementoCatalogoNavigation.ImagenUrl
                        ?? string.Empty,

                    Nuevo = false,
                    TieneFichaTecnica = p.FichaTecnicaPdf != null && p.FichaTecnicaPdf != "",
                    FichaTecnicaPdf = p.FichaTecnicaPdf
                })
                .ToListAsync();

            var totalPaginas = totalRegistros == 0
                ? 0
                : (int)Math.Ceiling(totalRegistros / (double)tamanioPagina);

            return new ProductoPublicoListadoDto
            {
                Items = items,
                TotalRegistros = totalRegistros,
                Pagina = pagina,
                TamanioPagina = tamanioPagina,
                TotalPaginas = totalPaginas,
                HayMas = pagina < totalPaginas
            };
        }

        public async Task<ProductoDetallePublicoDto?> ObtenerProductoDetalleAsync(int idProducto)
        {
            return await ConsultaProductosBase()
                .Where(p => p.IdProducto == idProducto)
                .Select(p => new ProductoDetallePublicoDto
                {
                    Id = p.IdProducto,
                    IdProducto = p.IdProducto,
                    IdElementoCatalogo = p.IdElementoCatalogo,
                    IdCategoria = p.IdCategoria,
                    IdMarca = p.IdMarca,

                    Codigo = p.CodigoProducto,
                    Nombre = p.IdElementoCatalogoNavigation.Nombre,
                    Categoria = p.IdCategoriaNavigation.Nombre,
                    Marca = p.IdMarcaNavigation != null ? p.IdMarcaNavigation.Nombre : null,
                    Descripcion = p.IdElementoCatalogoNavigation.Descripcion ?? string.Empty,

                    ImagenUrl =
                        p.IdElementoCatalogoNavigation.ImagenElementoCatalogo
                            .Where(i => i.Estado)
                            .OrderByDescending(i => i.EsPrincipal)
                            .ThenByDescending(i => i.IdImagenElementoCatalogo)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                        ?? p.IdElementoCatalogoNavigation.ImagenUrl
                        ?? string.Empty,

                    Nuevo = false,
                    TieneFichaTecnica = p.FichaTecnicaPdf != null && p.FichaTecnicaPdf != "",
                    FichaTecnicaPdf = p.FichaTecnicaPdf,

                    Imagenes = p.IdElementoCatalogoNavigation.ImagenElementoCatalogo
                        .Where(i => i.Estado)
                        .OrderByDescending(i => i.EsPrincipal)
                        .ThenByDescending(i => i.IdImagenElementoCatalogo)
                        .Select(i => new ImagenPublicaDto
                        {
                            IdImagen = i.IdImagenElementoCatalogo,
                            UrlImagen = i.UrlImagen,
                            TextoAlternativo = i.TextoAlternativo ?? p.IdElementoCatalogoNavigation.Nombre,
                            EsPrincipal = i.EsPrincipal
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<ServicioPublicoDto>> ObtenerServiciosPublicosAsync()
        {
            return await ConsultaServiciosBase()
                .OrderBy(s => s.IdElementoCatalogoNavigation.Nombre)
                .Select(s => new ServicioPublicoDto
                {
                    Id = s.IdServicio,
                    IdServicio = s.IdServicio,
                    IdElementoCatalogo = s.IdElementoCatalogo,

                    Nombre = s.IdElementoCatalogoNavigation.Nombre,
                    Descripcion = s.IdElementoCatalogoNavigation.Descripcion ?? string.Empty,
                    SectorAplicacion = s.SectorAplicacion,
                    MensajeWhatsApp = s.MensajeWhatsApp,
                    RequiereVisitaTecnica = s.RequiereVisitaTecnica,

                    ImagenUrl =
                        s.IdElementoCatalogoNavigation.ImagenElementoCatalogo
                            .Where(i => i.Estado)
                            .OrderByDescending(i => i.EsPrincipal)
                            .ThenByDescending(i => i.IdImagenElementoCatalogo)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                        ?? s.IdElementoCatalogoNavigation.ImagenUrl
                        ?? string.Empty
                })
                .ToListAsync();
        }

        public async Task<ServicioDetallePublicoDto?> ObtenerServicioDetalleAsync(int idServicio)
        {
            return await ConsultaServiciosBase()
                .Where(s => s.IdServicio == idServicio)
                .Select(s => new ServicioDetallePublicoDto
                {
                    Id = s.IdServicio,
                    IdServicio = s.IdServicio,
                    IdElementoCatalogo = s.IdElementoCatalogo,

                    Nombre = s.IdElementoCatalogoNavigation.Nombre,
                    Descripcion = s.IdElementoCatalogoNavigation.Descripcion ?? string.Empty,
                    SectorAplicacion = s.SectorAplicacion,
                    MensajeWhatsApp = s.MensajeWhatsApp,
                    RequiereVisitaTecnica = s.RequiereVisitaTecnica,

                    ImagenUrl =
                        s.IdElementoCatalogoNavigation.ImagenElementoCatalogo
                            .Where(i => i.Estado)
                            .OrderByDescending(i => i.EsPrincipal)
                            .ThenByDescending(i => i.IdImagenElementoCatalogo)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                        ?? s.IdElementoCatalogoNavigation.ImagenUrl
                        ?? string.Empty,

                    Imagenes = s.IdElementoCatalogoNavigation.ImagenElementoCatalogo
                        .Where(i => i.Estado)
                        .OrderByDescending(i => i.EsPrincipal)
                        .ThenByDescending(i => i.IdImagenElementoCatalogo)
                        .Select(i => new ImagenPublicaDto
                        {
                            IdImagen = i.IdImagenElementoCatalogo,
                            UrlImagen = i.UrlImagen,
                            TextoAlternativo = i.TextoAlternativo ?? s.IdElementoCatalogoNavigation.Nombre,
                            EsPrincipal = i.EsPrincipal
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        private async Task<List<ProductoPublicoDto>> ObtenerProductosNuevosAsync()
        {
            return await ConsultaProductosBase()
                .OrderByDescending(p => p.IdElementoCatalogoNavigation.FechaRegistro)
                .ThenByDescending(p => p.IdProducto)
                .Take(8)
                .Select(p => new ProductoPublicoDto
                {
                    Id = p.IdProducto,
                    IdProducto = p.IdProducto,
                    IdElementoCatalogo = p.IdElementoCatalogo,
                    IdCategoria = p.IdCategoria,
                    IdMarca = p.IdMarca,

                    Codigo = p.CodigoProducto,
                    Nombre = p.IdElementoCatalogoNavigation.Nombre,
                    Categoria = p.IdCategoriaNavigation.Nombre,
                    Marca = p.IdMarcaNavigation != null ? p.IdMarcaNavigation.Nombre : null,
                    Descripcion = p.IdElementoCatalogoNavigation.Descripcion ?? string.Empty,

                    ImagenUrl =
                        p.IdElementoCatalogoNavigation.ImagenElementoCatalogo
                            .Where(i => i.Estado)
                            .OrderByDescending(i => i.EsPrincipal)
                            .ThenByDescending(i => i.IdImagenElementoCatalogo)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                        ?? p.IdElementoCatalogoNavigation.ImagenUrl
                        ?? string.Empty,

                    Nuevo = true,
                    TieneFichaTecnica = p.FichaTecnicaPdf != null && p.FichaTecnicaPdf != "",
                    FichaTecnicaPdf = p.FichaTecnicaPdf
                })
                .ToListAsync();
        }

        private async Task<List<ProductoPublicoDto>> ObtenerProductosInicioAsync()
        {
            return await ConsultaProductosBase()
                .OrderBy(p => p.IdCategoriaNavigation.Nombre)
                .ThenByDescending(p => p.IdElementoCatalogoNavigation.FechaRegistro)
                .ThenByDescending(p => p.IdProducto)
                .Take(80)
                .Select(p => new ProductoPublicoDto
                {
                    Id = p.IdProducto,
                    IdProducto = p.IdProducto,
                    IdElementoCatalogo = p.IdElementoCatalogo,
                    IdCategoria = p.IdCategoria,
                    IdMarca = p.IdMarca,

                    Codigo = p.CodigoProducto,
                    Nombre = p.IdElementoCatalogoNavigation.Nombre,
                    Categoria = p.IdCategoriaNavigation.Nombre,
                    Marca = p.IdMarcaNavigation != null ? p.IdMarcaNavigation.Nombre : null,
                    Descripcion = p.IdElementoCatalogoNavigation.Descripcion ?? string.Empty,

                    ImagenUrl =
                        p.IdElementoCatalogoNavigation.ImagenElementoCatalogo
                            .Where(i => i.Estado)
                            .OrderByDescending(i => i.EsPrincipal)
                            .ThenByDescending(i => i.IdImagenElementoCatalogo)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                        ?? p.IdElementoCatalogoNavigation.ImagenUrl
                        ?? string.Empty,

                    Nuevo = false,
                    TieneFichaTecnica = p.FichaTecnicaPdf != null && p.FichaTecnicaPdf != "",
                    FichaTecnicaPdf = p.FichaTecnicaPdf
                })
                .ToListAsync();
        }

        private async Task<List<ServicioPublicoDto>> ObtenerServiciosInicioAsync()
        {
            return await ConsultaServiciosBase()
                .OrderByDescending(s => s.IdElementoCatalogoNavigation.FechaRegistro)
                .ThenByDescending(s => s.IdServicio)
                .Take(8)
                .Select(s => new ServicioPublicoDto
                {
                    Id = s.IdServicio,
                    IdServicio = s.IdServicio,
                    IdElementoCatalogo = s.IdElementoCatalogo,

                    Nombre = s.IdElementoCatalogoNavigation.Nombre,
                    Descripcion = s.IdElementoCatalogoNavigation.Descripcion ?? string.Empty,
                    SectorAplicacion = s.SectorAplicacion,
                    MensajeWhatsApp = s.MensajeWhatsApp,
                    RequiereVisitaTecnica = s.RequiereVisitaTecnica,

                    ImagenUrl =
                        s.IdElementoCatalogoNavigation.ImagenElementoCatalogo
                            .Where(i => i.Estado)
                            .OrderByDescending(i => i.EsPrincipal)
                            .ThenByDescending(i => i.IdImagenElementoCatalogo)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                        ?? s.IdElementoCatalogoNavigation.ImagenUrl
                        ?? string.Empty
                })
                .ToListAsync();
        }

        private IQueryable<Producto> ConsultaProductosBase()
        {
            return _context.Producto
                .AsNoTracking()
                .Where(p =>
                    p.IdElementoCatalogoNavigation.Estado &&
                    p.IdCategoriaNavigation.Estado &&
                    (
                        p.IdMarcaNavigation == null ||
                        p.IdMarcaNavigation.Estado
                    )
                );
        }

        private IQueryable<Servicio> ConsultaServiciosBase()
        {
            return _context.Servicio
                .AsNoTracking()
                .Where(s => s.IdElementoCatalogoNavigation.Estado);
        }

        private static string ObtenerIconoCategoria(string nombre)
        {
            var texto = nombre.ToLower();

            if (texto.Contains("vapor") || texto.Contains("caldera"))
            {
                return "♨";
            }

            if (texto.Contains("instrument") || texto.Contains("automat"))
            {
                return "⚙";
            }

            if (texto.Contains("mantenimiento") || texto.Contains("herramient"))
            {
                return "🛠";
            }

            if (texto.Contains("repuesto") || texto.Contains("terminal") || texto.Contains("eléctr"))
            {
                return "🔩";
            }

            return "🏭";
        }
    }
}