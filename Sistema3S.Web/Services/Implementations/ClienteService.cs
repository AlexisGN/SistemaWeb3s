using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Cliente;
using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.Services.Interfaces;

using ClienteEntity = Sistema3S.Web.Models.Cliente;
using ClienteEmpresaEntity = Sistema3S.Web.Models.ClienteEmpresa;
using ClientePersonaNaturalEntity = Sistema3S.Web.Models.ClientePersonaNatural;
using ContactoClienteEntity = Sistema3S.Web.Models.ContactoCliente;

namespace Sistema3S.Web.Services.Implementations
{
    public class ClienteService : IClienteService
    {
        private readonly Bd3sContext _context;

        public ClienteService(Bd3sContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<ClienteListadoDto>> ListarAsync(
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

            var query =
                from c in _context.Cliente
                join tc in _context.TipoCliente
                    on c.IdTipoCliente equals tc.IdTipoCliente
                join td in _context.TipoDocumento
                    on c.IdTipoDocumento equals td.IdTipoDocumento
                join pn in _context.ClientePersonaNatural
                    on c.IdCliente equals pn.IdCliente into personasNaturales
                from pn in personasNaturales.DefaultIfEmpty()
                join ce in _context.ClienteEmpresa
                    on c.IdCliente equals ce.IdCliente into empresas
                from ce in empresas.DefaultIfEmpty()
                join u in _context.Ubigeo
                    on c.IdUbigeo equals u.IdUbigeo into ubicaciones
                from ub in ubicaciones.DefaultIfEmpty()
                select new
                {
                    ClienteBase = c,
                    TipoClienteNombre = tc.Nombre,
                    TipoDocumentoNombre = td.Nombre,
                    PersonaNatural = pn,
                    Empresa = ce,
                    Ubicacion = ub == null
                        ? null
                        : (ub.CodigoUbigeo ?? "------") + " - " +
                          ub.Departamento + " / " +
                          ub.Provincia + " / " +
                          ub.Distrito
                };

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(x =>
                    x.ClienteBase.NumeroDocumento.Contains(texto) ||
                    x.TipoClienteNombre.Contains(texto) ||
                    x.TipoDocumentoNombre.Contains(texto) ||

                    (x.ClienteBase.Correo != null &&
                     x.ClienteBase.Correo.Contains(texto)) ||

                    (x.ClienteBase.Telefono != null &&
                     x.ClienteBase.Telefono.Contains(texto)) ||

                    (x.ClienteBase.Direccion != null &&
                     x.ClienteBase.Direccion.Contains(texto)) ||

                    (x.PersonaNatural != null &&
                     (
                         x.PersonaNatural.Nombres.Contains(texto) ||
                         x.PersonaNatural.ApellidoPaterno.Contains(texto) ||
                         (x.PersonaNatural.ApellidoMaterno != null &&
                          x.PersonaNatural.ApellidoMaterno.Contains(texto))
                     )) ||

                    (x.Empresa != null &&
                     (
                         x.Empresa.RazonSocial.Contains(texto) ||
                         (x.Empresa.NombreComercial != null &&
                          x.Empresa.NombreComercial.Contains(texto))
                     )) ||

                    _context.ContactoCliente.Any(cc =>
                        cc.IdCliente == x.ClienteBase.IdCliente &&
                        cc.Estado &&
                        (
                            cc.Nombres.Contains(texto) ||
                            cc.ApellidoPaterno.Contains(texto) ||
                            (cc.ApellidoMaterno != null && cc.ApellidoMaterno.Contains(texto)) ||
                            (cc.Cargo != null && cc.Cargo.Contains(texto)) ||
                            (cc.Correo != null && cc.Correo.Contains(texto)) ||
                            (cc.Telefono != null && cc.Telefono.Contains(texto))
                        )
                    )
                );
            }

            var totalRegistros = await query.CountAsync();

            var datos = await query
                .OrderByDescending(x => x.ClienteBase.IdCliente)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();

            var idsClientes = datos
                .Select(x => x.ClienteBase.IdCliente)
                .ToList();

            var contactos = await _context.ContactoCliente
                .Where(c => idsClientes.Contains(c.IdCliente) && c.Estado)
                .OrderBy(c => c.IdContactoCliente)
                .ToListAsync();

            var items = datos.Select(x =>
            {
                var cliente = x.ClienteBase;

                var contacto = contactos
                    .FirstOrDefault(c => c.IdCliente == cliente.IdCliente);

                var esPersonaNatural = x.PersonaNatural != null;

                var nombreCliente = esPersonaNatural
                    ? $"{x.PersonaNatural!.Nombres} {x.PersonaNatural.ApellidoPaterno} {x.PersonaNatural.ApellidoMaterno}".Trim()
                    : x.Empresa?.RazonSocial ?? "Empresa sin razón social";

                return new ClienteListadoDto
                {
                    IdCliente = cliente.IdCliente,

                    IdTipoCliente = cliente.IdTipoCliente,
                    TipoCliente = x.TipoClienteNombre,

                    IdTipoDocumento = cliente.IdTipoDocumento,
                    TipoDocumento = x.TipoDocumentoNombre,

                    IdUbigeo = cliente.IdUbigeo,
                    Ubicacion = x.Ubicacion,

                    IdUsuario = cliente.IdUsuario,
                    CuentaWebVinculada = cliente.IdUsuario != null,

                    NumeroDocumento = cliente.NumeroDocumento,

                    Cliente = nombreCliente,

                    Nombres = x.PersonaNatural?.Nombres,
                    ApellidoPaterno = x.PersonaNatural?.ApellidoPaterno,
                    ApellidoMaterno = x.PersonaNatural?.ApellidoMaterno,

                    RazonSocial = x.Empresa?.RazonSocial,
                    NombreComercial = x.Empresa?.NombreComercial,

                    Correo = cliente.Correo,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,

                    Estado = cliente.Estado,
                    FechaRegistro = cliente.FechaRegistro,

                    TieneContactoPrincipal = contacto != null,

                    ContactoPrincipal = contacto == null
                        ? null
                        : new ClienteContactoDto
                        {
                            IdContactoCliente = contacto.IdContactoCliente,
                            Nombres = contacto.Nombres,
                            ApellidoPaterno = contacto.ApellidoPaterno,
                            ApellidoMaterno = contacto.ApellidoMaterno,
                            Cargo = contacto.Cargo,
                            Correo = contacto.Correo,
                            Telefono = contacto.Telefono,
                            Estado = contacto.Estado
                        }
                };
            }).ToList();

            return new ResultadoPaginadoDto<ClienteListadoDto>
            {
                Items = items,
                Pagina = pagina,
                TamanioPagina = tamanioPagina,
                TotalRegistros = totalRegistros
            };
        }

