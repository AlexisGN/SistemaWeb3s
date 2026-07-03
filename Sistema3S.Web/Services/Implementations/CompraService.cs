using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Comun;
using Sistema3S.Web.DTOs.Compra;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class CompraService : ICompraService
    {
        private readonly Bd3sContext _context;

        public CompraService(Bd3sContext context)
        {
            _context = context;
        }

        public async Task<CompraRegistroResultadoDto> RegistrarCompraCompletaAsync(CompraCrearDto dto)
        {
            ValidarCompra(dto);

            var detallesJson = JsonSerializer.Serialize(
                dto.Detalles.Select(d => new
                {
                    idProducto = d.IdProducto,
                    cantidad = d.Cantidad,
                    precioCompra = d.PrecioCompra
                })
            );

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_RegistrarCompraCompleta", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdProveedor", dto.IdProveedor);
            command.Parameters.AddWithValue("@IdUsuarioRegistro", dto.IdUsuarioRegistro);

            command.Parameters.AddWithValue("@TipoComprobanteProveedor", dto.TipoComprobanteProveedor.Trim());
            command.Parameters.AddWithValue("@SerieComprobante", dto.SerieComprobante.Trim());
            command.Parameters.AddWithValue("@NumeroComprobante", dto.NumeroComprobante.Trim());
            command.Parameters.AddWithValue("@FechaEmisionComprobante", dto.FechaEmisionComprobante!.Value.Date);

            command.Parameters.AddWithValue("@ObservacionCompra", ValorONull(dto.ObservacionCompra));

            command.Parameters.AddWithValue("@TieneGuia", dto.GuiaRemision.TieneGuia);
            command.Parameters.AddWithValue("@NumeroGuia", ValorONull(dto.GuiaRemision.NumeroGuia));
            command.Parameters.AddWithValue("@FechaEmisionGuia", FechaONull(dto.GuiaRemision.FechaEmision));
            command.Parameters.AddWithValue("@FechaTrasladoGuia", FechaONull(dto.GuiaRemision.FechaTraslado));
            command.Parameters.AddWithValue("@PuntoPartida", ValorONull(dto.GuiaRemision.PuntoPartida));
            command.Parameters.AddWithValue("@PuntoLlegada", ValorONull(dto.GuiaRemision.PuntoLlegada));
            command.Parameters.AddWithValue("@Transportista", ValorONull(dto.GuiaRemision.Transportista));
            command.Parameters.AddWithValue("@RucTransportista", ValorONull(dto.GuiaRemision.RucTransportista));
            command.Parameters.AddWithValue("@PlacaVehiculo", ValorONull(dto.GuiaRemision.PlacaVehiculo));
            command.Parameters.AddWithValue("@ObservacionTraslado", ValorONull(dto.GuiaRemision.Observacion));

            command.Parameters.AddWithValue("@TipoPago", dto.TipoPago.Trim());
            command.Parameters.AddWithValue("@MetodoPago", dto.MetodoPago.Trim());
            command.Parameters.AddWithValue("@MontoPagado", dto.MontoPagado);
            command.Parameters.AddWithValue("@NumeroCuotas", dto.NumeroCuotas.HasValue ? dto.NumeroCuotas.Value : DBNull.Value);
            command.Parameters.AddWithValue("@FechaPrimerVencimiento", FechaONull(dto.FechaPrimerVencimiento));
            command.Parameters.AddWithValue("@ObservacionPago", ValorONull(dto.ObservacionPago));

            command.Parameters.AddWithValue("@DetallesJson", detallesJson);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("No se pudo registrar la compra.");
                }

                return new CompraRegistroResultadoDto
                {
                    IdCompra = Convert.ToInt32(reader["IdCompra"]),
                    Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                    Igv = Convert.ToDecimal(reader["Igv"]),
                    Total = Convert.ToDecimal(reader["Total"]),
                    TotalPagado = Convert.ToDecimal(reader["TotalPagado"]),
                    SaldoPendiente = Convert.ToDecimal(reader["SaldoPendiente"]),
                    Mensaje = "Compra registrada correctamente."
                };
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<ResultadoPaginadoDto<CompraListadoDto>> ListarAsync(
            string? buscar,
            string? estadoPago,
            DateTime? fechaInicio,
            DateTime? fechaFin,
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
                tamanioPagina = 8;
            }

            if (tamanioPagina > 50)
            {
                tamanioPagina = 50;
            }

            var compras = new List<CompraListadoDto>();
            var totalRegistros = 0;

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ListarCompras", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Buscar", ValorONull(buscar));
            command.Parameters.AddWithValue("@EstadoPago", ValorONull(estadoPago));
            command.Parameters.AddWithValue("@FechaInicio", FechaONull(fechaInicio));
            command.Parameters.AddWithValue("@FechaFin", FechaONull(fechaFin));
            command.Parameters.AddWithValue("@Pagina", pagina);
            command.Parameters.AddWithValue("@TamanioPagina", tamanioPagina);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (totalRegistros == 0 && reader["TotalRegistros"] != DBNull.Value)
                {
                    totalRegistros = Convert.ToInt32(reader["TotalRegistros"]);
                }

                var serie = reader["SerieComprobante"]?.ToString() ?? string.Empty;
                var numero = reader["NumeroComprobante"]?.ToString() ?? string.Empty;

                compras.Add(new CompraListadoDto
                {
                    IdCompra = Convert.ToInt32(reader["IdCompra"]),
                    FechaCompra = Convert.ToDateTime(reader["FechaCompra"]),
                    FechaEmisionComprobante = reader["FechaEmisionComprobante"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["FechaEmisionComprobante"]),

                    TipoComprobanteProveedor = reader["TipoComprobanteProveedor"]?.ToString() ?? string.Empty,
                    SerieComprobante = serie,
                    NumeroComprobante = numero,
                    DocumentoCompleto = $"{serie}-{numero}",

                    IdProveedor = Convert.ToInt32(reader["IdProveedor"]),
                    RucProveedor = reader["Ruc"]?.ToString() ?? string.Empty,
                    RazonSocialProveedor = reader["RazonSocial"]?.ToString() ?? string.Empty,

                    Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                    Igv = Convert.ToDecimal(reader["Igv"]),
                    Total = Convert.ToDecimal(reader["Total"]),
                    TotalPagado = Convert.ToDecimal(reader["TotalPagado"]),
                    SaldoPendiente = Convert.ToDecimal(reader["SaldoPendiente"]),

                    EstadoCompra = reader["EstadoCompra"]?.ToString() ?? string.Empty,
                    EstadoPago = reader["EstadoPago"]?.ToString() ?? string.Empty,

                    TieneGuia = Convert.ToBoolean(reader["TieneGuia"])
                });
            }

            return new ResultadoPaginadoDto<CompraListadoDto>
            {
                Items = compras,
                Pagina = pagina,
                TamanioPagina = tamanioPagina,
                TotalRegistros = totalRegistros
            };
        }

        public async Task<CompraRegistroResultadoDto> RegistrarPagoAsync(PagoCompraCrearDto dto)
        {
            if (dto.IdCompra <= 0)
            {
                throw new InvalidOperationException("Selecciona una compra válida.");
            }

            if (dto.IdUsuarioRegistro <= 0)
            {
                throw new InvalidOperationException("El usuario de registro no es válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.MetodoPago))
            {
                throw new InvalidOperationException("Selecciona el método de pago.");
            }

            if (dto.MontoPagado <= 0)
            {
                throw new InvalidOperationException("El monto pagado debe ser mayor a 0.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_RegistrarPagoCompra", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdCompra", dto.IdCompra);
            command.Parameters.AddWithValue("@IdUsuarioRegistro", dto.IdUsuarioRegistro);
            command.Parameters.AddWithValue("@MetodoPago", dto.MetodoPago.Trim());
            command.Parameters.AddWithValue("@MontoPagado", dto.MontoPagado);
            command.Parameters.AddWithValue("@Observacion", ValorONull(dto.Observacion));

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("No se pudo registrar el pago.");
                }

                return new CompraRegistroResultadoDto
                {
                    IdCompra = Convert.ToInt32(reader["IdCompra"]),
                    TotalPagado = Convert.ToDecimal(reader["TotalPagado"]),
                    SaldoPendiente = Convert.ToDecimal(reader["SaldoPendiente"]),
                    Mensaje = "Pago registrado correctamente."
                };
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        public async Task<bool> AnularAsync(int idCompra, AnularCompraDto dto)
        {
            if (idCompra <= 0)
            {
                throw new InvalidOperationException("Selecciona una compra válida.");
            }

            if (dto.IdUsuarioRegistro <= 0)
            {
                throw new InvalidOperationException("El usuario de registro no es válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.Motivo) || dto.Motivo.Trim().Length < 5)
            {
                throw new InvalidOperationException("Ingresa un motivo de anulación válido.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_AnularCompra", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdCompra", idCompra);
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
        public async Task<CompraDetalleCompletoDto?> ObtenerDetalleAsync(int idCompra)
        {
            if (idCompra <= 0)
            {
                throw new InvalidOperationException("Selecciona una compra válida.");
            }

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ObtenerCompraDetalle", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdCompra", idCompra);

            await using var reader = await command.ExecuteReaderAsync();

            CompraDetalleCompletoDto? compra = null;

            if (await reader.ReadAsync())
            {
                compra = new CompraDetalleCompletoDto
                {
                    IdCompra = Convert.ToInt32(reader["IdCompra"]),
                    FechaCompra = Convert.ToDateTime(reader["FechaCompra"]),
                    FechaEmisionComprobante = reader["FechaEmisionComprobante"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["FechaEmisionComprobante"]),

                    TipoComprobanteProveedor = reader["TipoComprobanteProveedor"]?.ToString() ?? string.Empty,
                    SerieComprobante = reader["SerieComprobante"]?.ToString() ?? string.Empty,
                    NumeroComprobante = reader["NumeroComprobante"]?.ToString() ?? string.Empty,

                    Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                    Igv = Convert.ToDecimal(reader["Igv"]),
                    Total = Convert.ToDecimal(reader["Total"]),
                    TotalPagado = Convert.ToDecimal(reader["TotalPagado"]),
                    SaldoPendiente = Convert.ToDecimal(reader["SaldoPendiente"]),

                    Observacion = reader["Observacion"] == DBNull.Value ? null : reader["Observacion"]?.ToString(),

                    EstadoCompra = reader["EstadoCompra"]?.ToString() ?? string.Empty,
                    EstadoPago = reader["EstadoPago"]?.ToString() ?? string.Empty,

                    IdProveedor = Convert.ToInt32(reader["IdProveedor"]),
                    RucProveedor = reader["Ruc"]?.ToString() ?? string.Empty,
                    RazonSocialProveedor = reader["RazonSocial"]?.ToString() ?? string.Empty,
                    NombreComercialProveedor = reader["NombreComercial"] == DBNull.Value ? null : reader["NombreComercial"]?.ToString(),
                    CorreoProveedor = reader["Correo"] == DBNull.Value ? null : reader["Correo"]?.ToString(),
                    TelefonoProveedor = reader["Telefono"] == DBNull.Value ? null : reader["Telefono"]?.ToString(),
                    DireccionProveedor = reader["Direccion"] == DBNull.Value ? null : reader["Direccion"]?.ToString()
                };
            }

            if (compra == null)
            {
                return null;
            }

            await reader.NextResultAsync();

            if (await reader.ReadAsync())
            {
                compra.GuiaRemision = new GuiaRemisionCompraDetalleDto
                {
                    IdGuiaRemisionCompra = Convert.ToInt32(reader["IdGuiaRemisionCompra"]),
                    NumeroGuia = reader["NumeroGuia"]?.ToString() ?? string.Empty,
                    FechaEmision = Convert.ToDateTime(reader["FechaEmision"]),
                    FechaTraslado = Convert.ToDateTime(reader["FechaTraslado"]),
                    PuntoPartida = reader["PuntoPartida"]?.ToString() ?? string.Empty,
                    PuntoLlegada = reader["PuntoLlegada"]?.ToString() ?? string.Empty,
                    Transportista = reader["Transportista"] == DBNull.Value ? null : reader["Transportista"]?.ToString(),
                    RucTransportista = reader["RucTransportista"] == DBNull.Value ? null : reader["RucTransportista"]?.ToString(),
                    PlacaVehiculo = reader["PlacaVehiculo"] == DBNull.Value ? null : reader["PlacaVehiculo"]?.ToString(),
                    Observacion = reader["Observacion"] == DBNull.Value ? null : reader["Observacion"]?.ToString()
                };
            }

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                compra.Detalles.Add(new DetalleCompraDetalleDto
                {
                    IdDetalleCompra = Convert.ToInt32(reader["IdDetalleCompra"]),
                    IdProducto = Convert.ToInt32(reader["IdProducto"]),
                    CodigoProducto = reader["CodigoProducto"]?.ToString() ?? string.Empty,
                    Producto = reader["Producto"]?.ToString() ?? string.Empty,
                    Cantidad = Convert.ToInt32(reader["Cantidad"]),
                    PrecioCompra = Convert.ToDecimal(reader["PrecioCompra"]),
                    Subtotal = Convert.ToDecimal(reader["Subtotal"])
                });
            }

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                compra.Pagos.Add(new PagoCompraDetalleDto
                {
                    IdPagoCompra = Convert.ToInt32(reader["IdPagoCompra"]),
                    MetodoPago = reader["MetodoPago"]?.ToString() ?? string.Empty,
                    MontoPagado = Convert.ToDecimal(reader["MontoPagado"]),
                    FechaPago = Convert.ToDateTime(reader["FechaPago"]),
                    Observacion = reader["Observacion"] == DBNull.Value ? null : reader["Observacion"]?.ToString()
                });
            }

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                compra.Cuotas.Add(new CuotaCompraDetalleDto
                {
                    IdCuotaCompra = Convert.ToInt32(reader["IdCuotaCompra"]),
                    NumeroCuota = Convert.ToInt32(reader["NumeroCuota"]),
                    FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                    MontoCuota = Convert.ToDecimal(reader["MontoCuota"]),
                    MontoPagado = Convert.ToDecimal(reader["MontoPagado"]),
                    EstadoCuota = reader["EstadoCuota"]?.ToString() ?? string.Empty
                });
            }

            return compra;
        }
        private static void ValidarCompra(CompraCrearDto dto)
        {
            if (dto.IdProveedor <= 0)
            {
                throw new InvalidOperationException("Selecciona un proveedor.");
            }

            if (dto.IdUsuarioRegistro <= 0)
            {
                throw new InvalidOperationException("El usuario de registro no es válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.TipoComprobanteProveedor))
            {
                throw new InvalidOperationException("Selecciona el tipo de comprobante recibido.");
            }

            if (string.IsNullOrWhiteSpace(dto.SerieComprobante))
            {
                throw new InvalidOperationException("Ingresa la serie del comprobante.");
            }

            if (string.IsNullOrWhiteSpace(dto.NumeroComprobante))
            {
                throw new InvalidOperationException("Ingresa el número del comprobante.");
            }

            if (!dto.FechaEmisionComprobante.HasValue)
            {
                throw new InvalidOperationException("Ingresa la fecha de emisión del comprobante.");
            }

            if (dto.GuiaRemision.TieneGuia)
            {
                if (string.IsNullOrWhiteSpace(dto.GuiaRemision.NumeroGuia))
                {
                    throw new InvalidOperationException("Ingresa el número de guía de remisión.");
                }

                if (!dto.GuiaRemision.FechaEmision.HasValue)
                {
                    throw new InvalidOperationException("Ingresa la fecha de emisión de la guía.");
                }

                if (!dto.GuiaRemision.FechaTraslado.HasValue)
                {
                    throw new InvalidOperationException("Ingresa la fecha de traslado de la guía.");
                }

                if (string.IsNullOrWhiteSpace(dto.GuiaRemision.PuntoPartida))
                {
                    throw new InvalidOperationException("Ingresa el punto de partida de la guía.");
                }

                if (string.IsNullOrWhiteSpace(dto.GuiaRemision.PuntoLlegada))
                {
                    throw new InvalidOperationException("Ingresa el punto de llegada de la guía.");
                }
            }

            if (dto.Detalles == null || dto.Detalles.Count == 0)
            {
                throw new InvalidOperationException("Agrega al menos un producto a la compra.");
            }

            if (dto.Detalles.Any(d => d.IdProducto <= 0))
            {
                throw new InvalidOperationException("Hay un producto inválido en el detalle.");
            }

            if (dto.Detalles.Any(d => d.Cantidad <= 0))
            {
                throw new InvalidOperationException("La cantidad debe ser mayor a 0.");
            }

            if (dto.Detalles.Any(d => d.PrecioCompra <= 0))
            {
                throw new InvalidOperationException("El precio de compra debe ser mayor a 0.");
            }

            if (dto.Detalles.GroupBy(d => d.IdProducto).Any(g => g.Count() > 1))
            {
                throw new InvalidOperationException("No puedes repetir el mismo producto en la compra.");
            }

            if (string.IsNullOrWhiteSpace(dto.TipoPago))
            {
                throw new InvalidOperationException("Selecciona el tipo de pago.");
            }

            if (dto.TipoPago != "Total" &&
                dto.TipoPago != "Parcial" &&
                dto.TipoPago != "Cuotas")
            {
                throw new InvalidOperationException("Selecciona un tipo de pago válido.");
            }

            if (string.IsNullOrWhiteSpace(dto.MetodoPago))
            {
                throw new InvalidOperationException("Selecciona el método de pago.");
            }

            if (dto.MontoPagado < 0)
            {
                throw new InvalidOperationException("El monto pagado no puede ser negativo.");
            }

            var subtotal = dto.Detalles.Sum(d => d.Cantidad * d.PrecioCompra);
            var igv = Math.Round(subtotal * 0.18m, 2);
            var total = subtotal + igv;

            if (dto.MontoPagado > total)
            {
                throw new InvalidOperationException("El monto pagado no puede ser mayor al total.");
            }

            if (dto.TipoPago == "Total" && dto.MontoPagado != total)
            {
                throw new InvalidOperationException("Para pago total, el monto pagado debe ser igual al total.");
            }

            if (dto.TipoPago == "Parcial" && (dto.MontoPagado <= 0 || dto.MontoPagado >= total))
            {
                throw new InvalidOperationException("Para pago parcial, el monto pagado debe ser mayor a 0 y menor al total.");
            }

            if (dto.TipoPago == "Cuotas")
            {
                if (!dto.NumeroCuotas.HasValue || dto.NumeroCuotas.Value <= 0)
                {
                    throw new InvalidOperationException("Ingresa un número de cuotas válido.");
                }

                if (!dto.FechaPrimerVencimiento.HasValue)
                {
                    throw new InvalidOperationException("Ingresa la fecha del primer vencimiento.");
                }
            }
        }

        private static object ValorONull(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return DBNull.Value;
            }

            return valor.Trim();
        }

        private static object FechaONull(DateTime? fecha)
        {
            if (!fecha.HasValue)
            {
                return DBNull.Value;
            }

            return fecha.Value.Date;
        }

        private static string ObtenerMensajeSql(SqlException ex)
        {
            if (ex.Errors.Count > 0)
            {
                return ex.Errors[0].Message;
            }

            return ex.Message;
        }
    }
}