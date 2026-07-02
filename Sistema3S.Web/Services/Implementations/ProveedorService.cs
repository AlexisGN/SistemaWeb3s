using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Proveedor;
using Sistema3S.Web.Models;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class ProveedorService : IProveedorService
    {
        private readonly Bd3sContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ProveedorService(
            Bd3sContext context,
            HttpClient httpClient,
            IConfiguration configuration
        )
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ResultadoPaginadoDto<ProveedorListadoDto>> ListarAsync(
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

            var query = _context.Proveedor
                .AsNoTracking()
                .Include(p => p.IdUbigeoNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(p =>
                    p.Ruc.Contains(texto) ||
                    p.RazonSocial.Contains(texto) ||
                    (
                        p.NombreComercial != null &&
                        p.NombreComercial.Contains(texto)
                    ) ||
                    (
                        p.Correo != null &&
                        p.Correo.Contains(texto)
                    ) ||
                    (
                        p.Telefono != null &&
                        p.Telefono.Contains(texto)
                    )
                );
            }

            var totalRegistros = await query.CountAsync();

            var proveedores = await query
                .OrderByDescending(p => p.IdProveedor)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(p => new ProveedorListadoDto
                {
                    IdProveedor = p.IdProveedor,

                    IdUbigeo = p.IdUbigeo,
                    Departamento = p.IdUbigeoNavigation != null
                        ? p.IdUbigeoNavigation.Departamento
                        : null,
                    Provincia = p.IdUbigeoNavigation != null
                        ? p.IdUbigeoNavigation.Provincia
                        : null,
                    Distrito = p.IdUbigeoNavigation != null
                        ? p.IdUbigeoNavigation.Distrito
                        : null,
                    Ubicacion = p.IdUbigeoNavigation != null
                        ? p.IdUbigeoNavigation.Departamento + " - " +
                          p.IdUbigeoNavigation.Provincia + " - " +
                          p.IdUbigeoNavigation.Distrito
                        : null,

                    Ruc = p.Ruc,
                    RazonSocial = p.RazonSocial,
                    NombreComercial = p.NombreComercial,

                    Correo = p.Correo,
                    Telefono = p.Telefono,
                    Direccion = p.Direccion,
                    Estado = p.Estado,

                    ContactoPrincipal = _context.ContactoProveedor
                        .AsNoTracking()
                        .Where(cp => cp.IdProveedor == p.IdProveedor && cp.Estado)
                        .OrderBy(cp => cp.IdContactoProveedor)
                        .Select(cp => new ContactoProveedorDto
                        {
                            IdContactoProveedor = cp.IdContactoProveedor,
                            Nombres = cp.Nombres,
                            ApellidoPaterno = cp.ApellidoPaterno,
                            ApellidoMaterno = cp.ApellidoMaterno,
                            Cargo = cp.Cargo,
                            Correo = cp.Correo,
                            Telefono = cp.Telefono,
                            Estado = cp.Estado
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return new ResultadoPaginadoDto<ProveedorListadoDto>
            {
                Items = proveedores,
                Pagina = pagina,
                TamanioPagina = tamanioPagina,
                TotalRegistros = totalRegistros
            };
        }

        public async Task<ProveedorListadoDto?> ObtenerPorIdAsync(int idProveedor)
        {
            var proveedor = await _context.Proveedor
                .AsNoTracking()
                .Include(p => p.IdUbigeoNavigation)
                .FirstOrDefaultAsync(p => p.IdProveedor == idProveedor);

            if (proveedor == null)
            {
                return null;
            }

            var contacto = await _context.ContactoProveedor
                .AsNoTracking()
                .Where(cp => cp.IdProveedor == idProveedor && cp.Estado)
                .OrderBy(cp => cp.IdContactoProveedor)
                .Select(cp => new ContactoProveedorDto
                {
                    IdContactoProveedor = cp.IdContactoProveedor,
                    Nombres = cp.Nombres,
                    ApellidoPaterno = cp.ApellidoPaterno,
                    ApellidoMaterno = cp.ApellidoMaterno,
                    Cargo = cp.Cargo,
                    Correo = cp.Correo,
                    Telefono = cp.Telefono,
                    Estado = cp.Estado
                })
                .FirstOrDefaultAsync();

            return MapearProveedor(proveedor, contacto);
        }

        public async Task<ProveedorListadoDto> CrearAsync(ProveedorCrearDto dto)
        {
            ValidarCrear(dto);

            var ruc = LimpiarNumeros(dto.Ruc);

            var existeRuc = await _context.Proveedor
                .AnyAsync(p => p.Ruc == ruc);

            if (existeRuc)
            {
                throw new InvalidOperationException("Ya existe un proveedor registrado con ese RUC.");
            }

            await ValidarUbigeoExisteAsync(dto.IdUbigeo);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var proveedor = new Proveedor
            {
                IdUbigeo = dto.IdUbigeo,
                Ruc = ruc,
                RazonSocial = dto.RazonSocial.Trim(),
                NombreComercial = NormalizarTexto(dto.NombreComercial),
                Correo = dto.Correo.Trim(),
                Telefono = dto.Telefono.Trim(),
                Direccion = dto.Direccion.Trim(),
                Estado = true
            };

            _context.Proveedor.Add(proveedor);
            await _context.SaveChangesAsync();

            if (dto.RegistrarContactoPrincipal)
            {
                if (dto.ContactoPrincipal == null)
                {
                    throw new InvalidOperationException("Ingresa los datos del contacto principal.");
                }

                ValidarContacto(dto.ContactoPrincipal);

                var contacto = new ContactoProveedor
                {
                    IdProveedor = proveedor.IdProveedor,
                    Nombres = dto.ContactoPrincipal.Nombres.Trim(),
                    ApellidoPaterno = dto.ContactoPrincipal.ApellidoPaterno.Trim(),
                    ApellidoMaterno = NormalizarTexto(dto.ContactoPrincipal.ApellidoMaterno),
                    Cargo = NormalizarTexto(dto.ContactoPrincipal.Cargo),
                    Correo = NormalizarTexto(dto.ContactoPrincipal.Correo),
                    Telefono = NormalizarTexto(dto.ContactoPrincipal.Telefono),
                    Estado = true
                };

                _context.ContactoProveedor.Add(contacto);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            var proveedorCreado = await ObtenerPorIdAsync(proveedor.IdProveedor);

            if (proveedorCreado == null)
            {
                throw new InvalidOperationException("No se pudo recuperar el proveedor registrado.");
            }

            return proveedorCreado;
        }

        public async Task<bool> ActualizarAsync(int idProveedor, ProveedorActualizarDto dto)
        {
            ValidarActualizar(dto);

            var proveedor = await _context.Proveedor
                .FirstOrDefaultAsync(p => p.IdProveedor == idProveedor);

            if (proveedor == null)
            {
                return false;
            }

            var ruc = LimpiarNumeros(dto.Ruc);

            var existeRuc = await _context.Proveedor
                .AnyAsync(p =>
                    p.IdProveedor != idProveedor &&
                    p.Ruc == ruc
                );

            if (existeRuc)
            {
                throw new InvalidOperationException("Ya existe otro proveedor registrado con ese RUC.");
            }

            await ValidarUbigeoExisteAsync(dto.IdUbigeo);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            proveedor.IdUbigeo = dto.IdUbigeo;
            proveedor.Ruc = ruc;
            proveedor.RazonSocial = dto.RazonSocial.Trim();
            proveedor.NombreComercial = NormalizarTexto(dto.NombreComercial);
            proveedor.Correo = dto.Correo.Trim();
            proveedor.Telefono = dto.Telefono.Trim();
            proveedor.Direccion = dto.Direccion.Trim();
            proveedor.Estado = dto.Estado;

            var contactoActual = await _context.ContactoProveedor
                .Where(cp => cp.IdProveedor == idProveedor && cp.Estado)
                .OrderBy(cp => cp.IdContactoProveedor)
                .FirstOrDefaultAsync();

            if (dto.RegistrarContactoPrincipal)
            {
                if (dto.ContactoPrincipal == null)
                {
                    throw new InvalidOperationException("Ingresa los datos del contacto principal.");
                }

                ValidarContacto(dto.ContactoPrincipal);

                if (contactoActual == null)
                {
                    var nuevoContacto = new ContactoProveedor
                    {
                        IdProveedor = idProveedor,
                        Nombres = dto.ContactoPrincipal.Nombres.Trim(),
                        ApellidoPaterno = dto.ContactoPrincipal.ApellidoPaterno.Trim(),
                        ApellidoMaterno = NormalizarTexto(dto.ContactoPrincipal.ApellidoMaterno),
                        Cargo = NormalizarTexto(dto.ContactoPrincipal.Cargo),
                        Correo = NormalizarTexto(dto.ContactoPrincipal.Correo),
                        Telefono = NormalizarTexto(dto.ContactoPrincipal.Telefono),
                        Estado = true
                    };

                    _context.ContactoProveedor.Add(nuevoContacto);
                }
                else
                {
                    contactoActual.Nombres = dto.ContactoPrincipal.Nombres.Trim();
                    contactoActual.ApellidoPaterno = dto.ContactoPrincipal.ApellidoPaterno.Trim();
                    contactoActual.ApellidoMaterno = NormalizarTexto(dto.ContactoPrincipal.ApellidoMaterno);
                    contactoActual.Cargo = NormalizarTexto(dto.ContactoPrincipal.Cargo);
                    contactoActual.Correo = NormalizarTexto(dto.ContactoPrincipal.Correo);
                    contactoActual.Telefono = NormalizarTexto(dto.ContactoPrincipal.Telefono);
                    contactoActual.Estado = true;
                }
            }
            else
            {
                var contactos = await _context.ContactoProveedor
                    .Where(cp => cp.IdProveedor == idProveedor && cp.Estado)
                    .ToListAsync();

                foreach (var contacto in contactos)
                {
                    contacto.Estado = false;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }

        public async Task<bool> EliminarLogicoAsync(int idProveedor)
        {
            var proveedor = await _context.Proveedor
                .FirstOrDefaultAsync(p => p.IdProveedor == idProveedor);

            if (proveedor == null)
            {
                return false;
            }

            proveedor.Estado = false;

            var contactos = await _context.ContactoProveedor
                .Where(cp => cp.IdProveedor == idProveedor && cp.Estado)
                .ToListAsync();

            foreach (var contacto in contactos)
            {
                contacto.Estado = false;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> ContarActivosAsync()
        {
            return await _context.Proveedor
                .CountAsync(p => p.Estado);
        }

        public async Task<ConsultaRucProveedorResultadoDto> ConsultarRucAsync(string ruc)
        {
            ruc = LimpiarNumeros(ruc);

            if (ruc.Length != 11)
            {
                return new ConsultaRucProveedorResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = ruc,
                    Mensaje = "El RUC debe tener 11 dígitos."
                };
            }

            var proveedorExistente = await _context.Proveedor
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Ruc == ruc);

            if (proveedorExistente != null)
            {
                return new ConsultaRucProveedorResultadoDto
                {
                    Exitoso = true,
                    ProveedorYaExiste = true,
                    IdProveedorExistente = proveedorExistente.IdProveedor,
                    NumeroDocumento = ruc,
                    RazonSocial = proveedorExistente.RazonSocial,
                    NombreComercial = proveedorExistente.NombreComercial,
                    Mensaje = "Ya existe un proveedor registrado con ese RUC."
                };
            }

            var apiKey = _configuration["Decolecta:ApiKey"];
            var baseUrl = _configuration["Decolecta:BaseUrl"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new ConsultaRucProveedorResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = ruc,
                    Mensaje = "No se configuró la API key de Decolecta."
                };
            }

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "https://api.decolecta.com/v1/";
            }

            try
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.GetAsync($"sunat/ruc?numero={ruc}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ConsultaRucProveedorResultadoDto
                    {
                        Exitoso = false,
                        NumeroDocumento = ruc,
                        Mensaje = "No se encontraron datos para el RUC ingresado."
                    };
                }

                var json = await response.Content.ReadAsStringAsync();

                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var razonSocial = ObtenerString(root, "razon_social");
                var numeroDocumento = ObtenerString(root, "numero_documento") ?? ruc;
                var estado = ObtenerString(root, "estado");
                var condicion = ObtenerString(root, "condicion");
                var direccion = ObtenerString(root, "direccion");
                var codigoUbigeo = ObtenerString(root, "ubigeo");
                var distrito = ObtenerString(root, "distrito");
                var provincia = ObtenerString(root, "provincia");
                var departamento = ObtenerString(root, "departamento");

                int? idUbigeo = null;
                string? ubicacion = null;

                if (!string.IsNullOrWhiteSpace(codigoUbigeo))
                {
                    var ubigeo = await _context.Ubigeo
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u =>
                            EF.Property<string?>(u, "CodigoUbigeo") == codigoUbigeo
                        );

                    if (ubigeo != null)
                    {
                        idUbigeo = ubigeo.IdUbigeo;
                        ubicacion = $"{ubigeo.Departamento} - {ubigeo.Provincia} - {ubigeo.Distrito}";
                    }
                }

                return new ConsultaRucProveedorResultadoDto
                {
                    Exitoso = true,
                    NumeroDocumento = numeroDocumento,
                    RazonSocial = razonSocial,
                    NombreComercial = razonSocial,
                    EstadoSunat = estado,
                    CondicionSunat = condicion,
                    Direccion = direccion,
                    CodigoUbigeo = codigoUbigeo,
                    IdUbigeo = idUbigeo,
                    Ubicacion = ubicacion,
                    Departamento = departamento,
                    Provincia = provincia,
                    Distrito = distrito,
                    Mensaje = "Datos SUNAT cargados correctamente."
                };
            }
            catch
            {
                return new ConsultaRucProveedorResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = ruc,
                    Mensaje = "No se pudo consultar SUNAT en este momento."
                };
            }
        }

        private static ProveedorListadoDto MapearProveedor(
            Proveedor proveedor,
            ContactoProveedorDto? contacto
        )
        {
            return new ProveedorListadoDto
            {
                IdProveedor = proveedor.IdProveedor,

                IdUbigeo = proveedor.IdUbigeo,
                Departamento = proveedor.IdUbigeoNavigation?.Departamento,
                Provincia = proveedor.IdUbigeoNavigation?.Provincia,
                Distrito = proveedor.IdUbigeoNavigation?.Distrito,
                Ubicacion = proveedor.IdUbigeoNavigation != null
                    ? $"{proveedor.IdUbigeoNavigation.Departamento} - {proveedor.IdUbigeoNavigation.Provincia} - {proveedor.IdUbigeoNavigation.Distrito}"
                    : null,

                Ruc = proveedor.Ruc,
                RazonSocial = proveedor.RazonSocial,
                NombreComercial = proveedor.NombreComercial,

                Correo = proveedor.Correo,
                Telefono = proveedor.Telefono,
                Direccion = proveedor.Direccion,

                Estado = proveedor.Estado,
                ContactoPrincipal = contacto
            };
        }

        private static void ValidarCrear(ProveedorCrearDto dto)
        {
            ValidarProveedorBase(
                dto.Ruc,
                dto.RazonSocial,
                dto.Correo,
                dto.Telefono,
                dto.Direccion,
                dto.IdUbigeo
            );

            if (dto.RegistrarContactoPrincipal)
            {
                if (dto.ContactoPrincipal == null)
                {
                    throw new InvalidOperationException("Ingresa los datos del contacto principal.");
                }

                ValidarContacto(dto.ContactoPrincipal);
            }
        }

        private static void ValidarActualizar(ProveedorActualizarDto dto)
        {
            ValidarProveedorBase(
                dto.Ruc,
                dto.RazonSocial,
                dto.Correo,
                dto.Telefono,
                dto.Direccion,
                dto.IdUbigeo
            );

            if (dto.RegistrarContactoPrincipal)
            {
                if (dto.ContactoPrincipal == null)
                {
                    throw new InvalidOperationException("Ingresa los datos del contacto principal.");
                }

                ValidarContacto(dto.ContactoPrincipal);
            }
        }

        private static void ValidarProveedorBase(
            string ruc,
            string razonSocial,
            string correo,
            string telefono,
            string direccion,
            int? idUbigeo
        )
        {
            var rucLimpio = LimpiarNumeros(ruc);

            if (string.IsNullOrWhiteSpace(rucLimpio))
            {
                throw new InvalidOperationException("El RUC del proveedor es obligatorio.");
            }

            if (rucLimpio.Length != 11)
            {
                throw new InvalidOperationException("El RUC debe tener 11 dígitos.");
            }

            if (!SoloNumeros(rucLimpio))
            {
                throw new InvalidOperationException("El RUC solo debe contener números.");
            }

            if (string.IsNullOrWhiteSpace(razonSocial))
            {
                throw new InvalidOperationException("La razón social del proveedor es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new InvalidOperationException("El correo del proveedor es obligatorio.");
            }

            if (!EsCorreoValido(correo))
            {
                throw new InvalidOperationException("Ingresa un correo válido para el proveedor.");
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                throw new InvalidOperationException("El teléfono del proveedor es obligatorio.");
            }

            if (!EsTelefonoPeruValido(telefono))
            {
                throw new InvalidOperationException("Formato válido de teléfono: +51 seguido de 9 dígitos.");
            }

            if (string.IsNullOrWhiteSpace(direccion))
            {
                throw new InvalidOperationException("La dirección del proveedor es obligatoria.");
            }

            if (!idUbigeo.HasValue || idUbigeo.Value <= 0)
            {
                throw new InvalidOperationException("Selecciona la ubicación del proveedor.");
            }
        }

        private static void ValidarContacto(ContactoProveedorDto contacto)
        {
            if (string.IsNullOrWhiteSpace(contacto.Nombres))
            {
                throw new InvalidOperationException("Ingresa los nombres del contacto.");
            }

            if (!EsTextoSoloLetras(contacto.Nombres))
            {
                throw new InvalidOperationException("Los nombres del contacto solo deben contener letras y espacios.");
            }

            if (string.IsNullOrWhiteSpace(contacto.ApellidoPaterno))
            {
                throw new InvalidOperationException("Ingresa el apellido paterno del contacto.");
            }

            if (!EsTextoSoloLetras(contacto.ApellidoPaterno))
            {
                throw new InvalidOperationException("El apellido paterno del contacto solo debe contener letras y espacios.");
            }

            if (
                !string.IsNullOrWhiteSpace(contacto.ApellidoMaterno) &&
                !EsTextoSoloLetras(contacto.ApellidoMaterno)
            )
            {
                throw new InvalidOperationException("El apellido materno del contacto solo debe contener letras y espacios.");
            }

            if (
                !string.IsNullOrWhiteSpace(contacto.Correo) &&
                !EsCorreoValido(contacto.Correo)
            )
            {
                throw new InvalidOperationException("Ingresa un correo válido para el contacto.");
            }

            if (
                !string.IsNullOrWhiteSpace(contacto.Telefono) &&
                !EsTelefonoPeruValido(contacto.Telefono)
            )
            {
                throw new InvalidOperationException("Formato válido del teléfono del contacto: +51 seguido de 9 dígitos.");
            }
        }

        private async Task ValidarUbigeoExisteAsync(int? idUbigeo)
        {
            if (!idUbigeo.HasValue || idUbigeo.Value <= 0)
            {
                throw new InvalidOperationException("Selecciona la ubicación del proveedor.");
            }

            var existe = await _context.Ubigeo
                .AnyAsync(u => u.IdUbigeo == idUbigeo.Value);

            if (!existe)
            {
                throw new InvalidOperationException("La ubicación seleccionada no existe.");
            }
        }

        private static string LimpiarNumeros(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            return new string(valor.Where(char.IsDigit).ToArray());
        }

        private static bool SoloNumeros(string valor)
        {
            return Regex.IsMatch(valor, @"^\d+$");
        }

        private static bool EsCorreoValido(string correo)
        {
            return Regex.IsMatch(
                correo.Trim(),
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase
            );
        }

        private static bool EsTelefonoPeruValido(string telefono)
        {
            return Regex.IsMatch(telefono.Trim(), @"^\+51\d{9}$");
        }

        private static bool EsTextoSoloLetras(string valor)
        {
            return Regex.IsMatch(
                valor.Trim(),
                @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"
            );
        }

        private static string? NormalizarTexto(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            return valor.Trim();
        }

        private static string? ObtenerString(JsonElement root, string propiedad)
        {
            if (!root.TryGetProperty(propiedad, out var valor))
            {
                return null;
            }

            if (valor.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return valor.GetString();
        }
    }
}