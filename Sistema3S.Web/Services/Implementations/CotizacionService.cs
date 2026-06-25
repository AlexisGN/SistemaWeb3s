using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Cotizacion;
using Sistema3S.Web.Models;
using Sistema3S.Web.Services.Interfaces;
using Sistema3S.Web.Services.Pdf;

namespace Sistema3S.Web.Services.Implementations
{
    public class CotizacionService : ICotizacionService
    {
        private readonly Bd3sContext _context;

        private const decimal PorcentajeIgv = 0.18m;

        private readonly IPdfCotizacionService _pdfCotizacionService;

        public CotizacionService(
            Bd3sContext context,
            IPdfCotizacionService pdfCotizacionService
)
        {
            _context = context;
            _pdfCotizacionService = pdfCotizacionService;
        }

        public async Task<ResultadoPaginadoDto<CotizacionListadoDto>> ListarAsync(
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
                tamanioPagina = 5;
            }

            if (tamanioPagina > 50)
            {
                tamanioPagina = 50;
            }

            var query = _context.Cotizacion.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(c =>
                    (c.Observacion != null && c.Observacion.Contains(texto)) ||
                    c.OrigenCotizacion.Contains(texto) ||
                    _context.EstadoCotizacion.Any(e =>
                        e.IdEstadoCotizacion == c.IdEstadoCotizacion &&
                        e.Nombre.Contains(texto)
                    ) ||
                    _context.Cliente.Any(cl =>
                        cl.IdCliente == c.IdCliente &&
                        cl.NumeroDocumento.Contains(texto)
                    ) ||
                    _context.ClientePersonaNatural.Any(pn =>
                        pn.IdCliente == c.IdCliente &&
                        (
                            pn.Nombres.Contains(texto) ||
                            pn.ApellidoPaterno.Contains(texto) ||
                            (pn.ApellidoMaterno != null && pn.ApellidoMaterno.Contains(texto))
                        )
                    ) ||
                    _context.ClienteEmpresa.Any(ce =>
                        ce.IdCliente == c.IdCliente &&
                        (
                            ce.RazonSocial.Contains(texto) ||
                            (ce.NombreComercial != null && ce.NombreComercial.Contains(texto))
                        )
                    )
                );
            }

            var totalRegistros = await query.CountAsync();

            var cotizaciones = await query
                .OrderByDescending(c => c.IdCotizacion)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();

            var items = new List<CotizacionListadoDto>();

            foreach (var cotizacion in cotizaciones)
            {
                var item = await MapearCotizacionAsync(cotizacion, incluirDetalles: false);
                items.Add(item);
            }

