using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Caja;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class CajaService : ICajaService
    {
        private readonly Bd3sContext _context;

        public CajaService(Bd3sContext context)
        {
            _context = context;
        }

        public async Task<CajaActivaDto?> ObtenerCajaActivaAsync(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new InvalidOperationException("El usuario no es válido.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ObtenerCajaActiva", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdUsuario", idUsuario);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return null;
                }

                return LeerCajaActiva(reader);
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<CajaActivaDto> AbrirCajaAsync(CajaAbrirDto dto)
        {
            if (dto.IdUsuarioApertura <= 0)
            {
                throw new InvalidOperationException("El usuario de apertura no es válido.");
            }

            if (dto.SaldoInicial < 0)
            {
                throw new InvalidOperationException("El saldo inicial no puede ser negativo.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_AbrirCaja", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdUsuarioApertura", dto.IdUsuarioApertura);
            command.Parameters.AddWithValue("@SaldoInicial", dto.SaldoInicial);
            command.Parameters.AddWithValue(
                "@ObservacionApertura",
                ValorONull(dto.ObservacionApertura)
            );

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("No se pudo abrir la caja.");
                }

                return LeerCajaActiva(reader);
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<CajaResumenDto> ObtenerResumenAsync(int idUsuario, int? idCaja)
        {
            if (idUsuario <= 0)
            {
                throw new InvalidOperationException("El usuario no es válido.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ObtenerResumenCaja", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdUsuario", idUsuario);
            command.Parameters.AddWithValue("@IdCaja", idCaja.HasValue ? idCaja.Value : DBNull.Value);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("No se pudo obtener el resumen de caja.");
                }

                return new CajaResumenDto
                {
                    IdCaja = LeerInt(reader, "IdCaja"),
                    IdEstadoCaja = LeerInt(reader, "IdEstadoCaja"),
                    EstadoCaja = LeerString(reader, "EstadoCaja"),
                    FechaApertura = LeerDateTime(reader, "FechaApertura"),
                    FechaCierre = LeerNullableDateTime(reader, "FechaCierre"),
                    SaldoInicial = LeerDecimal(reader, "SaldoInicial"),

                    IngresosPorVenta = LeerDecimal(reader, "IngresosPorVenta"),
                    IngresosManuales = LeerDecimal(reader, "IngresosManuales"),
                    AjustesIngreso = LeerDecimal(reader, "AjustesIngreso"),

                    EgresosPorCompra = LeerDecimal(reader, "EgresosPorCompra"),
                    EgresosManuales = LeerDecimal(reader, "EgresosManuales"),
                    AjustesEgreso = LeerDecimal(reader, "AjustesEgreso"),

                    TotalIngresos = LeerDecimal(reader, "TotalIngresos"),
                    TotalEgresos = LeerDecimal(reader, "TotalEgresos"),
                    SaldoSistema = LeerDecimal(reader, "SaldoSistema"),

                    SaldoContado = LeerNullableDecimal(reader, "SaldoContado"),
                    Diferencia = LeerNullableDecimal(reader, "Diferencia"),

                    ObservacionApertura = LeerNullableString(reader, "ObservacionApertura"),
                    ObservacionCierre = LeerNullableString(reader, "ObservacionCierre")
                };
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<List<MovimientoCajaDto>> ListarMovimientosAsync(
            int idUsuario,
            int? idCaja,
            DateTime? fechaInicio,
            DateTime? fechaFin
        )
        {
            if (idUsuario <= 0)
            {
                throw new InvalidOperationException("El usuario no es válido.");
            }

            var movimientos = new List<MovimientoCajaDto>();

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ListarMovimientosCaja", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdUsuario", idUsuario);
            command.Parameters.AddWithValue("@IdCaja", idCaja.HasValue ? idCaja.Value : DBNull.Value);
            command.Parameters.AddWithValue("@FechaInicio", fechaInicio.HasValue ? fechaInicio.Value.Date : DBNull.Value);
            command.Parameters.AddWithValue("@FechaFin", fechaFin.HasValue ? fechaFin.Value.Date : DBNull.Value);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    movimientos.Add(new MovimientoCajaDto
                    {
                        IdMovimientoCaja = LeerInt(reader, "IdMovimientoCaja"),
                        IdCaja = LeerInt(reader, "IdCaja"),
                        IdTipoMovimientoCaja = LeerInt(reader, "IdTipoMovimientoCaja"),
                        TipoMovimiento = LeerString(reader, "TipoMovimiento"),

                        IdVenta = LeerNullableInt(reader, "IdVenta"),
                        IdCompra = LeerNullableInt(reader, "IdCompra"),
                        IdPagoVenta = LeerNullableInt(reader, "IdPagoVenta"),
                        IdPagoCompra = LeerNullableInt(reader, "IdPagoCompra"),

                        IdUsuarioRegistro = LeerInt(reader, "IdUsuarioRegistro"),
                        UsuarioRegistro = LeerString(reader, "UsuarioRegistro"),

                        MetodoPago = LeerNullableString(reader, "MetodoPago"),
                        Monto = LeerDecimal(reader, "Monto"),
                        Descripcion = LeerNullableString(reader, "Descripcion"),
                        FechaMovimiento = LeerDateTime(reader, "FechaMovimiento"),
                        OrigenMovimiento = LeerString(reader, "OrigenMovimiento"),
                        EsAutomatico = LeerBool(reader, "EsAutomatico"),
                        Estado = LeerBool(reader, "Estado"),
                        Ingreso = LeerDecimal(reader, "Ingreso"),
                        Egreso = LeerDecimal(reader, "Egreso")
                    });
                }

                return movimientos;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<CajaOperacionResultadoDto> RegistrarMovimientoManualAsync(
            MovimientoCajaManualDto dto
        )
        {
            if (dto.IdUsuarioRegistro <= 0)
            {
                throw new InvalidOperationException("El usuario de registro no es válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.TipoMovimiento))
            {
                throw new InvalidOperationException("Selecciona el tipo de movimiento.");
            }

            if (string.IsNullOrWhiteSpace(dto.MetodoPago))
            {
                throw new InvalidOperationException("Selecciona el método de pago.");
            }

            if (dto.Monto <= 0)
            {
                throw new InvalidOperationException("El monto debe ser mayor a 0.");
            }

            if (string.IsNullOrWhiteSpace(dto.Descripcion) || dto.Descripcion.Trim().Length < 5)
            {
                throw new InvalidOperationException("Ingresa una descripción válida.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_RegistrarMovimientoCajaManual", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdUsuarioRegistro", dto.IdUsuarioRegistro);
            command.Parameters.AddWithValue("@TipoMovimiento", dto.TipoMovimiento.Trim());
            command.Parameters.AddWithValue("@MetodoPago", dto.MetodoPago.Trim());
            command.Parameters.AddWithValue("@Monto", dto.Monto);
            command.Parameters.AddWithValue("@Descripcion", dto.Descripcion.Trim());

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("No se pudo registrar el movimiento de caja.");
                }

                return new CajaOperacionResultadoDto
                {
                    Mensaje = LeerString(reader, "Mensaje"),
                    IdCaja = LeerInt(reader, "IdCaja")
                };
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<CajaActivaDto> CerrarCajaAsync(CajaCerrarDto dto)
        {
            if (dto.IdUsuarioCierre <= 0)
            {
                throw new InvalidOperationException("El usuario de cierre no es válido.");
            }

            if (dto.IdCaja <= 0)
            {
                throw new InvalidOperationException("Selecciona una caja válida.");
            }

            if (dto.SaldoContado < 0)
            {
                throw new InvalidOperationException("El saldo contado no puede ser negativo.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_CerrarCaja", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdUsuarioCierre", dto.IdUsuarioCierre);
            command.Parameters.AddWithValue("@IdCaja", dto.IdCaja);
            command.Parameters.AddWithValue("@SaldoContado", dto.SaldoContado);
            command.Parameters.AddWithValue("@ObservacionCierre", ValorONull(dto.ObservacionCierre));

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("No se pudo cerrar la caja.");
                }

                return LeerCajaActiva(reader);
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<List<CajaReporteDto>> ObtenerReporteAsync(
            int idUsuario,
            DateTime? fechaInicio,
            DateTime? fechaFin
        )
        {
            if (idUsuario <= 0)
            {
                throw new InvalidOperationException("El usuario no es válido.");
            }

            var cajas = new List<CajaReporteDto>();

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ReporteCaja", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdUsuario", idUsuario);
            command.Parameters.AddWithValue("@FechaInicio", fechaInicio.HasValue ? fechaInicio.Value.Date : DBNull.Value);
            command.Parameters.AddWithValue("@FechaFin", fechaFin.HasValue ? fechaFin.Value.Date : DBNull.Value);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    cajas.Add(new CajaReporteDto
                    {
                        IdCaja = LeerInt(reader, "IdCaja"),
                        EstadoCaja = LeerString(reader, "EstadoCaja"),
                        FechaApertura = LeerDateTime(reader, "FechaApertura"),
                        FechaCierre = LeerNullableDateTime(reader, "FechaCierre"),
                        SaldoInicial = LeerDecimal(reader, "SaldoInicial"),
                        TotalIngresos = LeerDecimal(reader, "TotalIngresos"),
                        TotalEgresos = LeerDecimal(reader, "TotalEgresos"),
                        SaldoSistema = LeerDecimal(reader, "SaldoSistema"),
                        SaldoFinal = LeerNullableDecimal(reader, "SaldoFinal"),
                        SaldoContado = LeerNullableDecimal(reader, "SaldoContado"),
                        Diferencia = LeerNullableDecimal(reader, "Diferencia"),
                        UsuarioApertura = LeerString(reader, "UsuarioApertura"),
                        UsuarioCierre = LeerNullableString(reader, "UsuarioCierre"),
                        ObservacionApertura = LeerNullableString(reader, "ObservacionApertura"),
                        ObservacionCierre = LeerNullableString(reader, "ObservacionCierre")
                    });
                }

                return cajas;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        private static CajaActivaDto LeerCajaActiva(SqlDataReader reader)
        {
            return new CajaActivaDto
            {
                IdCaja = LeerInt(reader, "IdCaja"),
                IdUsuarioApertura = LeerInt(reader, "IdUsuarioApertura"),
                UsuarioApertura = LeerNullableString(reader, "UsuarioApertura"),
                IdUsuarioCierre = LeerNullableInt(reader, "IdUsuarioCierre"),
                UsuarioCierre = LeerNullableString(reader, "UsuarioCierre"),
                IdEstadoCaja = LeerInt(reader, "IdEstadoCaja"),
                EstadoCaja = LeerString(reader, "EstadoCaja"),
                FechaApertura = LeerDateTime(reader, "FechaApertura"),
                FechaCierre = LeerNullableDateTime(reader, "FechaCierre"),
                SaldoInicial = LeerDecimal(reader, "SaldoInicial"),
                TotalIngresos = LeerDecimal(reader, "TotalIngresos"),
                TotalEgresos = LeerDecimal(reader, "TotalEgresos"),
                SaldoSistema = LeerDecimal(reader, "SaldoSistema"),
                SaldoFinal = LeerNullableDecimal(reader, "SaldoFinal"),
                SaldoContado = LeerNullableDecimal(reader, "SaldoContado"),
                Diferencia = LeerNullableDecimal(reader, "Diferencia"),
                ObservacionApertura = LeerNullableString(reader, "ObservacionApertura"),
                ObservacionCierre = LeerNullableString(reader, "ObservacionCierre"),
                Mensaje = LeerNullableString(reader, "Mensaje")
            };
        }

        private static object ValorONull(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return DBNull.Value;
            }

            return valor.Trim();
        }

        private static string ObtenerMensajeSql(SqlException ex)
        {
            if (ex.Errors.Count > 0)
            {
                return ex.Errors[0].Message;
            }

            return ex.Message;
        }

        private static bool ExisteColumna(SqlDataReader reader, string columna)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columna, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static int LeerInt(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(reader[columna]);
        }

        private static int? LeerNullableInt(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(reader[columna]);
        }

        private static string LeerString(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return string.Empty;
            }

            return reader[columna]?.ToString() ?? string.Empty;
        }

        private static string? LeerNullableString(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return null;
            }

            var valor = reader[columna]?.ToString();

            return string.IsNullOrWhiteSpace(valor) ? null : valor;
        }

        private static decimal LeerDecimal(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToDecimal(reader[columna]);
        }

        private static decimal? LeerNullableDecimal(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDecimal(reader[columna]);
        }

        private static DateTime LeerDateTime(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return DateTime.MinValue;
            }

            return Convert.ToDateTime(reader[columna]);
        }

        private static DateTime? LeerNullableDateTime(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(reader[columna]);
        }

        private static bool LeerBool(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return false;
            }

            return Convert.ToBoolean(reader[columna]);
        }
    }
}