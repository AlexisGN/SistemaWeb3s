using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Producto;
using Sistema3S.Web.Models;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class ProductoService : IProductoService
    {
        private readonly Bd3sContext _context;

        public ProductoService(Bd3sContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<ProductoListadoDto>> ListarAsync(
            string? buscar,
            int pagina,
            int tamanioPagina
        )
        {
            if (pagina <= 0)
            {
                pagina = 1;
            }

            if (tamanioPagina <= 0)
            {
                tamanioPagina = 10;
            }

            if (tamanioPagina > 50)
            {
                tamanioPagina = 50;
            }

            var query = _context.Producto
                .AsNoTracking()
                .Include(p => p.IdElementoCatalogoNavigation)
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdMarcaNavigation)
                .Include(p => p.IdUnidadMedidaNavigation)
                .Include(p => p.Inventario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(p =>
                    p.IdElementoCatalogoNavigation.Nombre.Contains(texto) ||
                    p.CodigoProducto.Contains(texto) ||
                    p.IdCategoriaNavigation.Nombre.Contains(texto) ||
                    (p.IdMarcaNavigation != null && p.IdMarcaNavigation.Nombre.Contains(texto)) ||
                    (p.IdUnidadMedidaNavigation != null && p.IdUnidadMedidaNavigation.Nombre.Contains(texto))
                );
            }

            var totalRegistros = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.IdProducto)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(p => new ProductoListadoDto
                {
                    IdProducto = p.IdProducto,
                    IdElementoCatalogo = p.IdElementoCatalogo,

                    IdCategoria = p.IdCategoria,
                    IdMarca = p.IdMarca,
                    IdUnidadMedida = p.IdUnidadMedida,

                    Nombre = p.IdElementoCatalogoNavigation.Nombre,
                    Descripcion = p.IdElementoCatalogoNavigation.Descripcion,
                    PrecioReferencial = p.IdElementoCatalogoNavigation.PrecioReferencial,
                    ImagenUrl = p.IdElementoCatalogoNavigation.ImagenUrl,
                    Estado = p.IdElementoCatalogoNavigation.Estado,

                    Categoria = p.IdCategoriaNavigation.Nombre,
                    Marca = p.IdMarcaNavigation != null
                        ? p.IdMarcaNavigation.Nombre
                        : null,
                    UnidadMedida = p.IdUnidadMedidaNavigation != null
                        ? p.IdUnidadMedidaNavigation.Nombre
                        : null,

                    CodigoProducto = p.CodigoProducto,
                    FichaTecnicaPdf = p.FichaTecnicaPdf,

                    StockActual = p.Inventario != null
                        ? p.Inventario.StockActual
                        : 0,
                    StockMinimo = p.Inventario != null
                        ? p.Inventario.StockMinimo
                        : 0
                })
                .ToListAsync();

            return new ResultadoPaginadoDto<ProductoListadoDto>
            {
                Items = items,
                Pagina = pagina,
                TamanioPagina = tamanioPagina,
                TotalRegistros = totalRegistros
            };
        }

        public async Task<ProductoListadoDto?> ObtenerPorIdAsync(int idProducto)
        {
            return await _context.Producto
                .AsNoTracking()
                .Include(p => p.IdElementoCatalogoNavigation)
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdMarcaNavigation)
                .Include(p => p.IdUnidadMedidaNavigation)
                .Include(p => p.Inventario)
                .Where(p => p.IdProducto == idProducto)
                .Select(p => new ProductoListadoDto
                {
                    IdProducto = p.IdProducto,
                    IdElementoCatalogo = p.IdElementoCatalogo,

                    IdCategoria = p.IdCategoria,
                    IdMarca = p.IdMarca,
                    IdUnidadMedida = p.IdUnidadMedida,

                    Nombre = p.IdElementoCatalogoNavigation.Nombre,
                    Descripcion = p.IdElementoCatalogoNavigation.Descripcion,
                    PrecioReferencial = p.IdElementoCatalogoNavigation.PrecioReferencial,
                    ImagenUrl = p.IdElementoCatalogoNavigation.ImagenUrl,
                    Estado = p.IdElementoCatalogoNavigation.Estado,

                    Categoria = p.IdCategoriaNavigation.Nombre,
                    Marca = p.IdMarcaNavigation != null
                        ? p.IdMarcaNavigation.Nombre
                        : null,
                    UnidadMedida = p.IdUnidadMedidaNavigation != null
                        ? p.IdUnidadMedidaNavigation.Nombre
                        : null,

                    CodigoProducto = p.CodigoProducto,
                    FichaTecnicaPdf = p.FichaTecnicaPdf,

                    StockActual = p.Inventario != null
                        ? p.Inventario.StockActual
                        : 0,
                    StockMinimo = p.Inventario != null
                        ? p.Inventario.StockMinimo
                        : 0
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProductoListadoDto> CrearAsync(ProductoCrearDto dto)
        {
            ValidarCrear(dto);

            var codigoLimpio = NormalizarCodigo(dto.CodigoProducto);

            await ValidarCodigoUnicoParaCrearAsync(codigoLimpio);

            var tipoProducto = await _context.TipoElemento
                .FirstOrDefaultAsync(t => t.Nombre == "Producto");

            if (tipoProducto == null)
            {
                throw new InvalidOperationException("No existe el tipo de elemento Producto.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var elemento = new ElementoCatalogo
            {
                IdTipoElemento = tipoProducto.IdTipoElemento,
                Nombre = dto.Nombre.Trim(),
                Descripcion = NormalizarTexto(dto.Descripcion),
                PrecioReferencial = dto.PrecioReferencial,
                ImagenUrl = NormalizarTexto(dto.ImagenUrl),
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            _context.ElementoCatalogo.Add(elemento);
            await _context.SaveChangesAsync();

            var producto = new Producto
            {
                IdElementoCatalogo = elemento.IdElementoCatalogo,
                IdCategoria = dto.IdCategoria,
                IdMarca = dto.IdMarca,
                IdUnidadMedida = dto.IdUnidadMedida,
                CodigoProducto = codigoLimpio,
                FichaTecnicaPdf = NormalizarTexto(dto.FichaTecnicaPdf),
                AplicaInventario = true
            };

            _context.Producto.Add(producto);
            await _context.SaveChangesAsync();

            var inventario = new Inventario
            {
                IdProducto = producto.IdProducto,
                StockActual = dto.StockInicial,
                StockMinimo = dto.StockMinimo,
                FechaActualizacion = DateTime.Now
            };

            _context.Inventario.Add(inventario);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var productoCreado = await ObtenerPorIdAsync(producto.IdProducto);

            if (productoCreado == null)
            {
                throw new InvalidOperationException("El producto fue creado, pero no se pudo recuperar.");
            }

            return productoCreado;
        }

        public async Task<bool> ActualizarAsync(int idProducto, ProductoActualizarDto dto)
        {
            ValidarActualizar(dto);

            var codigoLimpio = NormalizarCodigo(dto.CodigoProducto);

            var producto = await _context.Producto
                .Include(p => p.IdElementoCatalogoNavigation)
                .Include(p => p.Inventario)
                .FirstOrDefaultAsync(p => p.IdProducto == idProducto);

            if (producto == null)
            {
                return false;
            }

            await ValidarCodigoUnicoParaActualizarAsync(codigoLimpio, idProducto);

            producto.IdCategoria = dto.IdCategoria;
            producto.IdMarca = dto.IdMarca;
            producto.IdUnidadMedida = dto.IdUnidadMedida;
            producto.CodigoProducto = codigoLimpio;
            producto.FichaTecnicaPdf = NormalizarTexto(dto.FichaTecnicaPdf);

            producto.IdElementoCatalogoNavigation.Nombre = dto.Nombre.Trim();
            producto.IdElementoCatalogoNavigation.Descripcion = NormalizarTexto(dto.Descripcion);
            producto.IdElementoCatalogoNavigation.PrecioReferencial = dto.PrecioReferencial;
            producto.IdElementoCatalogoNavigation.ImagenUrl = NormalizarTexto(dto.ImagenUrl);
            producto.IdElementoCatalogoNavigation.Estado = dto.Estado;

            if (producto.Inventario != null)
            {
                if (producto.Inventario.StockActual <= dto.StockMinimo)
                {
                    throw new InvalidOperationException("El stock actual debe ser mayor que el stock mínimo.");
                }

                producto.Inventario.StockMinimo = dto.StockMinimo;
                producto.Inventario.FechaActualizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EliminarLogicoAsync(int idProducto)
        {
            var producto = await _context.Producto
                .Include(p => p.IdElementoCatalogoNavigation)
                .FirstOrDefaultAsync(p => p.IdProducto == idProducto);

            if (producto == null)
            {
                return false;
            }

            producto.IdElementoCatalogoNavigation.Estado = false;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> ContarActivosAsync()
        {
            return await _context.Producto
                .Include(p => p.IdElementoCatalogoNavigation)
                .CountAsync(p => p.IdElementoCatalogoNavigation.Estado);
        }

        private static void ValidarCrear(ProductoCrearDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                throw new InvalidOperationException("El nombre del producto es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(dto.CodigoProducto))
            {
                throw new InvalidOperationException("El código del producto es obligatorio.");
            }

            if (!dto.PrecioReferencial.HasValue)
            {
                throw new InvalidOperationException("El precio referencial es obligatorio.");
            }

            if (dto.PrecioReferencial.Value < 0)
            {
                throw new InvalidOperationException("El precio referencial no puede ser negativo.");
            }

            if (string.IsNullOrWhiteSpace(dto.Descripcion))
            {
                throw new InvalidOperationException("La descripción del producto es obligatoria.");
            }

            if (dto.IdCategoria <= 0)
            {
                throw new InvalidOperationException("Selecciona una categoría válida.");
            }

            if (!dto.IdUnidadMedida.HasValue || dto.IdUnidadMedida.Value <= 0)
            {
                throw new InvalidOperationException("Selecciona una unidad de medida válida.");
            }

            if (string.IsNullOrWhiteSpace(dto.ImagenUrl))
            {
                throw new InvalidOperationException("La URL de la imagen es obligatoria.");
            }

            if (dto.StockInicial < 0)
            {
                throw new InvalidOperationException("El stock inicial no puede ser negativo.");
            }

            if (dto.StockMinimo < 0)
            {
                throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
            }

            if (dto.StockInicial <= dto.StockMinimo)
            {
                throw new InvalidOperationException("El stock inicial debe ser mayor que el stock mínimo.");
            }
        }

        private static void ValidarActualizar(ProductoActualizarDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                throw new InvalidOperationException("El nombre del producto es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(dto.CodigoProducto))
            {
                throw new InvalidOperationException("El código del producto es obligatorio.");
            }

            if (!dto.PrecioReferencial.HasValue)
            {
                throw new InvalidOperationException("El precio referencial es obligatorio.");
            }

            if (dto.PrecioReferencial.Value < 0)
            {
                throw new InvalidOperationException("El precio referencial no puede ser negativo.");
            }

            if (string.IsNullOrWhiteSpace(dto.Descripcion))
            {
                throw new InvalidOperationException("La descripción del producto es obligatoria.");
            }

            if (dto.IdCategoria <= 0)
            {
                throw new InvalidOperationException("Selecciona una categoría válida.");
            }

            if (!dto.IdUnidadMedida.HasValue || dto.IdUnidadMedida.Value <= 0)
            {
                throw new InvalidOperationException("Selecciona una unidad de medida válida.");
            }

            if (string.IsNullOrWhiteSpace(dto.ImagenUrl))
            {
                throw new InvalidOperationException("La URL de la imagen es obligatoria.");
            }

            if (dto.StockMinimo < 0)
            {
                throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
            }
        }

        private async Task ValidarCodigoUnicoParaCrearAsync(string codigoLimpio)
        {
            var existeCodigo = await _context.Producto
                .AsNoTracking()
                .AnyAsync(p => p.CodigoProducto == codigoLimpio);

            if (existeCodigo)
            {
                throw new InvalidOperationException("Ya existe un producto registrado con ese código.");
            }
        }

        private async Task ValidarCodigoUnicoParaActualizarAsync(string codigoLimpio, int idProductoActual)
        {
            var existeCodigo = await _context.Producto
                .AsNoTracking()
                .AnyAsync(p =>
                    p.IdProducto != idProductoActual &&
                    p.CodigoProducto == codigoLimpio
                );

            if (existeCodigo)
            {
                throw new InvalidOperationException("Ya existe otro producto registrado con ese código.");
            }
        }

        private static string NormalizarCodigo(string codigo)
        {
            return codigo.Trim().ToUpper();
        }

        private static string? NormalizarTexto(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            return valor.Trim();
        }
    }
}