            return new ResultadoPaginadoDto<CotizacionListadoDto>
            {
                Items = items,
                Pagina = pagina,
                TamanioPagina = tamanioPagina,
                TotalRegistros = totalRegistros
            };
        }

        public async Task<CotizacionListadoDto?> ObtenerPorIdAsync(int idCotizacion)
        {
            var cotizacion = await _context.Cotizacion
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                return null;
            }

            return await MapearCotizacionAsync(cotizacion, incluirDetalles: true);
        }

        public async Task<CotizacionListadoDto> CrearAsync(CotizacionCrearDto dto)
        {
            await ValidarCrearActualizarAsync(
                dto.IdCliente,
                dto.IdEstadoCotizacion,
                dto.OrigenCotizacion,
                dto.Descuento,
                dto.Detalles
            );

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var cotizacion = new Cotizacion
            {
                IdCliente = dto.IdCliente,
                IdUsuarioRegistro = dto.IdUsuarioRegistro,
                IdUsuarioAtencion = dto.IdUsuarioAtencion,
                IdEstadoCotizacion = dto.IdEstadoCotizacion,
                FechaCotizacion = DateTime.Now,

                OrigenCotizacion = NormalizarOrigen(dto.OrigenCotizacion),

                Subtotal = 0,
                Descuento = dto.Descuento,
                Igv = 0,
                Total = 0,
                TotalReferencial = 0,

                Observacion = NormalizarTexto(dto.Observacion),
                ArchivoPdf = NormalizarTexto(dto.ArchivoPdf),
                CorreoEnviado = dto.CorreoEnviado
            };

            _context.Cotizacion.Add(cotizacion);
            await _context.SaveChangesAsync();

            await RegistrarDetallesAsync(cotizacion.IdCotizacion, dto.Detalles);

            await RecalcularTotalesAsync(cotizacion, dto.Descuento);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await GenerarPdfAsync(cotizacion.IdCotizacion);

            var cotizacionCreada = await ObtenerPorIdAsync(cotizacion.IdCotizacion);

            if (cotizacionCreada == null)
            {
                throw new InvalidOperationException("No se pudo recuperar la cotización creada.");
            }

            return cotizacionCreada;
        }

        public async Task<bool> ActualizarAsync(int idCotizacion, CotizacionActualizarDto dto)
        {
            await ValidarCrearActualizarAsync(
                dto.IdCliente,
                dto.IdEstadoCotizacion,
                dto.OrigenCotizacion,
                dto.Descuento,
                dto.Detalles
            );

            var cotizacion = await _context.Cotizacion
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                return false;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            cotizacion.IdCliente = dto.IdCliente;
            cotizacion.IdEstadoCotizacion = dto.IdEstadoCotizacion;
            cotizacion.IdUsuarioAtencion = dto.IdUsuarioAtencion;

            cotizacion.OrigenCotizacion = NormalizarOrigen(dto.OrigenCotizacion);

            cotizacion.Observacion = NormalizarTexto(dto.Observacion);
            cotizacion.ArchivoPdf = NormalizarTexto(dto.ArchivoPdf);
            cotizacion.CorreoEnviado = dto.CorreoEnviado;

            var detallesActuales = await _context.DetalleCotizacion
                .Where(d => d.IdCotizacion == idCotizacion)
                .ToListAsync();

            _context.DetalleCotizacion.RemoveRange(detallesActuales);
            await _context.SaveChangesAsync();

            await RegistrarDetallesAsync(idCotizacion, dto.Detalles);

            await RecalcularTotalesAsync(cotizacion, dto.Descuento);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await GenerarPdfAsync(cotizacion.IdCotizacion);

            return true;
        }

        public async Task<bool> CancelarAsync(int idCotizacion)
        {
            var cotizacion = await _context.Cotizacion
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                return false;
            }

            var estadoCancelada = await _context.EstadoCotizacion
                .Where(e => e.Nombre == "Cancelada")
                .Select(e => e.IdEstadoCotizacion)
                .FirstOrDefaultAsync();

            if (estadoCancelada == 0)
            {
                throw new InvalidOperationException("No existe el estado Cancelada.");
            }

            cotizacion.IdEstadoCotizacion = estadoCancelada;
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> CambiarEstadoAsync(int idCotizacion, int idEstadoCotizacion)
        {
            var cotizacion = await _context.Cotizacion
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                return false;
            }

            var existeEstado = await _context.EstadoCotizacion
                .AnyAsync(e => e.IdEstadoCotizacion == idEstadoCotizacion && e.Estado);

            if (!existeEstado)
            {
                throw new InvalidOperationException("El estado seleccionado no es válido.");
            }

            cotizacion.IdEstadoCotizacion = idEstadoCotizacion;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GenerarPdfAsync(int idCotizacion)
        {
            return await _pdfCotizacionService.GenerarCotizacionAsync(idCotizacion);
        }

        public async Task<bool> MarcarCorreoEnviadoAsync(int idCotizacion)
        {
            var cotizacion = await _context.Cotizacion
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(cotizacion.ArchivoPdf))
            {
                await GenerarPdfAsync(idCotizacion);
            }

            var correoCliente = await _context.Cliente
                .Where(c => c.IdCliente == cotizacion.IdCliente)
                .Select(c => c.Correo)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(correoCliente))
            {
                throw new InvalidOperationException("El cliente no tiene correo registrado.");
            }

            cotizacion.CorreoEnviado = true;

            var estadoRespondida = await _context.EstadoCotizacion
                .Where(e => e.Nombre == "Respondida")
                .Select(e => e.IdEstadoCotizacion)
                .FirstOrDefaultAsync();

            if (estadoRespondida != 0)
            {
                var estadoActual = await _context.EstadoCotizacion
                    .Where(e => e.IdEstadoCotizacion == cotizacion.IdEstadoCotizacion)
                    .Select(e => e.Nombre)
                    .FirstOrDefaultAsync();

                if (estadoActual == "Pendiente" || estadoActual == "En revisión")
                {
                    cotizacion.IdEstadoCotizacion = estadoRespondida;
                }
            }

            var envio = new EnvioCorreo
            {
                IdCotizacion = cotizacion.IdCotizacion,
                IdComprobante = null,
                IdUsuarioRegistro = cotizacion.IdUsuarioAtencion ?? cotizacion.IdUsuarioRegistro,
                Destinatario = correoCliente,
                Asunto = $"Cotización COT-{cotizacion.IdCotizacion.ToString().PadLeft(5, '0')} - 3S",
                Cuerpo = $"Se registró el envío de la cotización COT-{cotizacion.IdCotizacion.ToString().PadLeft(5, '0')} al cliente.",
                FechaEnvio = DateTime.Now,
                Exitoso = true,
                MensajeError = null
            };

            _context.EnvioCorreo.Add(envio);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CotizacionWhatsAppDto> ObtenerWhatsAppAsync(int idCotizacion)
        {
            var cotizacion = await ObtenerPorIdAsync(idCotizacion);

            if (cotizacion == null)
            {
                throw new InvalidOperationException("Cotización no encontrada.");
            }

            var telefono = await _context.Cliente
                .Where(c => c.IdCliente == cotizacion.IdCliente)
                .Select(c => c.Telefono)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(telefono))
            {
                throw new InvalidOperationException("El cliente no tiene teléfono registrado.");
            }

            var telefonoLimpio = LimpiarTelefonoWhatsApp(telefono);

            var mensaje =
                $"Hola, le saludamos de 3S - Servicios y Soluciones Superiores. " +
                $"Le compartimos la cotización COT-{idCotizacion.ToString().PadLeft(5, '0')} " +
                $"por un total de S/ {cotizacion.Total:0.00}. " +
                $"Estado: {cotizacion.EstadoCotizacion}.";

            if (!string.IsNullOrWhiteSpace(cotizacion.ArchivoPdf))
            {
                mensaje += $" PDF: {cotizacion.ArchivoPdf}";
            }

            var url = $"https://wa.me/{telefonoLimpio}?text={Uri.EscapeDataString(mensaje)}";

            return new CotizacionWhatsAppDto
            {
                Telefono = telefonoLimpio,
                Mensaje = mensaje,
                Url = url
            };
        }

        public async Task<int> ConvertirEnVentaAsync(int idCotizacion, int? idUsuarioRegistro)
        {
            var cotizacion = await _context.Cotizacion
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
            {
                throw new InvalidOperationException("Cotización no encontrada.");
            }

            var yaExisteVenta = await _context.Venta
                .AnyAsync(v => v.IdCotizacion == idCotizacion);

            if (yaExisteVenta)
            {
                throw new InvalidOperationException("Esta cotización ya fue convertida en venta.");
            }

            var estadoCotizacion = await _context.EstadoCotizacion
                .Where(e => e.IdEstadoCotizacion == cotizacion.IdEstadoCotizacion)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync();

            if (estadoCotizacion == "Cancelada")
            {
                throw new InvalidOperationException("No se puede convertir una cotización cancelada en venta.");
            }

            var detalles = await _context.DetalleCotizacion
                .Where(d => d.IdCotizacion == idCotizacion)
                .ToListAsync();

            if (detalles.Count == 0)
            {
                throw new InvalidOperationException("La cotización no tiene detalles para convertir en venta.");
            }

            var idEstadoVenta = await _context.EstadoVenta
                .Where(e => e.Nombre == "Registrada")
                .Select(e => e.IdEstadoVenta)
                .FirstOrDefaultAsync();

            if (idEstadoVenta == 0)
            {
                idEstadoVenta = await _context.EstadoVenta
                    .Where(e => e.Estado)
                    .OrderBy(e => e.IdEstadoVenta)
                    .Select(e => e.IdEstadoVenta)
                    .FirstOrDefaultAsync();
            }

            if (idEstadoVenta == 0)
            {
                throw new InvalidOperationException("No existe un estado de venta válido.");
            }

            var idUsuarioFinal =
                idUsuarioRegistro ??
                cotizacion.IdUsuarioAtencion ??
                cotizacion.IdUsuarioRegistro ??
                await _context.Usuario
                    .OrderBy(u => u.IdUsuario)
                    .Select(u => u.IdUsuario)
                    .FirstOrDefaultAsync();

            if (idUsuarioFinal == 0)
            {
                throw new InvalidOperationException("No existe usuario para registrar la venta.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var venta = new Venta
            {
                IdCliente = cotizacion.IdCliente,
                IdCotizacion = cotizacion.IdCotizacion,
                IdUsuarioRegistro = idUsuarioFinal,
                IdEstadoVenta = idEstadoVenta,
                FechaVenta = DateTime.Now,
                Subtotal = cotizacion.Subtotal,
                Igv = cotizacion.Igv,
                Total = cotizacion.Total
            };

            _context.Venta.Add(venta);
            await _context.SaveChangesAsync();

            foreach (var item in detalles)
            {
                var detalleVenta = new DetalleVenta
                {
                    IdVenta = venta.IdVenta,
                    IdElementoCatalogo = item.IdElementoCatalogo,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = item.Subtotal
                };

                _context.DetalleVenta.Add(detalleVenta);
            }

            var estadoConvertida = await _context.EstadoCotizacion
                .Where(e => e.Nombre == "Convertida en venta")
                .Select(e => e.IdEstadoCotizacion)
                .FirstOrDefaultAsync();

            if (estadoConvertida != 0)
            {
                cotizacion.IdEstadoCotizacion = estadoConvertida;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return venta.IdVenta;
        }
        public async Task<int> ContarPendientesAsync()
        {
            return await _context.Cotizacion
                .CountAsync(c =>
                    _context.EstadoCotizacion.Any(e =>
                        e.IdEstadoCotizacion == c.IdEstadoCotizacion &&
                        (
                            e.Nombre == "Pendiente" ||
                            e.Nombre == "En revisión"
                        )
                    )
                );
        }

        public async Task<List<ClienteSelectorDto>> ListarClientesAsync()
        {
            var naturales = await (
                from c in _context.Cliente
                join tc in _context.TipoCliente on c.IdTipoCliente equals tc.IdTipoCliente
                join td in _context.TipoDocumento on c.IdTipoDocumento equals td.IdTipoDocumento
                join pn in _context.ClientePersonaNatural on c.IdCliente equals pn.IdCliente
                where c.Estado
                select new ClienteSelectorDto
                {
                    IdCliente = c.IdCliente,
                    Cliente = pn.Nombres + " " + pn.ApellidoPaterno + " " + (pn.ApellidoMaterno ?? ""),
                    TipoCliente = tc.Nombre,
                    TipoDocumento = td.Nombre,
                    NumeroDocumento = c.NumeroDocumento
                }
            ).ToListAsync();

            var empresas = await (
                from c in _context.Cliente
                join tc in _context.TipoCliente on c.IdTipoCliente equals tc.IdTipoCliente
                join td in _context.TipoDocumento on c.IdTipoDocumento equals td.IdTipoDocumento
                join ce in _context.ClienteEmpresa on c.IdCliente equals ce.IdCliente
                where c.Estado
                select new ClienteSelectorDto
                {
                    IdCliente = c.IdCliente,
                    Cliente = ce.RazonSocial,
                    TipoCliente = tc.Nombre,
                    TipoDocumento = td.Nombre,
                    NumeroDocumento = c.NumeroDocumento
                }
            ).ToListAsync();

            return naturales
                .Concat(empresas)
                .OrderBy(c => c.Cliente)
                .ToList();
        }

        public async Task<List<ElementoCotizableDto>> ListarElementosCotizablesAsync()
        {
            var elementos = await (
                from ec in _context.ElementoCatalogo
                join te in _context.TipoElemento on ec.IdTipoElemento equals te.IdTipoElemento
                where ec.Estado &&
                      te.Estado &&
                      (
                          te.Nombre == "Producto" ||
                          te.Nombre == "Servicio"
                      )
                orderby te.Nombre, ec.Nombre
                select new ElementoCotizableDto
                {
                    IdElementoCatalogo = ec.IdElementoCatalogo,
                    Nombre = ec.Nombre,
                    TipoElemento = te.Nombre,
                    PrecioReferencial = ec.PrecioReferencial
                }
            ).ToListAsync();

            return elementos;
        }

        public async Task<List<EstadoCotizacionDto>> ListarEstadosAsync()
        {
            return await _context.EstadoCotizacion
                .Where(e => e.Estado)
                .OrderBy(e => e.IdEstadoCotizacion)
                .Select(e => new EstadoCotizacionDto
                {
                    IdEstadoCotizacion = e.IdEstadoCotizacion,
                    Nombre = e.Nombre
                })
                .ToListAsync();
        }

        private async Task RegistrarDetallesAsync(
            int idCotizacion,
            List<CotizacionDetalleCrearDto> detalles
        )
        {
            foreach (var item in detalles)
            {
                var subtotal = Math.Round(item.Cantidad * item.PrecioUnitario, 2);

                var detalle = new DetalleCotizacion
                {
                    IdCotizacion = idCotizacion,
                    IdElementoCatalogo = item.IdElementoCatalogo,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = subtotal,
                    Observacion = NormalizarTexto(item.Observacion)
                };

                _context.DetalleCotizacion.Add(detalle);
            }

            await _context.SaveChangesAsync();
        }

        private async Task RecalcularTotalesAsync(Cotizacion cotizacion, decimal descuento)
        {
            var subtotalConIgv = await _context.DetalleCotizacion
                .Where(d => d.IdCotizacion == cotizacion.IdCotizacion)
                .SumAsync(d => d.Subtotal);

            subtotalConIgv = Math.Round(subtotalConIgv, 2);

            if (descuento < 0)
            {
                descuento = 0;
            }

            if (descuento > subtotalConIgv)
            {
                descuento = subtotalConIgv;
            }

            descuento = Math.Round(descuento, 2);

            var total = Math.Round(subtotalConIgv - descuento, 2);
            var valorVenta = Math.Round(total / 1.18m, 2);
            var igvIncluido = Math.Round(total - valorVenta, 2);

            cotizacion.Subtotal = subtotalConIgv;
            cotizacion.Descuento = descuento;
            cotizacion.Igv = igvIncluido;
            cotizacion.Total = total;
            cotizacion.TotalReferencial = total;
        }

        private async Task ValidarCrearActualizarAsync(
            int idCliente,
            int idEstadoCotizacion,
            string origenCotizacion,
            decimal descuento,
            List<CotizacionDetalleCrearDto> detalles
        )
        {
            var existeCliente = await _context.Cliente
                .AnyAsync(c => c.IdCliente == idCliente && c.Estado);

            if (!existeCliente)
            {
                throw new InvalidOperationException("Selecciona un cliente válido.");
            }

            var existeEstado = await _context.EstadoCotizacion
                .AnyAsync(e => e.IdEstadoCotizacion == idEstadoCotizacion && e.Estado);

            if (!existeEstado)
            {
                throw new InvalidOperationException("Selecciona un estado de cotización válido.");
            }

            var origen = NormalizarOrigen(origenCotizacion);

            var origenValido =
                origen == "Manual" ||
                origen == "Web" ||
                origen == "WhatsApp" ||
                origen == "Correo";

            if (!origenValido)
            {
                throw new InvalidOperationException("El origen de la cotización no es válido.");
            }

            if (descuento < 0)
            {
                throw new InvalidOperationException("El descuento no puede ser negativo.");
            }

            if (detalles == null || detalles.Count == 0)
            {
                throw new InvalidOperationException("Agrega al menos un producto o servicio a la cotización.");
            }

            decimal subtotalTemporal = 0;

            foreach (var detalle in detalles)
            {
                if (detalle.IdElementoCatalogo <= 0)
                {
                    throw new InvalidOperationException("Selecciona un producto o servicio válido.");
                }

                if (detalle.Cantidad <= 0)
                {
                    throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
                }

                if (detalle.PrecioUnitario < 0)
                {
                    throw new InvalidOperationException("El precio unitario no puede ser negativo.");
                }

                var existeElemento = await _context.ElementoCatalogo
                    .AnyAsync(e => e.IdElementoCatalogo == detalle.IdElementoCatalogo && e.Estado);

                if (!existeElemento)
                {
                    throw new InvalidOperationException("Uno de los productos o servicios seleccionados no está activo.");
                }

                subtotalTemporal += detalle.Cantidad * detalle.PrecioUnitario;
            }

            if (descuento > subtotalTemporal)
            {
                throw new InvalidOperationException("El descuento no puede ser mayor al subtotal.");
            }
        }

        private async Task<CotizacionListadoDto> MapearCotizacionAsync(
            Cotizacion cotizacion,
            bool incluirDetalles
        )
        {
            var cliente = await ObtenerClienteAsync(cotizacion.IdCliente);

            var estado = await _context.EstadoCotizacion
                .Where(e => e.IdEstadoCotizacion == cotizacion.IdEstadoCotizacion)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync() ?? "Sin estado";

            var cantidadDetalles = await _context.DetalleCotizacion
                .CountAsync(d => d.IdCotizacion == cotizacion.IdCotizacion);

            var dto = new CotizacionListadoDto
            {
                IdCotizacion = cotizacion.IdCotizacion,

                IdCliente = cotizacion.IdCliente,
                Cliente = cliente.Cliente,
                DocumentoCliente = $"{cliente.TipoDocumento} {cliente.NumeroDocumento}",
                TipoCliente = cliente.TipoCliente,

                IdEstadoCotizacion = cotizacion.IdEstadoCotizacion,
                EstadoCotizacion = estado,

                OrigenCotizacion = cotizacion.OrigenCotizacion,

                FechaCotizacion = cotizacion.FechaCotizacion,

                Subtotal = cotizacion.Subtotal,
                Descuento = cotizacion.Descuento,
                Igv = cotizacion.Igv,
                Total = cotizacion.Total,

                TotalReferencial = cotizacion.TotalReferencial,

                Observacion = cotizacion.Observacion,
                ArchivoPdf = cotizacion.ArchivoPdf,
                CorreoEnviado = cotizacion.CorreoEnviado,

                CantidadDetalles = cantidadDetalles
            };

            if (incluirDetalles)
            {
                dto.Detalles = await ListarDetallesAsync(cotizacion.IdCotizacion);
            }

            return dto;
        }

        private async Task<List<CotizacionDetalleDto>> ListarDetallesAsync(int idCotizacion)
        {
            return await (
                from d in _context.DetalleCotizacion
                join ec in _context.ElementoCatalogo on d.IdElementoCatalogo equals ec.IdElementoCatalogo
                join te in _context.TipoElemento on ec.IdTipoElemento equals te.IdTipoElemento
                where d.IdCotizacion == idCotizacion
                orderby d.IdDetalleCotizacion
                select new CotizacionDetalleDto
                {
                    IdDetalleCotizacion = d.IdDetalleCotizacion,
                    IdElementoCatalogo = d.IdElementoCatalogo,
                    ElementoNombre = ec.Nombre,
                    TipoElemento = te.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Observacion = d.Observacion
                }
            ).ToListAsync();
        }

        private async Task<ClienteSelectorDto> ObtenerClienteAsync(int idCliente)
        {
            var natural = await (
                from c in _context.Cliente
                join tc in _context.TipoCliente on c.IdTipoCliente equals tc.IdTipoCliente
                join td in _context.TipoDocumento on c.IdTipoDocumento equals td.IdTipoDocumento
                join pn in _context.ClientePersonaNatural on c.IdCliente equals pn.IdCliente
                where c.IdCliente == idCliente
                select new ClienteSelectorDto
                {
                    IdCliente = c.IdCliente,
                    Cliente = pn.Nombres + " " + pn.ApellidoPaterno + " " + (pn.ApellidoMaterno ?? ""),
                    TipoCliente = tc.Nombre,
                    TipoDocumento = td.Nombre,
                    NumeroDocumento = c.NumeroDocumento
                }
            ).FirstOrDefaultAsync();

            if (natural != null)
            {
                return natural;
            }

            var empresa = await (
                from c in _context.Cliente
                join tc in _context.TipoCliente on c.IdTipoCliente equals tc.IdTipoCliente
                join td in _context.TipoDocumento on c.IdTipoDocumento equals td.IdTipoDocumento
                join ce in _context.ClienteEmpresa on c.IdCliente equals ce.IdCliente
                where c.IdCliente == idCliente
                select new ClienteSelectorDto
                {
                    IdCliente = c.IdCliente,
                    Cliente = ce.RazonSocial,
                    TipoCliente = tc.Nombre,
                    TipoDocumento = td.Nombre,
                    NumeroDocumento = c.NumeroDocumento
                }
            ).FirstOrDefaultAsync();

            return empresa ?? new ClienteSelectorDto
            {
                IdCliente = idCliente,
                Cliente = "Cliente no encontrado",
                TipoCliente = "Sin tipo",
                TipoDocumento = "Doc.",
                NumeroDocumento = "-"
            };
        }

        private static string? NormalizarTexto(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            return valor.Trim();
        }

        private static string NormalizarOrigen(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return "Manual";
            }

            var origen = valor.Trim();

            if (origen.Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                return "Web";
            }

            if (origen.Equals("whatsapp", StringComparison.OrdinalIgnoreCase))
            {
                return "WhatsApp";
            }

            if (origen.Equals("correo", StringComparison.OrdinalIgnoreCase))
            {
                return "Correo";
            }

            return "Manual";
        }
        

       

        private string LimpiarTelefonoWhatsApp(string telefono)
        {
            var limpio = new string(
                telefono.Where(char.IsDigit).ToArray()
            );

            if (limpio.Length == 9)
            {
                limpio = "51" + limpio;
            }

            return limpio;
        }
    }
}