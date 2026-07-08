using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Venta;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class VentaService : IVentaService
    {
        private readonly Bd3sContext _context;

        public VentaService(Bd3sContext context)
        {
            _context = context;
        }

        public async Task<ResultadoPaginadoDto<VentaListadoDto>> ListarAsync(
            string? buscar,
            string? estadoPago,
            string? tipoComprobante,
            string? origenVenta,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina,
            int tamanioPagina
        )
        {
            pagina = pagina <= 0 ? 1 : pagina;
            tamanioPagina = tamanioPagina <= 0 ? 8 : tamanioPagina;

            var ventas = new List<VentaListadoDto>();
            var totalRegistros = 0;

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ListarVentas", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Buscar", (object?)buscar?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("@EstadoPago", (object?)estadoPago?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("@TipoComprobante", (object?)tipoComprobante?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrigenVenta", (object?)origenVenta?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("@FechaInicio", (object?)fechaInicio?.Date ?? DBNull.Value);
            command.Parameters.AddWithValue("@FechaFin", (object?)fechaFin?.Date ?? DBNull.Value);
            command.Parameters.AddWithValue("@Pagina", pagina);
            command.Parameters.AddWithValue("@TamanioPagina", tamanioPagina);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                totalRegistros = LeerInt(reader, "TotalRegistros");

                ventas.Add(new VentaListadoDto
                {
                    IdVenta = LeerInt(reader, "IdVenta"),
                    IdCliente = LeerInt(reader, "IdCliente"),
                    IdCotizacion = LeerNullableInt(reader, "IdCotizacion"),
                    FechaVenta = LeerDateTime(reader, "FechaVenta"),
                    TipoComprobante = LeerString(reader, "TipoComprobante"),
                    Serie = LeerString(reader, "Serie"),
                    Numero = LeerString(reader, "Numero"),
                    DocumentoCompleto = LeerString(reader, "DocumentoCompleto"),
                    DocumentoCliente = LeerString(reader, "DocumentoCliente"),
                    Cliente = LeerString(reader, "Cliente"),
                    OrigenVenta = LeerString(reader, "OrigenVenta"),
                    Subtotal = LeerDecimal(reader, "Subtotal"),
                    Igv = LeerDecimal(reader, "Igv"),
                    Total = LeerDecimal(reader, "Total"),
                    TotalPagado = LeerDecimal(reader, "TotalPagado"),
                    SaldoPendiente = LeerDecimal(reader, "SaldoPendiente"),
                    EstadoVenta = LeerString(reader, "EstadoVenta"),
                    EstadoPago = LeerString(reader, "EstadoPago")
                });
            }

            return new ResultadoPaginadoDto<VentaListadoDto>
            {
                Items = ventas,
                Pagina = pagina,
                TamanioPagina = tamanioPagina,
                TotalRegistros = totalRegistros
            };
        }

        public async Task<VentaRegistroResultadoDto> RegistrarVentaCompletaAsync(VentaCrearDto dto)
        {
            ValidarVenta(dto);

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_RegistrarVentaCompleta", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdCliente", dto.IdCliente);
            command.Parameters.AddWithValue("@IdCotizacion", (object?)dto.IdCotizacion ?? DBNull.Value);
            command.Parameters.AddWithValue("@IdUsuarioRegistro", dto.IdUsuarioRegistro);
            command.Parameters.AddWithValue("@TipoComprobante", dto.TipoComprobante.Trim());
            command.Parameters.AddWithValue("@Serie", dto.Serie.Trim().ToUpper());
            command.Parameters.AddWithValue("@Numero", dto.Numero.Trim());
            command.Parameters.AddWithValue("@FechaEmision", (object?)dto.FechaEmision ?? DBNull.Value);
            command.Parameters.AddWithValue("@Observacion", (object?)dto.Observacion?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("@TipoPago", dto.TipoPago.Trim());
            command.Parameters.AddWithValue("@MetodoPago", dto.MetodoPago.Trim());
            command.Parameters.AddWithValue("@MontoPagado", dto.MontoPagado);
            command.Parameters.AddWithValue("@NumeroCuotas", (object?)dto.NumeroCuotas ?? DBNull.Value);
            command.Parameters.AddWithValue("@FechaPrimerVencimiento", (object?)dto.FechaPrimerVencimiento?.Date ?? DBNull.Value);

            var detalleTable = CrearDetalleDataTable(dto.Detalles);

            var detalleParam = command.Parameters.AddWithValue("@Detalles", detalleTable);
            detalleParam.SqlDbType = SqlDbType.Structured;
            detalleParam.TypeName = "VentaDetalleType";

            var cuotasJsonParam = command.Parameters.Add("@CuotasJson", SqlDbType.NVarChar, -1);
            cuotasJsonParam.Value = CrearCuotasJson(dto);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("No se pudo obtener el resultado de la venta.");
                }

                return LeerResultadoVenta(reader);
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<SiguienteComprobanteDto> ObtenerSiguienteComprobanteAsync(string tipoComprobante)
        {
            if (string.IsNullOrWhiteSpace(tipoComprobante))
            {
                throw new InvalidOperationException("Selecciona el tipo de comprobante.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ObtenerSiguienteNumeroComprobante", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@TipoComprobante", tipoComprobante.Trim());

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                throw new InvalidOperationException("No se pudo obtener el siguiente número de comprobante.");
            }

            return new SiguienteComprobanteDto
            {
                Serie = LeerString(reader, "Serie"),
                Numero = LeerString(reader, "Numero"),
                DocumentoCompleto = LeerString(reader, "DocumentoCompleto")
            };
        }

        public async Task<VentaDetalleCompletoDto?> ObtenerDetalleAsync(int idVenta)
        {
            if (idVenta <= 0)
            {
                throw new InvalidOperationException("Selecciona una venta válida.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ObtenerVentaDetalle", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdVenta", idVenta);

            await using var reader = await command.ExecuteReaderAsync();

            VentaDetalleCompletoDto? venta = null;

            if (await reader.ReadAsync())
            {
                venta = new VentaDetalleCompletoDto
                {
                    IdVenta = LeerInt(reader, "IdVenta"),
                    IdCliente = LeerInt(reader, "IdCliente"),
                    IdCotizacion = LeerNullableInt(reader, "IdCotizacion"),
                    FechaVenta = LeerDateTime(reader, "FechaVenta"),
                    TipoComprobante = LeerString(reader, "TipoComprobante"),
                    Serie = LeerString(reader, "Serie"),
                    Numero = LeerString(reader, "Numero"),
                    DocumentoCompleto = LeerString(reader, "DocumentoCompleto"),
                    FechaEmision = LeerDateTime(reader, "FechaEmision"),
                    TipoDocumentoCliente = LeerString(reader, "TipoDocumentoCliente"),
                    DocumentoCliente = LeerString(reader, "DocumentoCliente"),
                    Cliente = LeerString(reader, "Cliente"),
                    CorreoCliente = LeerNullableString(reader, "CorreoCliente"),
                    TelefonoCliente = LeerNullableString(reader, "TelefonoCliente"),
                    DireccionCliente = LeerNullableString(reader, "DireccionCliente"),
                    Subtotal = LeerDecimal(reader, "Subtotal"),
                    Igv = LeerDecimal(reader, "Igv"),
                    Total = LeerDecimal(reader, "Total"),
                    TotalPagado = LeerDecimal(reader, "TotalPagado"),
                    SaldoPendiente = LeerDecimal(reader, "SaldoPendiente"),
                    EstadoVenta = LeerString(reader, "EstadoVenta"),
                    EstadoPago = LeerString(reader, "EstadoPago"),
                    OrigenVenta = LeerString(reader, "OrigenVenta"),
                    Observacion = LeerNullableString(reader, "Observacion")
                };
            }

            if (venta == null)
            {
                return null;
            }

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                venta.Detalles.Add(new VentaDetalleItemDto
                {
                    IdDetalleVenta = LeerInt(reader, "IdDetalleVenta"),
                    IdElementoCatalogo = LeerInt(reader, "IdElementoCatalogo"),
                    Elemento = LeerString(reader, "Elemento"),
                    TipoElemento = LeerString(reader, "TipoElemento"),
                    IdProducto = LeerNullableInt(reader, "IdProducto"),
                    CodigoProducto = LeerNullableString(reader, "CodigoProducto"),
                    IdServicio = LeerNullableInt(reader, "IdServicio"),
                    Cantidad = LeerInt(reader, "Cantidad"),
                    PrecioUnitario = LeerDecimal(reader, "PrecioUnitario"),
                    Subtotal = LeerDecimal(reader, "Subtotal")
                });
            }

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                venta.Pagos.Add(new PagoVentaDetalleDto
                {
                    IdPagoVenta = LeerInt(reader, "IdPagoVenta"),
                    IdVenta = LeerInt(reader, "IdVenta"),
                    MetodoPago = LeerString(reader, "MetodoPago"),
                    MontoPagado = LeerDecimal(reader, "MontoPagado"),
                    FechaPago = LeerDateTime(reader, "FechaPago"),
                    Observacion = LeerNullableString(reader, "Observacion")
                });
            }

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                venta.Cuotas.Add(new CuotaVentaDetalleDto
                {
                    IdCuotaVenta = LeerInt(reader, "IdCuotaVenta"),
                    IdVenta = LeerInt(reader, "IdVenta"),
                    NumeroCuota = LeerInt(reader, "NumeroCuota"),
                    FechaVencimiento = LeerDateTime(reader, "FechaVencimiento"),
                    MontoCuota = LeerDecimal(reader, "MontoCuota"),
                    MontoPagado = LeerDecimal(reader, "MontoPagado"),
                    EstadoCuota = LeerString(reader, "EstadoCuota")
                });
            }

            return venta;
        }

        public async Task<VentaRegistroResultadoDto> RegistrarPagoAsync(PagoVentaCrearDto dto)
        {
            if (dto.IdVenta <= 0)
            {
                throw new InvalidOperationException("Selecciona una venta válida.");
            }

            if (dto.IdUsuarioRegistro <= 0)
            {
                throw new InvalidOperationException("El usuario que registra el cobro no es válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.MetodoPago))
            {
                throw new InvalidOperationException("Selecciona el método de cobro.");
            }

            if (dto.MontoPagado <= 0)
            {
                throw new InvalidOperationException("El monto cobrado debe ser mayor a 0.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_RegistrarPagoVenta", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdVenta", dto.IdVenta);
            command.Parameters.AddWithValue("@IdUsuarioRegistro", dto.IdUsuarioRegistro);
            command.Parameters.AddWithValue("@MetodoPago", dto.MetodoPago.Trim());
            command.Parameters.AddWithValue("@MontoPagado", dto.MontoPagado);
            command.Parameters.AddWithValue("@Observacion", (object?)dto.Observacion?.Trim() ?? DBNull.Value);

            var cuotasCobradasJsonParam = command.Parameters.Add("@CuotasCobradasJson", SqlDbType.NVarChar, -1);
            cuotasCobradasJsonParam.Value = CrearCuotasCobradasJson(dto);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("No se pudo obtener el resultado del cobro.");
                }

                return LeerResultadoVenta(reader);
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<bool> AnularAsync(int idVenta, AnularVentaDto dto)
        {
            if (idVenta <= 0)
            {
                throw new InvalidOperationException("Selecciona una venta válida.");
            }

            if (dto.IdUsuarioRegistro <= 0)
            {
                throw new InvalidOperationException("El usuario que anula la venta no es válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.Motivo) || dto.Motivo.Trim().Length < 5)
            {
                throw new InvalidOperationException("Ingresa un motivo de anulación válido.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_AnularVenta", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdVenta", idVenta);
            command.Parameters.AddWithValue("@IdUsuarioRegistro", dto.IdUsuarioRegistro);
            command.Parameters.AddWithValue("@Motivo", dto.Motivo.Trim());

            try
            {
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<List<ReporteVentaDto>> ObtenerReporteAsync(
            string? buscar,
            string? estadoPago,
            string? tipoComprobante,
            string? origenVenta,
            DateTime? fechaInicio,
            DateTime? fechaFin
        )
        {
            var ventas = new List<ReporteVentaDto>();

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ReporteVentas", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Buscar", (object?)buscar?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("@EstadoPago", (object?)estadoPago?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("@TipoComprobante", (object?)tipoComprobante?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrigenVenta", (object?)origenVenta?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("@FechaInicio", (object?)fechaInicio?.Date ?? DBNull.Value);
            command.Parameters.AddWithValue("@FechaFin", (object?)fechaFin?.Date ?? DBNull.Value);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ventas.Add(new ReporteVentaDto
                {
                    IdVenta = LeerInt(reader, "IdVenta"),
                    FechaVenta = LeerDateTime(reader, "FechaVenta"),
                    TipoComprobante = LeerString(reader, "TipoComprobante"),
                    Serie = LeerString(reader, "Serie"),
                    Numero = LeerString(reader, "Numero"),
                    DocumentoCompleto = LeerString(reader, "DocumentoCompleto"),
                    DocumentoCliente = LeerString(reader, "DocumentoCliente"),
                    Cliente = LeerString(reader, "Cliente"),
                    OrigenVenta = LeerString(reader, "OrigenVenta"),
                    Subtotal = LeerDecimal(reader, "Subtotal"),
                    Igv = LeerDecimal(reader, "Igv"),
                    Total = LeerDecimal(reader, "Total"),
                    TotalPagado = LeerDecimal(reader, "TotalPagado"),
                    SaldoPendiente = LeerDecimal(reader, "SaldoPendiente"),
                    EstadoVenta = LeerString(reader, "EstadoVenta"),
                    EstadoPago = LeerString(reader, "EstadoPago")
                });
            }

            return ventas;
        }

        private static void ValidarVenta(VentaCrearDto dto)
        {
            if (dto.IdCliente <= 0)
            {
                throw new InvalidOperationException("Selecciona un cliente.");
            }

            if (dto.IdUsuarioRegistro <= 0)
            {
                throw new InvalidOperationException("El usuario que registra la venta no es válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.TipoComprobante))
            {
                throw new InvalidOperationException("Selecciona el tipo de comprobante.");
            }

            var comprobantesPermitidos = new[] { "Factura", "Boleta", "Nota de venta" };

            if (!comprobantesPermitidos.Contains(dto.TipoComprobante.Trim()))
            {
                throw new InvalidOperationException("Tipo de comprobante no válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.TipoPago))
            {
                throw new InvalidOperationException("Selecciona el tipo de cobro.");
            }

            var tiposPagoPermitidos = new[] { "Total", "Parcial", "Cuotas" };

            if (!tiposPagoPermitidos.Contains(dto.TipoPago.Trim()))
            {
                throw new InvalidOperationException("Selecciona un tipo de cobro válido.");
            }

            if (dto.Detalles == null || dto.Detalles.Count == 0)
            {
                throw new InvalidOperationException("Agrega al menos un producto o servicio.");
            }

            if (dto.Detalles.Any(d => d.IdElementoCatalogo <= 0))
            {
                throw new InvalidOperationException("Selecciona productos o servicios válidos.");
            }

            if (dto.Detalles.Any(d => d.Cantidad <= 0))
            {
                throw new InvalidOperationException("La cantidad debe ser mayor a 0.");
            }

            if (dto.Detalles.Any(d => d.PrecioUnitario <= 0))
            {
                throw new InvalidOperationException("El precio debe ser mayor a 0.");
            }

            var total = dto.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
            total = Math.Round(total, 2);

            dto.MontoPagado = Math.Round(dto.MontoPagado, 2);

            if (dto.MontoPagado < 0)
            {
                throw new InvalidOperationException("El monto cobrado no puede ser negativo.");
            }

            if (dto.MontoPagado > total)
            {
                throw new InvalidOperationException("El monto cobrado no puede ser mayor al total.");
            }

            if (dto.MontoPagado > 0 && string.IsNullOrWhiteSpace(dto.MetodoPago))
            {
                throw new InvalidOperationException("Selecciona el método de cobro.");
            }

            if (dto.TipoPago == "Total" && dto.MontoPagado != total)
            {
                throw new InvalidOperationException("Para cobro total, el monto cobrado debe ser igual al total.");
            }

            if (dto.TipoPago == "Parcial" && (dto.MontoPagado <= 0 || dto.MontoPagado >= total))
            {
                throw new InvalidOperationException("Para cobro parcial, el monto debe ser mayor a 0 y menor al total.");
            }

            if (dto.TipoPago != "Cuotas" && dto.Cuotas != null && dto.Cuotas.Count > 0)
            {
                throw new InvalidOperationException("Solo las ventas en cuotas pueden tener cronograma.");
            }

            if (dto.TipoPago == "Cuotas")
            {
                if (!dto.NumeroCuotas.HasValue || dto.NumeroCuotas.Value <= 0)
                {
                    throw new InvalidOperationException("Ingresa un número de cuotas válido.");
                }

                if (dto.NumeroCuotas.Value > 24)
                {
                    throw new InvalidOperationException("El número máximo permitido es 24 cuotas.");
                }

                if (!dto.FechaPrimerVencimiento.HasValue)
                {
                    throw new InvalidOperationException("Ingresa la fecha del primer vencimiento.");
                }

                if (dto.MontoPagado >= total)
                {
                    throw new InvalidOperationException("Para cobro en cuotas, debe quedar un saldo pendiente.");
                }

                if (dto.Cuotas != null && dto.Cuotas.Count > 0)
                {
                    if (dto.Cuotas.Count != dto.NumeroCuotas.Value)
                    {
                        throw new InvalidOperationException("La cantidad de cuotas no coincide con el número de cuotas indicado.");
                    }

                    var numerosCuota = dto.Cuotas.Select(c => c.NumeroCuota).ToList();

                    if (numerosCuota.Any(n => n <= 0 || n > dto.NumeroCuotas.Value))
                    {
                        throw new InvalidOperationException("Existe una cuota con número fuera del rango permitido.");
                    }

                    if (numerosCuota.Distinct().Count() != dto.NumeroCuotas.Value)
                    {
                        throw new InvalidOperationException("Las cuotas no pueden estar repetidas.");
                    }

                    if (dto.Cuotas.Any(c => !c.FechaVencimiento.HasValue))
                    {
                        throw new InvalidOperationException("Todas las cuotas deben tener fecha de vencimiento.");
                    }

                    if (dto.Cuotas.Any(c => c.MontoCuota <= 0))
                    {
                        throw new InvalidOperationException("El monto de cada cuota debe ser mayor a 0.");
                    }

                    var saldoFinanciar = Math.Round(total - dto.MontoPagado, 2);
                    var totalCuotas = Math.Round(dto.Cuotas.Sum(c => c.MontoCuota), 2);

                    if (saldoFinanciar != totalCuotas)
                    {
                        throw new InvalidOperationException("La suma de las cuotas debe ser igual al saldo pendiente.");
                    }
                }
            }
        }

        private static object CrearCuotasJson(VentaCrearDto dto)
        {
            if (dto.TipoPago != "Cuotas")
            {
                return DBNull.Value;
            }

            if (dto.Cuotas == null || dto.Cuotas.Count == 0)
            {
                return DBNull.Value;
            }

            var cuotas = dto.Cuotas
                .OrderBy(c => c.NumeroCuota)
                .Select(c => new
                {
                    numeroCuota = c.NumeroCuota,
                    fechaVencimiento = c.FechaVencimiento?.Date.ToString("yyyy-MM-dd")
                })
                .ToList();

            return JsonSerializer.Serialize(cuotas);
        }

        private static object CrearCuotasCobradasJson(PagoVentaCrearDto dto)
        {
            if (dto.IdsCuotasCobradas == null || dto.IdsCuotasCobradas.Count == 0)
            {
                return DBNull.Value;
            }

            var idsCuotas = dto.IdsCuotasCobradas
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (idsCuotas.Count == 0)
            {
                return DBNull.Value;
            }

            return JsonSerializer.Serialize(idsCuotas);
        }

        private static DataTable CrearDetalleDataTable(IEnumerable<VentaDetalleCrearDto> detalles)
        {
            var table = new DataTable();

            table.Columns.Add("IdElementoCatalogo", typeof(int));
            table.Columns.Add("Cantidad", typeof(int));
            table.Columns.Add("PrecioUnitario", typeof(decimal));

            foreach (var item in detalles)
            {
                table.Rows.Add(
                    item.IdElementoCatalogo,
                    item.Cantidad,
                    item.PrecioUnitario
                );
            }

            return table;
        }

        private static VentaRegistroResultadoDto LeerResultadoVenta(SqlDataReader reader)
        {
            return new VentaRegistroResultadoDto
            {
                IdVenta = LeerInt(reader, "IdVenta"),
                Subtotal = LeerDecimal(reader, "Subtotal"),
                Igv = LeerDecimal(reader, "Igv"),
                Total = LeerDecimal(reader, "Total"),
                TotalPagado = LeerDecimal(reader, "TotalPagado"),
                SaldoPendiente = LeerDecimal(reader, "SaldoPendiente"),
                EstadoPago = LeerString(reader, "EstadoPago"),
                Mensaje = LeerString(reader, "Mensaje")
            };
        }

        private static string ObtenerMensajeSql(SqlException ex)
        {
            if (ex.Errors.Count > 0)
            {
                return ex.Errors[0].Message;
            }

            return ex.Message;
        }

        private static int LeerInt(SqlDataReader reader, string columna)
        {
            return reader[columna] == DBNull.Value ? 0 : Convert.ToInt32(reader[columna]);
        }

        private static int? LeerNullableInt(SqlDataReader reader, string columna)
        {
            return reader[columna] == DBNull.Value ? null : Convert.ToInt32(reader[columna]);
        }

        private static decimal LeerDecimal(SqlDataReader reader, string columna)
        {
            return reader[columna] == DBNull.Value ? 0 : Convert.ToDecimal(reader[columna]);
        }

        private static string LeerString(SqlDataReader reader, string columna)
        {
            return reader[columna] == DBNull.Value ? string.Empty : reader[columna].ToString() ?? string.Empty;
        }

        private static string? LeerNullableString(SqlDataReader reader, string columna)
        {
            return reader[columna] == DBNull.Value ? null : reader[columna].ToString();
        }

        private static DateTime LeerDateTime(SqlDataReader reader, string columna)
        {
            return reader[columna] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader[columna]);
        }
    }
}