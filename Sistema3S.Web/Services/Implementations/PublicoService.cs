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
            var categorias = await ObtenerCategoriasAsync();
            var marcas = await ObtenerMarcasAsync();
            var productosNuevos = await ObtenerProductosNuevosAsync();
            var productos = await ObtenerProductosAsync();
            var servicios = await ObtenerServiciosAsync();

            return new InicioPublicoDto
            {
                Categorias = categorias,
                Marcas = marcas,
                ProductosNuevos = productosNuevos,
                Productos = productos,
                Servicios = servicios
            };
        }

        private async Task<List<CategoriaPublicaDto>> ObtenerCategoriasAsync()
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

        private async Task<List<MarcaPublicaDto>> ObtenerMarcasAsync()
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
                    Disponible = !p.AplicaInventario || p.Inventario == null || p.Inventario.StockActual > 0,
                    TieneFichaTecnica = p.FichaTecnicaPdf != null && p.FichaTecnicaPdf != "",
                    FichaTecnicaPdf = p.FichaTecnicaPdf
                })
                .ToListAsync();
        }

        private async Task<List<ProductoPublicoDto>> ObtenerProductosAsync()
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
                    Disponible = !p.AplicaInventario || p.Inventario == null || p.Inventario.StockActual > 0,
                    TieneFichaTecnica = p.FichaTecnicaPdf != null && p.FichaTecnicaPdf != "",
                    FichaTecnicaPdf = p.FichaTecnicaPdf
                })
                .ToListAsync();
        }

        private async Task<List<ServicioPublicoDto>> ObtenerServiciosAsync()
        {
            return await _context.Servicio
                .AsNoTracking()
                .Where(s => s.IdElementoCatalogoNavigation.Estado)
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