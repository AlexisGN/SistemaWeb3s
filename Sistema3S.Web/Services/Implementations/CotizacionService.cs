using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Cotizacion;
using Sistema3S.Web.Services.Interfaces;
using Sistema3S.Web.Services.Pdf;

namespace Sistema3S.Web.Services.Implementations
{
    public class CotizacionService : ICotizacionService
    {
        private readonly Bd3sContext _context;
        private readonly IPdfCotizacionService _pdfCotizacionService;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public CotizacionService(
    Bd3sContext context,
    IPdfCotizacionService pdfCotizacionService,
    IEmailService emailService,
    IWhatsAppService whatsAppService,
    IWebHostEnvironment environment,
    IConfiguration configuration
)
        {
            _context = context;
            _pdfCotizacionService = pdfCotizacionService;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<ResultadoPaginadoDto<CotizacionListadoDto>> ListarAsync(
            string? buscar,
            string? estado,
            string? origen,
            int pagina,
            int tamanioPagina
        )
        {
            if (pagina <= 0) pagina = 1;
            if (tamanioPagina <= 0) tamanioPagina = 8;
            if (tamanioPagina > 50) tamanioPagina = 50;

            var items = new List<CotizacionListadoDto>();
            var totalRegistros = 0;

            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_ListarCotizaciones", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Buscar", ValorDb(buscar));
            command.Parameters.AddWithValue("@Estado", ValorDb(estado));
            command.Parameters.AddWithValue("@Origen", ValorDb(origen));
            command.Parameters.AddWithValue("@Pagina", pagina);
            command.Parameters.AddWithValue("@TamanioPagina", tamanioPagina);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = MapearListado(reader);
                totalRegistros = LeerInt(reader, "TotalRegistros");
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
            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_ObtenerCotizacionDetalle", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdCotizacion", idCotizacion);

            await using var reader = await command.ExecuteReaderAsync();

            CotizacionListadoDto? cotizacion = null;

            if (await reader.ReadAsync())
            {
                cotizacion = MapearListado(reader);
            }

            if (cotizacion == null)
            {
                return null;
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    cotizacion.Detalles.Add(MapearDetalle(reader));
                }
            }

