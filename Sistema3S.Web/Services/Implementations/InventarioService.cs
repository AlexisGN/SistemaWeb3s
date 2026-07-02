using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Inventario;
using Sistema3S.Web.Models;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class InventarioService : IInventarioService
    {
        private readonly Bd3sContext _context;

        public InventarioService(Bd3sContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<InventarioListadoDto>> ListarAsync(
            string? buscar,
            string? estadoStock,
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
                tamanioPagina = 6;
            }

            if (tamanioPagina > 50)
            {
                tamanioPagina = 50;
            }

            var query = _context.Inventario
                .AsNoTracking()
                .Where(i =>
                    i.IdProductoNavigation.AplicaInventario &&
                    i.IdProductoNavigation.IdElementoCatalogoNavigation.Estado
                )
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(i =>
                    i.IdProductoNavigation.CodigoProducto.Contains(texto) ||
                    i.IdProductoNavigation.IdElementoCatalogoNavigation.Nombre.Contains(texto) ||
                    i.IdProductoNavigation.IdCategoriaNavigation.Nombre.Contains(texto) ||
                    (
                        i.IdProductoNavigation.IdMarcaNavigation != null &&
                        i.IdProductoNavigation.IdMarcaNavigation.Nombre.Contains(texto)
                    )
                );
            }

            estadoStock = NormalizarEstadoStock(estadoStock);

            if (estadoStock == "normal")
            {
                query = query.Where(i => i.StockActual > i.StockMinimo);
            }
            else if (estadoStock == "bajo")
            {
                query = query.Where(i => i.StockActual > 0 && i.StockActual <= i.StockMinimo);
            }
            else if (estadoStock == "sin-stock")
            {
                query = query.Where(i => i.StockActual == 0);
            }

            var totalRegistros = await query.CountAsync();

            var inventarios = await query
                .OrderBy(i => i.StockActual == 0 ? 0 : i.StockActual <= i.StockMinimo ? 1 : 2)
                .ThenBy(i => i.IdProductoNavigation.IdElementoCatalogoNavigation.Nombre)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(i => new InventarioListadoDto
                {
                    IdInventario = i.IdInventario,
                    IdProducto = i.IdProducto,
                    IdElementoCatalogo = i.IdProductoNavigation.IdElementoCatalogo,

                    CodigoProducto = i.IdProductoNavigation.CodigoProducto,
                    Producto = i.IdProductoNavigation.IdElementoCatalogoNavigation.Nombre,

                    Categoria = i.IdProductoNavigation.IdCategoriaNavigation.Nombre,
                    Marca = i.IdProductoNavigation.IdMarcaNavigation != null
                        ? i.IdProductoNavigation.IdMarcaNavigation.Nombre
                        : null,
                    UnidadMedida = i.IdProductoNavigation.IdUnidadMedidaNavigation != null
                        ? i.IdProductoNavigation.IdUnidadMedidaNavigation.Nombre
                        : null,

                    StockActual = i.StockActual,
                    StockMinimo = i.StockMinimo,
                    FechaActualizacion = i.FechaActualizacion
                })
                .ToListAsync();

            foreach (var inventario in inventarios)
            {
                AplicarEstadoStock(inventario);
            }

            return new ResultadoPaginadoDto<InventarioListadoDto>
            {
                Items = inventarios,
                Pagina = pagina,
                TamanioPagina = tamanioPagina,
                TotalRegistros = totalRegistros
            };
        }

        public async Task<InventarioListadoDto?> ObtenerPorProductoAsync(int idProducto)
        {
            var inventario = await _context.Inventario
                .AsNoTracking()
                .Where(i =>
                    i.IdProducto == idProducto &&
                    i.IdProductoNavigation.AplicaInventario &&
                    i.IdProductoNavigation.IdElementoCatalogoNavigation.Estado
                )
                .Select(i => new InventarioListadoDto
                {
                    IdInventario = i.IdInventario,
                    IdProducto = i.IdProducto,
                    IdElementoCatalogo = i.IdProductoNavigation.IdElementoCatalogo,

                    CodigoProducto = i.IdProductoNavigation.CodigoProducto,
                    Producto = i.IdProductoNavigation.IdElementoCatalogoNavigation.Nombre,

                    Categoria = i.IdProductoNavigation.IdCategoriaNavigation.Nombre,
                    Marca = i.IdProductoNavigation.IdMarcaNavigation != null
                        ? i.IdProductoNavigation.IdMarcaNavigation.Nombre
                        : null,
                    UnidadMedida = i.IdProductoNavigation.IdUnidadMedidaNavigation != null
                        ? i.IdProductoNavigation.IdUnidadMedidaNavigation.Nombre
                        : null,

                    StockActual = i.StockActual,
                    StockMinimo = i.StockMinimo,
                    FechaActualizacion = i.FechaActualizacion
                })
                .FirstOrDefaultAsync();

            if (inventario == null)
            {
                return null;
            }

            AplicarEstadoStock(inventario);

            return inventario;
        }

        public async Task<InventarioResumenDto> ObtenerResumenAsync()
        {
            var inventarios = _context.Inventario
                .AsNoTracking()
                .Where(i =>
                    i.IdProductoNavigation.AplicaInventario &&
                    i.IdProductoNavigation.IdElementoCatalogoNavigation.Estado
                );

            var resumen = await inventarios
                .GroupBy(i => 1)
                .Select(g => new
                {
                    TotalProductosInventario = g.Count(),
                    TotalStockNormal = g.Count(i => i.StockActual > i.StockMinimo),
                    TotalStockBajo = g.Count(i => i.StockActual > 0 && i.StockActual <= i.StockMinimo),
                    TotalSinStock = g.Count(i => i.StockActual == 0)
                })
                .FirstOrDefaultAsync();

            var totalMovimientos = await _context.MovimientoStock
                .AsNoTracking()
                .CountAsync();

            if (resumen == null)
            {
                return new InventarioResumenDto
                {
                    TotalProductosInventario = 0,
                    TotalStockNormal = 0,
                    TotalStockBajo = 0,
                    TotalSinStock = 0,
                    TotalAlertasPendientes = 0,
                    TotalMovimientos = totalMovimientos
                };
            }

            return new InventarioResumenDto
            {
                TotalProductosInventario = resumen.TotalProductosInventario,
                TotalStockNormal = resumen.TotalStockNormal,
                TotalStockBajo = resumen.TotalStockBajo,
                TotalSinStock = resumen.TotalSinStock,
                TotalAlertasPendientes = resumen.TotalStockBajo + resumen.TotalSinStock,
                TotalMovimientos = totalMovimientos
            };
        }

        public async Task<List<MovimientoStockListadoDto>> ListarMovimientosPorProductoAsync(int idProducto)
        {
            var existeProducto = await _context.Producto
                .AsNoTracking()
                .AnyAsync(p =>
                    p.IdProducto == idProducto &&
                    p.AplicaInventario &&
                    p.IdElementoCatalogoNavigation.Estado
                );

            if (!existeProducto)
            {
                throw new InvalidOperationException("El producto seleccionado no existe o no aplica inventario.");
            }

            return await ObtenerQueryMovimientos()
                .Where(m => m.IdProducto == idProducto)
                .OrderByDescending(m => m.FechaMovimiento)
                .Take(50)
                .ToListAsync();
        }

        public async Task<List<MovimientoStockListadoDto>> ListarMovimientosRecientesAsync(int cantidad)
        {
            if (cantidad <= 0)
            {
                cantidad = 5;
            }

            if (cantidad > 20)
            {
                cantidad = 20;
            }

            return await ObtenerQueryMovimientos()
                .OrderByDescending(m => m.FechaMovimiento)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<InventarioListadoDto> ActualizarStockMinimoAsync(
            int idProducto,
            ActualizarStockMinimoDto dto
        )
        {
            if (idProducto <= 0)
            {
                throw new InvalidOperationException("Selecciona un producto válido.");
            }

            if (dto.StockMinimo < 0)
            {
                throw new InvalidOperationException("El stock mínimo no puede ser negativo.");
            }

            var inventario = await _context.Inventario
                .Include(i => i.IdProductoNavigation)
                    .ThenInclude(p => p.IdElementoCatalogoNavigation)
                .FirstOrDefaultAsync(i =>
                    i.IdProducto == idProducto &&
                    i.IdProductoNavigation.AplicaInventario &&
                    i.IdProductoNavigation.IdElementoCatalogoNavigation.Estado
                );

            if (inventario == null)
            {
                throw new InvalidOperationException("No se encontró inventario para el producto seleccionado.");
            }

            inventario.StockMinimo = dto.StockMinimo;
            inventario.FechaActualizacion = DateTime.Now;

            await SincronizarAlertaStockAsync(
                inventario.IdProducto,
                inventario.StockActual,
                inventario.StockMinimo
            );

            await _context.SaveChangesAsync();

            var actualizado = await ObtenerPorProductoAsync(idProducto);

            if (actualizado == null)
            {
                throw new InvalidOperationException("No se pudo recuperar el inventario actualizado.");
            }

            return actualizado;
        }

        public async Task<InventarioListadoDto> RegistrarMovimientoManualAsync(
            RegistrarMovimientoStockDto dto
        )
        {
            ValidarMovimiento(dto);

            var idUsuarioRegistro = dto.IdUsuarioRegistro.GetValueOrDefault(1);

            var existeUsuario = await _context.Usuario
                .AsNoTracking()
                .AnyAsync(u => u.IdUsuario == idUsuarioRegistro && u.Estado);

            if (!existeUsuario)
            {
                throw new InvalidOperationException("El usuario que registra el movimiento no existe o está inactivo.");
            }

            var inventario = await _context.Inventario
                .Include(i => i.IdProductoNavigation)
                    .ThenInclude(p => p.IdElementoCatalogoNavigation)
                .FirstOrDefaultAsync(i =>
                    i.IdProducto == dto.IdProducto &&
                    i.IdProductoNavigation.AplicaInventario &&
                    i.IdProductoNavigation.IdElementoCatalogoNavigation.Estado
                );

            if (inventario == null)
            {
                throw new InvalidOperationException("No se encontró inventario para el producto seleccionado.");
            }

            var tipoMovimientoNormalizado = NormalizarTipoMovimiento(dto.TipoMovimiento);
            var idTipoMovimiento = await ObtenerIdTipoMovimientoStockAsync(tipoMovimientoNormalizado);

            var stockAnterior = inventario.StockActual;
            var nuevoStock = stockAnterior;
            var cantidadMovimiento = 0;

            if (tipoMovimientoNormalizado == "Entrada")
            {
                cantidadMovimiento = dto.Cantidad!.Value;
                nuevoStock = stockAnterior + cantidadMovimiento;
            }
            else if (tipoMovimientoNormalizado == "Salida")
            {
                cantidadMovimiento = dto.Cantidad!.Value;

                if (cantidadMovimiento > stockAnterior)
                {
                    throw new InvalidOperationException("No puedes registrar una salida mayor al stock actual.");
                }

                nuevoStock = stockAnterior - cantidadMovimiento;
            }
            else
            {
                nuevoStock = dto.NuevoStock!.Value;

                if (nuevoStock < 0)
                {
                    throw new InvalidOperationException("El nuevo stock no puede ser negativo.");
                }

                cantidadMovimiento = Math.Abs(nuevoStock - stockAnterior);

                if (cantidadMovimiento == 0)
                {
                    throw new InvalidOperationException("El nuevo stock debe ser diferente al stock actual.");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            inventario.StockActual = nuevoStock;
            inventario.FechaActualizacion = DateTime.Now;

            var motivoFinal = dto.Motivo.Trim();

            if (tipoMovimientoNormalizado == "Ajuste")
            {
                motivoFinal =
                    $"{motivoFinal}. Stock anterior: {stockAnterior}. Stock nuevo: {nuevoStock}. Diferencia: {cantidadMovimiento}.";
            }

            var movimiento = new MovimientoStock
            {
                IdProducto = inventario.IdProducto,
                IdUsuarioRegistro = idUsuarioRegistro,
                IdTipoMovimientoStock = idTipoMovimiento,
                IdVenta = null,
                IdCompra = null,
                Cantidad = cantidadMovimiento,
                FechaMovimiento = DateTime.Now,
                Motivo = motivoFinal
            };

            _context.MovimientoStock.Add(movimiento);

            await SincronizarAlertaStockAsync(
                inventario.IdProducto,
                inventario.StockActual,
                inventario.StockMinimo
            );

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var actualizado = await ObtenerPorProductoAsync(inventario.IdProducto);

            if (actualizado == null)
            {
                throw new InvalidOperationException("No se pudo recuperar el inventario actualizado.");
            }

            return actualizado;
        }

        private IQueryable<MovimientoStockListadoDto> ObtenerQueryMovimientos()
        {
            return _context.MovimientoStock
                .AsNoTracking()
                .Select(m => new MovimientoStockListadoDto
                {
                    IdMovimientoStock = m.IdMovimientoStock,

                    IdProducto = m.IdProducto,
                    CodigoProducto = m.IdProductoNavigation.CodigoProducto,
                    Producto = m.IdProductoNavigation.IdElementoCatalogoNavigation.Nombre,

                    TipoMovimiento = m.IdTipoMovimientoStockNavigation.Nombre,
                    Cantidad = m.Cantidad,

                    FechaMovimiento = m.FechaMovimiento,
                    Motivo = m.Motivo,

                    IdVenta = m.IdVenta,
                    IdCompra = m.IdCompra,

                    UsuarioRegistro = m.IdUsuarioRegistroNavigation.Correo
                });
        }

        private async Task SincronizarAlertaStockAsync(
            int idProducto,
            int stockActual,
            int stockMinimo
        )
        {
            var idPendiente = await ObtenerIdEstadoAlertaAsync("Pendiente", true);
            var idAtendida = await ObtenerIdEstadoAlertaAsync("Atendida", true);

            var alertaPendiente = await _context.AlertaStock
                .FirstOrDefaultAsync(a =>
                    a.IdProducto == idProducto &&
                    a.IdEstadoAlertaStock == idPendiente
                );

            if (stockActual <= stockMinimo)
            {
                var producto = await _context.Producto
                    .AsNoTracking()
                    .Include(p => p.IdElementoCatalogoNavigation)
                    .FirstAsync(p => p.IdProducto == idProducto);

                var mensaje = stockActual == 0
                    ? $"Producto sin stock: {producto.IdElementoCatalogoNavigation.Nombre}."
                    : $"Producto con stock bajo: {producto.IdElementoCatalogoNavigation.Nombre}. Stock actual: {stockActual}, mínimo: {stockMinimo}.";

                if (alertaPendiente == null)
                {
                    _context.AlertaStock.Add(new AlertaStock
                    {
                        IdProducto = idProducto,
                        IdEstadoAlertaStock = idPendiente,
                        FechaAlerta = DateTime.Now,
                        Mensaje = mensaje
                    });
                }
                else
                {
                    alertaPendiente.Mensaje = mensaje;
                }
            }
            else
            {
                if (alertaPendiente != null)
                {
                    alertaPendiente.IdEstadoAlertaStock = idAtendida;
                    alertaPendiente.Mensaje =
                        $"Alerta atendida. Stock actual: {stockActual}, mínimo: {stockMinimo}.";
                }
            }
        }

        private async Task<int> ObtenerIdTipoMovimientoStockAsync(string nombre)
        {
            var idTipo = await _context.TipoMovimientoStock
                .AsNoTracking()
                .Where(t => t.Nombre == nombre && t.Estado)
                .Select(t => t.IdTipoMovimientoStock)
                .FirstOrDefaultAsync();

            if (idTipo == 0)
            {
                throw new InvalidOperationException($"No existe el tipo de movimiento {nombre}.");
            }

            return idTipo;
        }

        private async Task<int> ObtenerIdEstadoAlertaAsync(string nombre, bool lanzarError)
        {
            var idEstado = await _context.EstadoAlertaStock
                .AsNoTracking()
                .Where(e => e.Nombre == nombre && e.Estado)
                .Select(e => e.IdEstadoAlertaStock)
                .FirstOrDefaultAsync();

            if (idEstado == 0 && lanzarError)
            {
                throw new InvalidOperationException($"No existe el estado de alerta {nombre}.");
            }

            return idEstado;
        }

        private static void ValidarMovimiento(RegistrarMovimientoStockDto dto)
        {
            if (dto.IdProducto <= 0)
            {
                throw new InvalidOperationException("Selecciona un producto.");
            }

            var tipoMovimiento = NormalizarTipoMovimiento(dto.TipoMovimiento);

            if (tipoMovimiento != "Entrada" &&
                tipoMovimiento != "Salida" &&
                tipoMovimiento != "Ajuste")
            {
                throw new InvalidOperationException("Selecciona un tipo de movimiento válido.");
            }

            if (tipoMovimiento == "Entrada" || tipoMovimiento == "Salida")
            {
                if (!dto.Cantidad.HasValue)
                {
                    throw new InvalidOperationException("Ingresa una cantidad.");
                }

                if (dto.Cantidad.Value <= 0)
                {
                    throw new InvalidOperationException("La cantidad debe ser mayor a 0.");
                }
            }

            if (tipoMovimiento == "Ajuste")
            {
                if (!dto.NuevoStock.HasValue)
                {
                    throw new InvalidOperationException("Ingresa el nuevo stock real.");
                }

                if (dto.NuevoStock.Value < 0)
                {
                    throw new InvalidOperationException("El nuevo stock no puede ser negativo.");
                }
            }

            if (string.IsNullOrWhiteSpace(dto.Motivo))
            {
                throw new InvalidOperationException("Ingresa el motivo del movimiento.");
            }

            if (dto.Motivo.Trim().Length < 5)
            {
                throw new InvalidOperationException("El motivo debe tener al menos 5 caracteres.");
            }
        }

        private static void AplicarEstadoStock(InventarioListadoDto inventario)
        {
            if (inventario.StockActual == 0)
            {
                inventario.EstadoStock = "Sin stock";
                inventario.TieneAlertaPendiente = true;
                inventario.MensajeAlerta =
                    $"Producto sin stock: {inventario.Producto}.";
                return;
            }

            if (inventario.StockActual <= inventario.StockMinimo)
            {
                inventario.EstadoStock = "Stock bajo";
                inventario.TieneAlertaPendiente = true;
                inventario.MensajeAlerta =
                    $"Producto con stock bajo: {inventario.Producto}. Stock actual: {inventario.StockActual}, mínimo: {inventario.StockMinimo}.";
                return;
            }

            inventario.EstadoStock = "Normal";
            inventario.TieneAlertaPendiente = false;
            inventario.MensajeAlerta = null;
        }

        private static string NormalizarTipoMovimiento(string? tipoMovimiento)
        {
            var valor = (tipoMovimiento ?? string.Empty).Trim().ToLower();

            return valor switch
            {
                "entrada" => "Entrada",
                "salida" => "Salida",
                "ajuste" => "Ajuste",
                _ => string.Empty
            };
        }

        private static string NormalizarEstadoStock(string? estadoStock)
        {
            var valor = (estadoStock ?? "todos").Trim().ToLower();

            return valor switch
            {
                "normal" => "normal",
                "bajo" => "bajo",
                "stock-bajo" => "bajo",
                "sin-stock" => "sin-stock",
                "sinstock" => "sin-stock",
                _ => "todos"
            };
        }
    }
}