        public async Task<ClienteListadoDto?> ObtenerPorIdAsync(int idCliente)
        {
            var cliente = await _context.Cliente
                .FirstOrDefaultAsync(c => c.IdCliente == idCliente);

            if (cliente == null)
            {
                return null;
            }

            return await MapearClienteAsync(cliente);
        }

        public async Task<ClienteListadoDto> CrearAsync(ClienteCrearDto dto)
        {
            await ValidarCrearActualizarAsync(dto, null);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var cliente = new ClienteEntity
            {
                IdUsuario = null,
                IdTipoCliente = dto.IdTipoCliente,
                IdTipoDocumento = dto.IdTipoDocumento,
                IdUbigeo = dto.IdUbigeo,
                NumeroDocumento = NormalizarDocumento(dto.NumeroDocumento),
                Correo = NormalizarTexto(dto.Correo),
                Telefono = NormalizarTexto(dto.Telefono),
                Direccion = NormalizarTexto(dto.Direccion),
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            var tipoCliente = await ObtenerNombreTipoClienteAsync(dto.IdTipoCliente);

            if (EsPersonaNatural(tipoCliente))
            {
                var persona = new ClientePersonaNaturalEntity
                {
                    IdCliente = cliente.IdCliente,
                    Nombres = NormalizarTextoObligatorio(dto.Nombres),
                    ApellidoPaterno = NormalizarTextoObligatorio(dto.ApellidoPaterno),
                    ApellidoMaterno = NormalizarTexto(dto.ApellidoMaterno)
                };

                _context.ClientePersonaNatural.Add(persona);
            }
            else
            {
                var empresa = new ClienteEmpresaEntity
                {
                    IdCliente = cliente.IdCliente,
                    RazonSocial = NormalizarTextoObligatorio(dto.RazonSocial),
                    NombreComercial = NormalizarTexto(dto.NombreComercial)
                };

                _context.ClienteEmpresa.Add(empresa);

                if (dto.RegistrarContactoPrincipal && dto.ContactoPrincipal != null)
                {
                    _context.ContactoCliente.Add(CrearContacto(cliente.IdCliente, dto.ContactoPrincipal));
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var creado = await ObtenerPorIdAsync(cliente.IdCliente);

            if (creado == null)
            {
                throw new InvalidOperationException("No se pudo recuperar el cliente creado.");
            }

            return creado;
        }

        public async Task<bool> ActualizarAsync(int idCliente, ClienteActualizarDto dto)
        {
            var cliente = await _context.Cliente
                .FirstOrDefaultAsync(c => c.IdCliente == idCliente);

            if (cliente == null)
            {
                return false;
            }

            await ValidarCrearActualizarAsync(dto, idCliente);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            cliente.IdTipoCliente = dto.IdTipoCliente;
            cliente.IdTipoDocumento = dto.IdTipoDocumento;
            cliente.IdUbigeo = dto.IdUbigeo;
            cliente.NumeroDocumento = NormalizarDocumento(dto.NumeroDocumento);
            cliente.Correo = NormalizarTexto(dto.Correo);
            cliente.Telefono = NormalizarTexto(dto.Telefono);
            cliente.Direccion = NormalizarTexto(dto.Direccion);
            cliente.Estado = true;

            var tipoCliente = await ObtenerNombreTipoClienteAsync(dto.IdTipoCliente);

            if (EsPersonaNatural(tipoCliente))
            {
                var empresaActual = await _context.ClienteEmpresa
                    .FirstOrDefaultAsync(e => e.IdCliente == idCliente);

                if (empresaActual != null)
                {
                    _context.ClienteEmpresa.Remove(empresaActual);
                }

                var contactos = await _context.ContactoCliente
                    .Where(c => c.IdCliente == idCliente)
                    .ToListAsync();

                foreach (var contacto in contactos)
                {
                    contacto.Estado = false;
                }

                var personaActual = await _context.ClientePersonaNatural
                    .FirstOrDefaultAsync(p => p.IdCliente == idCliente);

                if (personaActual == null)
                {
                    personaActual = new ClientePersonaNaturalEntity
                    {
                        IdCliente = idCliente
                    };

                    _context.ClientePersonaNatural.Add(personaActual);
                }

                personaActual.Nombres = NormalizarTextoObligatorio(dto.Nombres);
                personaActual.ApellidoPaterno = NormalizarTextoObligatorio(dto.ApellidoPaterno);
                personaActual.ApellidoMaterno = NormalizarTexto(dto.ApellidoMaterno);
            }
            else
            {
                var personaActual = await _context.ClientePersonaNatural
                    .FirstOrDefaultAsync(p => p.IdCliente == idCliente);

                if (personaActual != null)
                {
                    _context.ClientePersonaNatural.Remove(personaActual);
                }

                var empresaActual = await _context.ClienteEmpresa
                    .FirstOrDefaultAsync(e => e.IdCliente == idCliente);

                if (empresaActual == null)
                {
                    empresaActual = new ClienteEmpresaEntity
                    {
                        IdCliente = idCliente
                    };

                    _context.ClienteEmpresa.Add(empresaActual);
                }

                empresaActual.RazonSocial = NormalizarTextoObligatorio(dto.RazonSocial);
                empresaActual.NombreComercial = NormalizarTexto(dto.NombreComercial);

                await GestionarContactoEmpresaAsync(
                    idCliente,
                    dto.RegistrarContactoPrincipal,
                    dto.ContactoPrincipal
                );
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }

        public async Task<bool> EliminarLogicoAsync(int idCliente)
        {
            var cliente = await _context.Cliente
                .FirstOrDefaultAsync(c => c.IdCliente == idCliente);

            if (cliente == null)
            {
                return false;
            }

            cliente.Estado = false;

            var contactos = await _context.ContactoCliente
                .Where(c => c.IdCliente == idCliente)
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
            return await _context.Cliente
                .CountAsync(c => c.Estado);
        }

        public async Task<List<TipoClienteDto>> ListarTiposClienteAsync()
        {
            return await _context.TipoCliente
                .Where(t => t.Estado)
                .OrderBy(t => t.IdTipoCliente)
                .Select(t => new TipoClienteDto
                {
                    IdTipoCliente = t.IdTipoCliente,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    RequiereRuc = t.RequiereRuc,
                    RequiereFactura = t.RequiereFactura
                })
                .ToListAsync();
        }

        public async Task<List<TipoDocumentoDto>> ListarTiposDocumentoAsync()
        {
            return await _context.TipoDocumento
                .Where(t => t.Estado)
                .OrderBy(t => t.IdTipoDocumento)
                .Select(t => new TipoDocumentoDto
                {
                    IdTipoDocumento = t.IdTipoDocumento,
                    Nombre = t.Nombre,
                    Longitud = t.Longitud
                })
                .ToListAsync();
        }

        public async Task<List<UbigeoDto>> ListarUbigeosAsync(string? buscar)
        {
            var query = _context.Ubigeo.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(u =>
                    (u.CodigoUbigeo != null && u.CodigoUbigeo.Contains(texto)) ||
                    u.Departamento.Contains(texto) ||
                    u.Provincia.Contains(texto) ||
                    u.Distrito.Contains(texto)
                );
            }
            else
            {
                query = query.Where(u => false);
            }

            return await query
                .OrderBy(u => u.Departamento)
                .ThenBy(u => u.Provincia)
                .ThenBy(u => u.Distrito)
                .Take(25)
                .Select(u => new UbigeoDto
                {
                    IdUbigeo = u.IdUbigeo,
                    CodigoUbigeo = u.CodigoUbigeo,
                    Departamento = u.Departamento,
                    Provincia = u.Provincia,
                    Distrito = u.Distrito,
                    Ubicacion =
                        (u.CodigoUbigeo ?? "------") + " - " +
                        u.Departamento + " / " +
                        u.Provincia + " / " +
                        u.Distrito
                })
                .ToListAsync();
        }

        private async Task<ClienteListadoDto> MapearClienteAsync(ClienteEntity cliente)
        {
            var tipoCliente = await _context.TipoCliente
                .Where(t => t.IdTipoCliente == cliente.IdTipoCliente)
                .Select(t => t.Nombre)
                .FirstOrDefaultAsync() ?? "Sin tipo";

            var tipoDocumento = await _context.TipoDocumento
                .Where(t => t.IdTipoDocumento == cliente.IdTipoDocumento)
                .Select(t => t.Nombre)
                .FirstOrDefaultAsync() ?? "Documento";

            var ubicacion = await _context.Ubigeo
                .Where(u => u.IdUbigeo == cliente.IdUbigeo)
                .Select(u =>
                    (u.CodigoUbigeo ?? "------") + " - " +
                    u.Departamento + " / " +
                    u.Provincia + " / " +
                    u.Distrito
                )
                .FirstOrDefaultAsync();

            var dto = new ClienteListadoDto
            {
                IdCliente = cliente.IdCliente,
                IdTipoCliente = cliente.IdTipoCliente,
                TipoCliente = tipoCliente,
                IdTipoDocumento = cliente.IdTipoDocumento,
                TipoDocumento = tipoDocumento,
                IdUbigeo = cliente.IdUbigeo,
                Ubicacion = ubicacion,
                IdUsuario = cliente.IdUsuario,
                CuentaWebVinculada = cliente.IdUsuario != null,
                NumeroDocumento = cliente.NumeroDocumento,
                Correo = cliente.Correo,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion,
                Estado = cliente.Estado,
                FechaRegistro = cliente.FechaRegistro
            };

            if (EsPersonaNatural(tipoCliente))
            {
                var persona = await _context.ClientePersonaNatural
                    .FirstOrDefaultAsync(p => p.IdCliente == cliente.IdCliente);

                dto.Nombres = persona?.Nombres;
                dto.ApellidoPaterno = persona?.ApellidoPaterno;
                dto.ApellidoMaterno = persona?.ApellidoMaterno;
                dto.Cliente = $"{persona?.Nombres} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim();
                dto.TieneContactoPrincipal = false;
                dto.ContactoPrincipal = null;
            }
            else
            {
                var empresa = await _context.ClienteEmpresa
                    .FirstOrDefaultAsync(e => e.IdCliente == cliente.IdCliente);

                dto.RazonSocial = empresa?.RazonSocial;
                dto.NombreComercial = empresa?.NombreComercial;
                dto.Cliente = empresa?.RazonSocial ?? "Empresa sin razón social";

                var contacto = await _context.ContactoCliente
                    .Where(c => c.IdCliente == cliente.IdCliente && c.Estado)
                    .OrderBy(c => c.IdContactoCliente)
                    .FirstOrDefaultAsync();

                dto.TieneContactoPrincipal = contacto != null;

                if (contacto != null)
                {
                    dto.ContactoPrincipal = new ClienteContactoDto
                    {
                        IdContactoCliente = contacto.IdContactoCliente,
                        Nombres = contacto.Nombres,
                        ApellidoPaterno = contacto.ApellidoPaterno,
                        ApellidoMaterno = contacto.ApellidoMaterno,
                        Cargo = contacto.Cargo,
                        Correo = contacto.Correo,
                        Telefono = contacto.Telefono,
                        Estado = contacto.Estado
                    };
                }
            }

            return dto;
        }

        private async Task ValidarCrearActualizarAsync(ClienteCrearDto dto, int? idClienteActual)
        {
            await ValidarDatosBaseAsync(
                dto.IdTipoCliente,
                dto.IdTipoDocumento,
                dto.IdUbigeo,
                dto.NumeroDocumento,
                dto.Correo,
                idClienteActual
            );

            var tipoCliente = await ObtenerNombreTipoClienteAsync(dto.IdTipoCliente);
            var tipoDocumento = await ObtenerNombreTipoDocumentoAsync(dto.IdTipoDocumento);

            if (EsPersonaNatural(tipoCliente))
            {
                ValidarPersonaNatural(dto.Nombres, dto.ApellidoPaterno, tipoDocumento, dto.NumeroDocumento);
            }
            else
            {
                ValidarEmpresa(dto.RazonSocial, tipoDocumento, dto.NumeroDocumento);

                if (dto.RegistrarContactoPrincipal)
                {
                    ValidarContacto(dto.ContactoPrincipal);
                }
            }
        }

        private async Task ValidarCrearActualizarAsync(ClienteActualizarDto dto, int? idClienteActual)
        {
            await ValidarDatosBaseAsync(
                dto.IdTipoCliente,
                dto.IdTipoDocumento,
                dto.IdUbigeo,
                dto.NumeroDocumento,
                dto.Correo,
                idClienteActual
            );

            var tipoCliente = await ObtenerNombreTipoClienteAsync(dto.IdTipoCliente);
            var tipoDocumento = await ObtenerNombreTipoDocumentoAsync(dto.IdTipoDocumento);

            if (EsPersonaNatural(tipoCliente))
            {
                ValidarPersonaNatural(dto.Nombres, dto.ApellidoPaterno, tipoDocumento, dto.NumeroDocumento);
            }
            else
            {
                ValidarEmpresa(dto.RazonSocial, tipoDocumento, dto.NumeroDocumento);

                if (dto.RegistrarContactoPrincipal)
                {
                    ValidarContacto(dto.ContactoPrincipal);
                }
            }
        }

        private async Task ValidarDatosBaseAsync(
            int idTipoCliente,
            int idTipoDocumento,
            int? idUbigeo,
            string numeroDocumento,
            string? correo,
            int? idClienteActual
        )
        {
            var existeTipoCliente = await _context.TipoCliente
                .AnyAsync(t => t.IdTipoCliente == idTipoCliente && t.Estado);

            if (!existeTipoCliente)
            {
                throw new InvalidOperationException("Selecciona un tipo de cliente válido.");
            }

            var existeTipoDocumento = await _context.TipoDocumento
                .AnyAsync(t => t.IdTipoDocumento == idTipoDocumento && t.Estado);

            if (!existeTipoDocumento)
            {
                throw new InvalidOperationException("Selecciona un tipo de documento válido.");
            }

            if (idUbigeo != null)
            {
                var existeUbigeo = await _context.Ubigeo
                    .AnyAsync(u => u.IdUbigeo == idUbigeo);

                if (!existeUbigeo)
                {
                    throw new InvalidOperationException("Selecciona una ubicación válida.");
                }
            }

            var documento = NormalizarDocumento(numeroDocumento);

            if (string.IsNullOrWhiteSpace(documento))
            {
                throw new InvalidOperationException("Ingresa el número de documento.");
            }

            var documentoDuplicado = await _context.Cliente
                .AnyAsync(c =>
                    c.IdTipoDocumento == idTipoDocumento &&
                    c.NumeroDocumento == documento &&
                    (!idClienteActual.HasValue || c.IdCliente != idClienteActual.Value)
                );

            if (documentoDuplicado)
            {
                throw new InvalidOperationException("Ya existe un cliente registrado con ese documento.");
            }

            if (!string.IsNullOrWhiteSpace(correo) && !correo.Contains("@"))
            {
                throw new InvalidOperationException("Ingresa un correo válido.");
            }
        }

        private void ValidarPersonaNatural(
            string? nombres,
            string? apellidoPaterno,
            string tipoDocumento,
            string numeroDocumento
        )
        {
            if (!tipoDocumento.Equals("DNI", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La persona natural debe registrarse con DNI.");
            }

            var documento = NormalizarDocumento(numeroDocumento);

            if (documento.Length != 8)
            {
                throw new InvalidOperationException("El DNI debe tener 8 dígitos.");
            }

            if (string.IsNullOrWhiteSpace(nombres))
            {
                throw new InvalidOperationException("Ingresa los nombres del cliente.");
            }

            if (string.IsNullOrWhiteSpace(apellidoPaterno))
            {
                throw new InvalidOperationException("Ingresa el apellido paterno del cliente.");
            }
        }

        private void ValidarEmpresa(
            string? razonSocial,
            string tipoDocumento,
            string numeroDocumento
        )
        {
            if (!tipoDocumento.Equals("RUC", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La empresa debe registrarse con RUC.");
            }

            var documento = NormalizarDocumento(numeroDocumento);

            if (documento.Length != 11)
            {
                throw new InvalidOperationException("El RUC debe tener 11 dígitos.");
            }

            if (string.IsNullOrWhiteSpace(razonSocial))
            {
                throw new InvalidOperationException("Ingresa la razón social de la empresa.");
            }
        }

        private void ValidarContacto(ClienteContactoDto? contacto)
        {
            if (contacto == null)
            {
                throw new InvalidOperationException("Ingresa los datos del contacto principal.");
            }

            if (string.IsNullOrWhiteSpace(contacto.Nombres))
            {
                throw new InvalidOperationException("Ingresa los nombres del contacto.");
            }

            if (string.IsNullOrWhiteSpace(contacto.ApellidoPaterno))
            {
                throw new InvalidOperationException("Ingresa el apellido paterno del contacto.");
            }

            if (!string.IsNullOrWhiteSpace(contacto.Correo) && !contacto.Correo.Contains("@"))
            {
                throw new InvalidOperationException("Ingresa un correo válido para el contacto.");
            }
        }

        private ContactoClienteEntity CrearContacto(int idCliente, ClienteContactoDto contacto)
        {
            return new ContactoClienteEntity
            {
                IdCliente = idCliente,
                Nombres = NormalizarTextoObligatorio(contacto.Nombres),
                ApellidoPaterno = NormalizarTextoObligatorio(contacto.ApellidoPaterno),
                ApellidoMaterno = NormalizarTexto(contacto.ApellidoMaterno),
                Cargo = NormalizarTexto(contacto.Cargo),
                Correo = NormalizarTexto(contacto.Correo),
                Telefono = NormalizarTexto(contacto.Telefono),
                Estado = true
            };
        }

        private async Task GestionarContactoEmpresaAsync(
            int idCliente,
            bool registrarContactoPrincipal,
            ClienteContactoDto? contactoDto
        )
        {
            var contactos = await _context.ContactoCliente
                .Where(c => c.IdCliente == idCliente)
                .OrderBy(c => c.IdContactoCliente)
                .ToListAsync();

            if (!registrarContactoPrincipal)
            {
                foreach (var contacto in contactos)
                {
                    contacto.Estado = false;
                }

                return;
            }

            if (contactoDto == null)
            {
                throw new InvalidOperationException("Ingresa los datos del contacto principal.");
            }

            var contactoActual = contactos.FirstOrDefault();

            if (contactoActual == null)
            {
                _context.ContactoCliente.Add(CrearContacto(idCliente, contactoDto));
                return;
            }

            contactoActual.Nombres = NormalizarTextoObligatorio(contactoDto.Nombres);
            contactoActual.ApellidoPaterno = NormalizarTextoObligatorio(contactoDto.ApellidoPaterno);
            contactoActual.ApellidoMaterno = NormalizarTexto(contactoDto.ApellidoMaterno);
            contactoActual.Cargo = NormalizarTexto(contactoDto.Cargo);
            contactoActual.Correo = NormalizarTexto(contactoDto.Correo);
            contactoActual.Telefono = NormalizarTexto(contactoDto.Telefono);
            contactoActual.Estado = true;
        }

        private async Task<string> ObtenerNombreTipoClienteAsync(int idTipoCliente)
        {
            return await _context.TipoCliente
                .Where(t => t.IdTipoCliente == idTipoCliente)
                .Select(t => t.Nombre)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        private async Task<string> ObtenerNombreTipoDocumentoAsync(int idTipoDocumento)
        {
            return await _context.TipoDocumento
                .Where(t => t.IdTipoDocumento == idTipoDocumento)
                .Select(t => t.Nombre)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        private bool EsPersonaNatural(string tipoCliente)
        {
            return tipoCliente.Equals("Persona Natural", StringComparison.OrdinalIgnoreCase);
        }

        private string NormalizarDocumento(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            return new string(valor.Where(char.IsDigit).ToArray());
        }

        private string? NormalizarTexto(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            return valor.Trim();
        }

        private string NormalizarTextoObligatorio(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            return valor.Trim();
        }
    }
}