            return cotizacion;
        }

        public async Task<CotizacionListadoDto> CrearAsync(CotizacionCrearDto dto)
        {
            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_RegistrarCotizacion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdCliente", dto.IdCliente);
            command.Parameters.AddWithValue("@IdUsuarioRegistro", ValorDb(dto.IdUsuarioRegistro));
            command.Parameters.AddWithValue("@OrigenCotizacion", ValorDb(dto.OrigenCotizacion));
            command.Parameters.AddWithValue("@Descuento", dto.Descuento);
            command.Parameters.AddWithValue("@Observacion", ValorDb(dto.Observacion));

            var detallesParam = command.Parameters.AddWithValue(
                "@Detalles",
                CrearTablaDetalles(dto.Detalles)
            );

            detallesParam.SqlDbType = SqlDbType.Structured;
            detallesParam.TypeName = "dbo.CotizacionDetalleType";

            var idCotizacion = 0;

            await using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    idCotizacion = LeerInt(reader, "IdCotizacion");
                }
            }

            if (idCotizacion <= 0)
            {
                throw new InvalidOperationException("No se pudo registrar la cotización.");
            }

            await GenerarPdfAsync(idCotizacion);

            var cotizacion = await ObtenerPorIdAsync(idCotizacion);

            if (cotizacion == null)
            {
                throw new InvalidOperationException("No se pudo recuperar la cotización registrada.");
            }

            return cotizacion;
        }

        public async Task<bool> CancelarAsync(int idCotizacion, int? idUsuarioAtencion)
        {
            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_CancelarCotizacion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdCotizacion", idCotizacion);
            command.Parameters.AddWithValue("@IdUsuarioAtencion", ValorDb(idUsuarioAtencion));

            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<bool> CambiarEstadoAsync(
            int idCotizacion,
            CotizacionCambiarEstadoDto dto
        )
        {
            var nuevoEstado = dto.NuevoEstado;

            if (string.IsNullOrWhiteSpace(nuevoEstado) && dto.IdEstadoCotizacion.HasValue)
            {
                nuevoEstado = await ObtenerNombreEstadoAsync(dto.IdEstadoCotizacion.Value);
            }

            if (string.IsNullOrWhiteSpace(nuevoEstado))
            {
                throw new InvalidOperationException("Selecciona el nuevo estado de la cotización.");
            }

            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_CambiarEstadoCotizacion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdCotizacion", idCotizacion);
            command.Parameters.AddWithValue("@NuevoEstado", nuevoEstado);
            command.Parameters.AddWithValue("@IdUsuarioAtencion", ValorDb(dto.IdUsuarioAtencion));

            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<string> GenerarPdfAsync(int idCotizacion)
        {
            var ruta = await _pdfCotizacionService.GenerarCotizacionAsync(idCotizacion);

            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_ActualizarPdfCotizacion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdCotizacion", idCotizacion);
            command.Parameters.AddWithValue("@ArchivoPdf", ruta);

            await command.ExecuteNonQueryAsync();

            return ruta;
        }

        public async Task<bool> EnviarCorreoAsync(
    int idCotizacion,
    int? idUsuarioAtencion
)
        {
            var cotizacion = await ObtenerPorIdAsync(idCotizacion);

            if (cotizacion == null)
            {
                return false;
            }

            var correoDestino = cotizacion.CorreoCliente?.Trim();

            if (string.IsNullOrWhiteSpace(correoDestino))
            {
                throw new InvalidOperationException(
                    "El cliente no tiene correo registrado. Agrega un correo antes de enviar la cotización."
                );
            }

            var archivoPdf = cotizacion.ArchivoPdf;

            if (string.IsNullOrWhiteSpace(archivoPdf))
            {
                archivoPdf = await GenerarPdfAsync(idCotizacion);

                var cotizacionActualizada = await ObtenerPorIdAsync(idCotizacion);

                if (cotizacionActualizada == null)
                {
                    return false;
                }

                cotizacion = cotizacionActualizada;
            }

            var rutaFisica = ObtenerRutaFisicaArchivo(archivoPdf);

            var asunto = $"{cotizacion.CodigoCotizacion} - Empresa 3S";

            var cuerpo =
        $@"Estimado(a) cliente,

Adjuntamos la cotización solicitada correspondiente a productos y/o servicios de Empresa 3S.

Código de cotización: {cotizacion.CodigoCotizacion}
Total referencial: S/ {cotizacion.Total:0.00}

Puede revisar el documento adjunto para validar el detalle de la propuesta.

Quedamos atentos a su confirmación.

Atentamente,
Empresa 3S";

            try
            {
                await _emailService.EnviarConAdjuntoAsync(
                    correoDestino,
                    asunto,
                    cuerpo,
                    rutaFisica
                );

                await RegistrarEnvioCorreoAsync(
                    cotizacion,
                    idUsuarioAtencion,
                    asunto,
                    cuerpo,
                    true,
                    null
                );

                await MarcarRespondidaSqlAsync(
                    idCotizacion,
                    idUsuarioAtencion,
                    "Correo",
                    archivoPdf
                );

                return true;
            }
            catch (Exception ex)
            {
                await RegistrarEnvioCorreoAsync(
                    cotizacion,
                    idUsuarioAtencion,
                    asunto,
                    cuerpo,
                    false,
                    ex.Message
                );

                throw new InvalidOperationException(
                    $"No se pudo enviar el correo: {ex.Message}"
                );
            }
        }

        public async Task<CotizacionWhatsAppDto> ObtenerWhatsAppAsync(int idCotizacion)
        {
            var cotizacion = await ObtenerPorIdAsync(idCotizacion);

            if (cotizacion == null)
            {
                throw new InvalidOperationException("Cotización no encontrada.");
            }

            var telefonoCliente = cotizacion.TelefonoCliente?.Trim();

            if (string.IsNullOrWhiteSpace(telefonoCliente))
            {
                throw new InvalidOperationException(
                    "El cliente no tiene teléfono registrado. Agrega un número antes de enviar por WhatsApp."
                );
            }

            var archivoPdf = cotizacion.ArchivoPdf;

            if (string.IsNullOrWhiteSpace(archivoPdf))
            {
                archivoPdf = await GenerarPdfAsync(idCotizacion);
            }

            var telefono = LimpiarTelefonoWhatsApp(telefonoCliente);

            var mensaje = CrearMensajeWhatsAppCliente(cotizacion);

            var url = $"https://wa.me/{telefono}?text={Uri.EscapeDataString(mensaje)}";

            return new CotizacionWhatsAppDto
            {
                IdCotizacion = cotizacion.IdCotizacion,
                CodigoCotizacion = cotizacion.CodigoCotizacion,
                Telefono = telefono,
                Mensaje = mensaje,
                Url = url,
                ArchivoPdf = archivoPdf,
                RequiereConfirmacionRespondida = true
            };
        }
        public async Task<CotizacionWhatsAppDto> EnviarWhatsAppAsync(
    int idCotizacion,
    int? idUsuarioAtencion
)
        {
            var cotizacion = await ObtenerPorIdAsync(idCotizacion);

            if (cotizacion == null)
            {
                throw new InvalidOperationException("Cotización no encontrada.");
            }

            var telefonoCliente = cotizacion.TelefonoCliente?.Trim();

            if (string.IsNullOrWhiteSpace(telefonoCliente))
            {
                throw new InvalidOperationException(
                    "El cliente no tiene teléfono registrado. Agrega un número antes de enviar por WhatsApp."
                );
            }

            var archivoPdf = cotizacion.ArchivoPdf;

            if (string.IsNullOrWhiteSpace(archivoPdf))
            {
                archivoPdf = await GenerarPdfAsync(idCotizacion);

                var cotizacionActualizada = await ObtenerPorIdAsync(idCotizacion);

                if (cotizacionActualizada == null)
                {
                    throw new InvalidOperationException("No se pudo recuperar la cotización.");
                }

                cotizacion = cotizacionActualizada;
            }

            var telefono = LimpiarTelefonoWhatsApp(telefonoCliente);
            var mensaje = CrearMensajeWhatsAppCliente(cotizacion);

            var modo = (_configuration["WhatsApp:Mode"] ?? "WebLocal").Trim();

            if (modo.Equals("CloudApi", StringComparison.OrdinalIgnoreCase))
            {
                var rutaFisica = ObtenerRutaFisicaArchivo(archivoPdf);
                var nombreArchivo = Path.GetFileName(rutaFisica);

                await _whatsAppService.EnviarDocumentoPdfAsync(
                    telefono,
                    rutaFisica,
                    nombreArchivo,
                    mensaje
                );

                await MarcarRespondidaSqlAsync(
                    idCotizacion,
                    idUsuarioAtencion,
                    "WhatsApp",
                    archivoPdf
                );

                return new CotizacionWhatsAppDto
                {
                    IdCotizacion = cotizacion.IdCotizacion,
                    CodigoCotizacion = cotizacion.CodigoCotizacion,
                    Telefono = telefono,
                    Mensaje = mensaje,
                    Url = string.Empty,
                    ArchivoPdf = archivoPdf,
                    RequiereConfirmacionRespondida = false
                };
            }

            var url = $"https://wa.me/{telefono}?text={Uri.EscapeDataString(mensaje)}";

            return new CotizacionWhatsAppDto
            {
                IdCotizacion = cotizacion.IdCotizacion,
                CodigoCotizacion = cotizacion.CodigoCotizacion,
                Telefono = telefono,
                Mensaje = mensaje,
                Url = url,
                ArchivoPdf = archivoPdf,
                RequiereConfirmacionRespondida = true
            };
        }

        public async Task<bool> MarcarRespondidaAsync(
            int idCotizacion,
            CotizacionMarcarRespondidaDto dto
        )
        {
            var cotizacion = await ObtenerPorIdAsync(idCotizacion);

            if (cotizacion == null)
            {
                return false;
            }

            var archivoPdf = cotizacion.ArchivoPdf;

            if (string.IsNullOrWhiteSpace(archivoPdf))
            {
                archivoPdf = await GenerarPdfAsync(idCotizacion);
            }

            await MarcarRespondidaSqlAsync(
                idCotizacion,
                dto.IdUsuarioAtencion,
                dto.CanalEnvio,
                archivoPdf
            );

            return true;
        }

        public async Task<CotizacionListadoDto> PrepararParaVentaAsync(int idCotizacion)
        {
            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_PrepararCotizacionParaVenta", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdCotizacion", idCotizacion);

            await using var reader = await command.ExecuteReaderAsync();

            CotizacionListadoDto? cotizacion = null;

            if (await reader.ReadAsync())
            {
                cotizacion = MapearListado(reader);
            }

            if (cotizacion == null)
            {
                throw new InvalidOperationException("No se pudo preparar la cotización para venta.");
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    cotizacion.Detalles.Add(MapearDetalle(reader));
                }
            }

            return cotizacion;
        }

        public async Task<bool> MarcarConvertidaVentaAsync(
            int idCotizacion,
            CotizacionConvertirVentaDto dto
        )
        {
            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_MarcarCotizacionConvertidaVenta", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdCotizacion", idCotizacion);
            command.Parameters.AddWithValue("@IdVenta", dto.IdVenta);
            command.Parameters.AddWithValue("@IdUsuarioAtencion", ValorDb(dto.IdUsuarioAtencion));

            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<int> ContarPendientesAsync()
        {
            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_ListarCotizaciones", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Buscar", DBNull.Value);
            command.Parameters.AddWithValue("@Estado", "Pendiente");
            command.Parameters.AddWithValue("@Origen", DBNull.Value);
            command.Parameters.AddWithValue("@Pagina", 1);
            command.Parameters.AddWithValue("@TamanioPagina", 1);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return LeerInt(reader, "TotalRegistros");
            }

            return 0;
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
            return await (
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

        private async Task MarcarRespondidaSqlAsync(
            int idCotizacion,
            int? idUsuarioAtencion,
            string canalEnvio,
            string? archivoPdf
        )
        {
            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.sp_MarcarCotizacionRespondida", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdCotizacion", idCotizacion);
            command.Parameters.AddWithValue("@IdUsuarioAtencion", ValorDb(idUsuarioAtencion));
            command.Parameters.AddWithValue("@CanalEnvio", canalEnvio);
            command.Parameters.AddWithValue("@ArchivoPdf", ValorDb(archivoPdf));

            await command.ExecuteNonQueryAsync();
        }

        private async Task RegistrarEnvioCorreoAsync(
            CotizacionListadoDto cotizacion,
            int? idUsuarioRegistro,
            string asunto,
            string cuerpo,
            bool exitoso,
            string? mensajeError
        )
        {
            await using var connection = CrearConexion();
            await connection.OpenAsync();

            await using var command = new SqlCommand(@"
                INSERT INTO EnvioCorreo (
                    IdCotizacion,
                    IdComprobante,
                    IdUsuarioRegistro,
                    Destinatario,
                    Asunto,
                    Cuerpo,
                    FechaEnvio,
                    Exitoso,
                    MensajeError
                )
                VALUES (
                    @IdCotizacion,
                    NULL,
                    @IdUsuarioRegistro,
                    @Destinatario,
                    @Asunto,
                    @Cuerpo,
                    GETDATE(),
                    @Exitoso,
                    @MensajeError
                );
            ", connection);

            command.Parameters.AddWithValue("@IdCotizacion", cotizacion.IdCotizacion);
            command.Parameters.AddWithValue("@IdUsuarioRegistro", ValorDb(idUsuarioRegistro));
            command.Parameters.AddWithValue("@Destinatario", cotizacion.CorreoCliente ?? "");
            command.Parameters.AddWithValue("@Asunto", asunto);
            command.Parameters.AddWithValue("@Cuerpo", cuerpo);
            command.Parameters.AddWithValue("@Exitoso", exitoso);
            command.Parameters.AddWithValue("@MensajeError", ValorDb(mensajeError));

            await command.ExecuteNonQueryAsync();
        }

        private async Task<string?> ObtenerNombreEstadoAsync(int idEstadoCotizacion)
        {
            return await _context.EstadoCotizacion
                .Where(e => e.IdEstadoCotizacion == idEstadoCotizacion && e.Estado)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync();
        }

        private SqlConnection CrearConexion()
        {
            var connectionString = _context.Database.GetConnectionString();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("No se encontró la cadena de conexión.");
            }

            return new SqlConnection(connectionString);
        }

        private static DataTable CrearTablaDetalles(List<CotizacionDetalleCrearDto> detalles)
        {
            var table = new DataTable();

            table.Columns.Add("IdElementoCatalogo", typeof(int));
            table.Columns.Add("Cantidad", typeof(int));
            table.Columns.Add("PrecioUnitario", typeof(decimal));
            table.Columns.Add("Observacion", typeof(string));

            foreach (var item in detalles)
            {
                table.Rows.Add(
                    item.IdElementoCatalogo,
                    item.Cantidad,
                    item.PrecioUnitario,
                    string.IsNullOrWhiteSpace(item.Observacion) ? DBNull.Value : item.Observacion.Trim()
                );
            }

            return table;
        }

        private static CotizacionListadoDto MapearListado(SqlDataReader reader)
        {
            return new CotizacionListadoDto
            {
                IdCotizacion = LeerInt(reader, "IdCotizacion"),
                CodigoCotizacion = LeerString(reader, "CodigoCotizacion"),

                IdCliente = LeerInt(reader, "IdCliente"),
                Cliente = LeerString(reader, "Cliente"),
                TipoDocumentoCliente = LeerString(reader, "TipoDocumentoCliente"),
                DocumentoCliente = LeerString(reader, "DocumentoCliente"),
                TipoCliente = LeerString(reader, "TipoCliente"),

                CorreoCliente = LeerStringNullable(reader, "CorreoCliente"),
                TelefonoCliente = LeerStringNullable(reader, "TelefonoCliente"),
                DireccionCliente = LeerStringNullable(reader, "DireccionCliente"),

                IdUsuarioRegistro = LeerIntNullable(reader, "IdUsuarioRegistro"),
                IdUsuarioAtencion = LeerIntNullable(reader, "IdUsuarioAtencion"),

                IdEstadoCotizacion = LeerInt(reader, "IdEstadoCotizacion"),
                EstadoCotizacion = LeerString(reader, "EstadoCotizacion"),

                OrigenCotizacion = LeerString(reader, "OrigenCotizacion"),

                FechaCotizacion = LeerDateTime(reader, "FechaCotizacion"),
                FechaRespuesta = LeerDateTimeNullable(reader, "FechaRespuesta"),
                CanalRespuesta = LeerStringNullable(reader, "CanalRespuesta"),

                Subtotal = LeerDecimal(reader, "Subtotal"),
                Descuento = LeerDecimal(reader, "Descuento"),
                Igv = LeerDecimal(reader, "Igv"),
                Total = LeerDecimal(reader, "Total"),
                TotalReferencial = LeerDecimal(reader, "TotalReferencial"),

                Observacion = LeerStringNullable(reader, "Observacion"),
                ArchivoPdf = LeerStringNullable(reader, "ArchivoPdf"),

                CorreoEnviado = LeerBool(reader, "CorreoEnviado"),
                WhatsappEnviado = LeerBool(reader, "WhatsappEnviado"),
                PdfGenerado = LeerBool(reader, "PdfGenerado"),

                CantidadDetalles = LeerInt(reader, "CantidadDetalles"),

                IdVentaGenerada = LeerIntNullable(reader, "IdVentaGenerada"),
                IdVentaAsociada = LeerIntNullable(reader, "IdVentaAsociada"),

                PuedeConvertirVenta = LeerBool(reader, "PuedeConvertirVenta"),
                PuedeGestionar = LeerBool(reader, "PuedeGestionar")
            };
        }

        private static CotizacionDetalleDto MapearDetalle(SqlDataReader reader)
        {
            var elemento = LeerString(reader, "Elemento");

            return new CotizacionDetalleDto
            {
                IdDetalleCotizacion = LeerInt(reader, "IdDetalleCotizacion"),
                IdCotizacion = LeerInt(reader, "IdCotizacion"),
                IdElementoCatalogo = LeerInt(reader, "IdElementoCatalogo"),

                Elemento = elemento,
                ElementoNombre = elemento,
                TipoElemento = LeerString(reader, "TipoElemento"),

                IdProducto = LeerIntNullable(reader, "IdProducto"),
                CodigoProducto = LeerStringNullable(reader, "CodigoProducto"),

                IdServicio = LeerIntNullable(reader, "IdServicio"),

                Codigo = LeerString(reader, "Codigo"),

                Cantidad = LeerInt(reader, "Cantidad"),
                PrecioUnitario = LeerDecimal(reader, "PrecioUnitario"),
                Subtotal = LeerDecimal(reader, "Subtotal"),

                Observacion = LeerStringNullable(reader, "Observacion")
            };
        }

        private string ObtenerRutaFisicaArchivo(string? rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
            {
                return string.Empty;
            }

            var webRoot = _environment.WebRootPath ??
                          Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var rutaLimpia = rutaRelativa
                .TrimStart('/')
                .Replace("/", Path.DirectorySeparatorChar.ToString());

            return Path.Combine(webRoot, rutaLimpia);
        }

        private string ObtenerBaseUrlPublica()
        {
            var baseUrl = _configuration["App:PublicBaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "https://3s-omega.vercel.app";
            }

            return baseUrl.TrimEnd('/');
        }

        private static object ValorDb(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
        }

        private static object ValorDb(int? valor)
        {
            return valor.HasValue ? valor.Value : DBNull.Value;
        }

        private static bool ExisteColumna(SqlDataReader reader, string nombre)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(nombre, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string LeerString(SqlDataReader reader, string nombre)
        {
            if (!ExisteColumna(reader, nombre))
            {
                return string.Empty;
            }

            var ordinal = reader.GetOrdinal(nombre);

            return reader.IsDBNull(ordinal) ? string.Empty : Convert.ToString(reader.GetValue(ordinal)) ?? string.Empty;
        }

        private static string? LeerStringNullable(SqlDataReader reader, string nombre)
        {
            if (!ExisteColumna(reader, nombre))
            {
                return null;
            }

            var ordinal = reader.GetOrdinal(nombre);

            return reader.IsDBNull(ordinal) ? null : Convert.ToString(reader.GetValue(ordinal));
        }

        private static int LeerInt(SqlDataReader reader, string nombre)
        {
            if (!ExisteColumna(reader, nombre))
            {
                return 0;
            }

            var ordinal = reader.GetOrdinal(nombre);

            return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
        }
        private static string CrearMensajeWhatsAppCliente(CotizacionListadoDto cotizacion)
        {
            return
        $@"Hola, le saludamos de Empresa 3S.

Le enviamos su cotización {cotizacion.CodigoCotizacion} por la venta de productos y/o servicios solicitados.

Total referencial: S/ {cotizacion.Total:0.00}

Adjuntamos el PDF de la cotización para su revisión.

Quedamos atentos a su confirmación.";
        }
        private static int? LeerIntNullable(SqlDataReader reader, string nombre)
        {
            if (!ExisteColumna(reader, nombre))
            {
                return null;
            }

            var ordinal = reader.GetOrdinal(nombre);

            return reader.IsDBNull(ordinal) ? null : Convert.ToInt32(reader.GetValue(ordinal));
        }

        private static decimal LeerDecimal(SqlDataReader reader, string nombre)
        {
            if (!ExisteColumna(reader, nombre))
            {
                return 0;
            }

            var ordinal = reader.GetOrdinal(nombre);

            return reader.IsDBNull(ordinal) ? 0 : Convert.ToDecimal(reader.GetValue(ordinal));
        }

        private static bool LeerBool(SqlDataReader reader, string nombre)
        {
            if (!ExisteColumna(reader, nombre))
            {
                return false;
            }

            var ordinal = reader.GetOrdinal(nombre);

            return !reader.IsDBNull(ordinal) && Convert.ToBoolean(reader.GetValue(ordinal));
        }

        private static DateTime LeerDateTime(SqlDataReader reader, string nombre)
        {
            if (!ExisteColumna(reader, nombre))
            {
                return DateTime.MinValue;
            }

            var ordinal = reader.GetOrdinal(nombre);

            return reader.IsDBNull(ordinal) ? DateTime.MinValue : Convert.ToDateTime(reader.GetValue(ordinal));
        }

        private static DateTime? LeerDateTimeNullable(SqlDataReader reader, string nombre)
        {
            if (!ExisteColumna(reader, nombre))
            {
                return null;
            }

            var ordinal = reader.GetOrdinal(nombre);

            return reader.IsDBNull(ordinal) ? null : Convert.ToDateTime(reader.GetValue(ordinal));
        }

        private static string LimpiarTelefonoWhatsApp(string telefono)
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