using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Servicio;
using Sistema3S.Web.Models;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class ServicioService : IServicioService
    {
        private readonly Bd3sContext _context;

        public ServicioService(Bd3sContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<ServicioListadoDto>> ListarAsync(
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

            var idTipoServicio = await ObtenerIdTipoServicioAsync();

            var query = _context.Servicio
                .Include(s => s.IdElementoCatalogoNavigation)
                .Where(s => s.IdElementoCatalogoNavigation.IdTipoElemento == idTipoServicio)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(s =>
                    s.IdElementoCatalogoNavigation.Nombre.Contains(texto) ||
                    (
                        s.IdElementoCatalogoNavigation.Descripcion != null &&
                        s.IdElementoCatalogoNavigation.Descripcion.Contains(texto)
                    ) ||
                    (
                        s.SectorAplicacion != null &&
                        s.SectorAplicacion.Contains(texto)
                    )
                );
            }

            var totalRegistros = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.IdServicio)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(s => new ServicioListadoDto
                {
                    IdServicio = s.IdServicio,
                    IdElementoCatalogo = s.IdElementoCatalogo,

                    Nombre = s.IdElementoCatalogoNavigation.Nombre,
                    Descripcion = s.IdElementoCatalogoNavigation.Descripcion,
                    PrecioReferencial = s.IdElementoCatalogoNavigation.PrecioReferencial,
                    ImagenUrl = s.IdElementoCatalogoNavigation.ImagenUrl,

                    SectorAplicacion = s.SectorAplicacion,
                    MensajeWhatsApp = s.MensajeWhatsApp,
                    RequiereVisitaTecnica = s.RequiereVisitaTecnica,

                    Estado = s.IdElementoCatalogoNavigation.Estado
                })
                .ToListAsync();

            return new ResultadoPaginadoDto<ServicioListadoDto>
            {
                Items = items,
                Pagina = pagina,
                TamanioPagina = tamanioPagina,
                TotalRegistros = totalRegistros
            };
        }

        public async Task<ServicioListadoDto?> ObtenerPorIdAsync(int idServicio)
        {
            var servicio = await _context.Servicio
                .Include(s => s.IdElementoCatalogoNavigation)
                .FirstOrDefaultAsync(s => s.IdServicio == idServicio);

            if (servicio == null)
            {
                return null;
            }

            return MapearServicio(servicio);
        }

        public async Task<ServicioListadoDto> CrearAsync(ServicioCrearDto dto)
        {
            ValidarCrear(dto);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var idTipoServicio = await ObtenerIdTipoServicioAsync();

            var elemento = new ElementoCatalogo
            {
                IdTipoElemento = idTipoServicio,
                Nombre = dto.Nombre.Trim(),
                Descripcion = NormalizarTexto(dto.Descripcion),
                PrecioReferencial = dto.PrecioReferencial,
                ImagenUrl = NormalizarTexto(dto.ImagenUrl),
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            _context.ElementoCatalogo.Add(elemento);
            await _context.SaveChangesAsync();

            var servicio = new Servicio
            {
                IdElementoCatalogo = elemento.IdElementoCatalogo,
                SectorAplicacion = NormalizarTexto(dto.SectorAplicacion),
                MensajeWhatsApp = ObtenerMensajeWhatsApp(dto.MensajeWhatsApp, dto.Nombre),
                RequiereVisitaTecnica = dto.RequiereVisitaTecnica
            };

            _context.Servicio.Add(servicio);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var servicioCreado = await ObtenerPorIdAsync(servicio.IdServicio);

            if (servicioCreado == null)
            {
                throw new InvalidOperationException("No se pudo recuperar el servicio creado.");
            }

            return servicioCreado;
        }

        public async Task<bool> ActualizarAsync(int idServicio, ServicioActualizarDto dto)
        {
            ValidarActualizar(dto);

            var servicio = await _context.Servicio
                .Include(s => s.IdElementoCatalogoNavigation)
                .FirstOrDefaultAsync(s => s.IdServicio == idServicio);

            if (servicio == null)
            {
                return false;
            }

            servicio.IdElementoCatalogoNavigation.Nombre = dto.Nombre.Trim();
            servicio.IdElementoCatalogoNavigation.Descripcion = NormalizarTexto(dto.Descripcion);
            servicio.IdElementoCatalogoNavigation.PrecioReferencial = dto.PrecioReferencial;
            servicio.IdElementoCatalogoNavigation.ImagenUrl = NormalizarTexto(dto.ImagenUrl);
            servicio.IdElementoCatalogoNavigation.Estado = dto.Estado;

            servicio.SectorAplicacion = NormalizarTexto(dto.SectorAplicacion);
            servicio.MensajeWhatsApp = ObtenerMensajeWhatsApp(dto.MensajeWhatsApp, dto.Nombre);
            servicio.RequiereVisitaTecnica = dto.RequiereVisitaTecnica;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EliminarLogicoAsync(int idServicio)
        {
            var servicio = await _context.Servicio
                .Include(s => s.IdElementoCatalogoNavigation)
                .FirstOrDefaultAsync(s => s.IdServicio == idServicio);

            if (servicio == null)
            {
                return false;
            }

            servicio.IdElementoCatalogoNavigation.Estado = false;

            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<int> ObtenerIdTipoServicioAsync()
        {
            var idTipoServicio = await _context.TipoElemento
                .Where(t => t.Nombre == "Servicio" && t.Estado)
                .Select(t => t.IdTipoElemento)
                .FirstOrDefaultAsync();

            if (idTipoServicio == 0)
            {
                throw new InvalidOperationException("No existe el tipo de elemento Servicio en la base de datos.");
            }

            return idTipoServicio;
        }

        private static ServicioListadoDto MapearServicio(Servicio servicio)
        {
            return new ServicioListadoDto
            {
                IdServicio = servicio.IdServicio,
                IdElementoCatalogo = servicio.IdElementoCatalogo,

                Nombre = servicio.IdElementoCatalogoNavigation.Nombre,
                Descripcion = servicio.IdElementoCatalogoNavigation.Descripcion,
                PrecioReferencial = servicio.IdElementoCatalogoNavigation.PrecioReferencial,
                ImagenUrl = servicio.IdElementoCatalogoNavigation.ImagenUrl,

                SectorAplicacion = servicio.SectorAplicacion,
                MensajeWhatsApp = servicio.MensajeWhatsApp,
                RequiereVisitaTecnica = servicio.RequiereVisitaTecnica,

                Estado = servicio.IdElementoCatalogoNavigation.Estado
            };
        }

        private static void ValidarCrear(ServicioCrearDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                throw new InvalidOperationException("El nombre del servicio es obligatorio.");
            }

            if (dto.PrecioReferencial.HasValue && dto.PrecioReferencial.Value < 0)
            {
                throw new InvalidOperationException("El precio referencial no puede ser negativo.");
            }
        }

        private static void ValidarActualizar(ServicioActualizarDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                throw new InvalidOperationException("El nombre del servicio es obligatorio.");
            }

            if (dto.PrecioReferencial.HasValue && dto.PrecioReferencial.Value < 0)
            {
                throw new InvalidOperationException("El precio referencial no puede ser negativo.");
            }
        }

        private static string? NormalizarTexto(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            return valor.Trim();
        }

        private static string ObtenerMensajeWhatsApp(string? mensaje, string nombreServicio)
        {
            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                return mensaje.Trim();
            }

            return $"Hola, deseo solicitar información sobre el servicio de {nombreServicio.Trim()}.";
        }
        public async Task<int> ContarActivosAsync()
        {
            var idTipoServicio = await ObtenerIdTipoServicioAsync();

            return await _context.Servicio
                .Include(s => s.IdElementoCatalogoNavigation)
                .CountAsync(s =>
                    s.IdElementoCatalogoNavigation.IdTipoElemento == idTipoServicio &&
                    s.IdElementoCatalogoNavigation.Estado
                );
        }
    }
}