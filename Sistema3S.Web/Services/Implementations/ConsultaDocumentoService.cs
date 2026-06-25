using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Cliente;
using Sistema3S.Web.Models;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class ConsultaDocumentoService : IConsultaDocumentoService
    {
        private readonly Bd3sContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ConsultaDocumentoService(
            Bd3sContext context,
            HttpClient httpClient,
            IConfiguration configuration
        )
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ConsultaDniResultadoDto> ConsultarDniAsync(
            string dni,
            int? idUsuarioRegistro = null
        )
        {
            dni = NormalizarNumero(dni);

            if (dni.Length != 8)
            {
                return new ConsultaDniResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = dni,
                    Mensaje = "El DNI debe tener 8 dígitos."
                };
            }

            var idTipoDocumentoDni = await ObtenerIdTipoDocumentoAsync("DNI");

            if (idTipoDocumentoDni == null)
            {
                return new ConsultaDniResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = dni,
                    Mensaje = "No existe el tipo de documento DNI en la base de datos."
                };
            }

            var clienteExistente = await ObtenerClientePersonaNaturalExistenteAsync(
                idTipoDocumentoDni.Value,
                dni
            );

            if (clienteExistente != null)
            {
                return clienteExistente;
            }

            var apiKey = ObtenerApiKey();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new ConsultaDniResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = dni,
                    Mensaje = "No se configuró la API key de Decolecta."
                };
            }

            var url = ConstruirUrl($"reniec/dni?numero={dni}");

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var contenido = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    await RegistrarConsultaExternaAsync(
                        "RENIEC",
                        idUsuarioRegistro,
                        null,
                        dni,
                        false,
                        contenido,
                        $"Error HTTP {(int)response.StatusCode}"
                    );

                    return new ConsultaDniResultadoDto
                    {
                        Exitoso = false,
                        NumeroDocumento = dni,
                        Mensaje = "No se encontró información del DNI o el servicio no respondió correctamente."
                    };
                }

                var datos = JsonSerializer.Deserialize<DecolectaDniResponse>(
                    contenido,
                    _jsonOptions
                );

                if (datos == null || string.IsNullOrWhiteSpace(datos.DocumentNumber))
                {
                    await RegistrarConsultaExternaAsync(
                        "RENIEC",
                        idUsuarioRegistro,
                        null,
                        dni,
                        false,
                        contenido,
                        "Respuesta vacía o inválida."
                    );

                    return new ConsultaDniResultadoDto
                    {
                        Exitoso = false,
                        NumeroDocumento = dni,
                        Mensaje = "No se encontraron datos para el DNI consultado."
                    };
                }

                await RegistrarConsultaExternaAsync(
                    "RENIEC",
                    idUsuarioRegistro,
                    null,
                    dni,
                    true,
                    contenido,
                    null
                );

                return new ConsultaDniResultadoDto
                {
                    Exitoso = true,
                    ClienteYaExiste = false,
                    IdClienteExistente = null,
                    NumeroDocumento = datos.DocumentNumber ?? dni,
                    Nombres = datos.FirstName,
                    ApellidoPaterno = datos.FirstLastName,
                    ApellidoMaterno = datos.SecondLastName,
                    NombreCompleto = datos.FullName,
                    Mensaje = "Datos RENIEC encontrados correctamente."
                };
            }
            catch (Exception ex)
            {
                await RegistrarConsultaExternaAsync(
                    "RENIEC",
                    idUsuarioRegistro,
                    null,
                    dni,
                    false,
                    null,
                    ex.Message
                );

                return new ConsultaDniResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = dni,
                    Mensaje = "Ocurrió un error al consultar RENIEC."
                };
            }
        }

        public async Task<ConsultaRucResultadoDto> ConsultarRucAsync(
            string ruc,
            int? idUsuarioRegistro = null
        )
        {
            ruc = NormalizarNumero(ruc);

            if (ruc.Length != 11)
            {
                return new ConsultaRucResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = ruc,
                    Mensaje = "El RUC debe tener 11 dígitos."
                };
            }

            var idTipoDocumentoRuc = await ObtenerIdTipoDocumentoAsync("RUC");

            if (idTipoDocumentoRuc == null)
            {
                return new ConsultaRucResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = ruc,
                    Mensaje = "No existe el tipo de documento RUC en la base de datos."
                };
            }

            var clienteExistente = await ObtenerClienteEmpresaExistenteAsync(
                idTipoDocumentoRuc.Value,
                ruc
            );

            if (clienteExistente != null)
            {
                return clienteExistente;
            }

            var apiKey = ObtenerApiKey();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new ConsultaRucResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = ruc,
                    Mensaje = "No se configuró la API key de Decolecta."
                };
            }

            var url = ConstruirUrl($"sunat/ruc?numero={ruc}");

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var contenido = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    await RegistrarConsultaExternaAsync(
                        "SUNAT",
                        idUsuarioRegistro,
                        null,
                        ruc,
                        false,
                        contenido,
                        $"Error HTTP {(int)response.StatusCode}"
                    );

                    return new ConsultaRucResultadoDto
                    {
                        Exitoso = false,
                        NumeroDocumento = ruc,
                        Mensaje = "No se encontró información del RUC o el servicio no respondió correctamente."
                    };
                }

                var datos = JsonSerializer.Deserialize<DecolectaRucResponse>(
                    contenido,
                    _jsonOptions
                );

                if (datos == null || string.IsNullOrWhiteSpace(datos.NumeroDocumento))
                {
                    await RegistrarConsultaExternaAsync(
                        "SUNAT",
                        idUsuarioRegistro,
                        null,
                        ruc,
                        false,
                        contenido,
                        "Respuesta vacía o inválida."
                    );

                    return new ConsultaRucResultadoDto
                    {
                        Exitoso = false,
                        NumeroDocumento = ruc,
                        Mensaje = "No se encontraron datos para el RUC consultado."
                    };
                }

                var ubigeo = await ObtenerUbigeoPorCodigoAsync(datos.Ubigeo);

                await RegistrarConsultaExternaAsync(
                    "SUNAT",
                    idUsuarioRegistro,
                    null,
                    ruc,
                    true,
                    contenido,
                    null
                );

                return new ConsultaRucResultadoDto
                {
                    Exitoso = true,
                    ClienteYaExiste = false,
                    IdClienteExistente = null,
                    NumeroDocumento = datos.NumeroDocumento ?? ruc,
                    RazonSocial = datos.RazonSocial,
                    NombreComercial = null,
                    EstadoSunat = datos.Estado,
                    CondicionSunat = datos.Condicion,
                    Direccion = datos.Direccion,
                    CodigoUbigeo = datos.Ubigeo,
                    IdUbigeo = ubigeo?.IdUbigeo,
                    Ubicacion = ubigeo == null
                        ? null
                        : FormatearUbicacion(
                            ubigeo.CodigoUbigeo,
                            ubigeo.Departamento,
                            ubigeo.Provincia,
                            ubigeo.Distrito
                        ),
                    Departamento = datos.Departamento,
                    Provincia = datos.Provincia,
                    Distrito = datos.Distrito,
                    Mensaje = "Datos SUNAT encontrados correctamente."
                };
            }
            catch (Exception ex)
            {
                await RegistrarConsultaExternaAsync(
                    "SUNAT",
                    idUsuarioRegistro,
                    null,
                    ruc,
                    false,
                    null,
                    ex.Message
                );

                return new ConsultaRucResultadoDto
                {
                    Exitoso = false,
                    NumeroDocumento = ruc,
                    Mensaje = "Ocurrió un error al consultar SUNAT."
                };
            }
        }

        private async Task<int?> ObtenerIdTipoDocumentoAsync(string nombre)
        {
            return await _context.TipoDocumento
                .Where(t => t.Nombre.ToLower() == nombre.ToLower() && t.Estado)
                .Select(t => (int?)t.IdTipoDocumento)
                .FirstOrDefaultAsync();
        }

        private async Task<ConsultaDniResultadoDto?> ObtenerClientePersonaNaturalExistenteAsync(
            int idTipoDocumento,
            string dni
        )
        {
            var existente = await (
                from c in _context.Cliente
                join pn in _context.ClientePersonaNatural
                    on c.IdCliente equals pn.IdCliente
                where c.IdTipoDocumento == idTipoDocumento
                      && c.NumeroDocumento == dni
                select new
                {
                    c.IdCliente,
                    c.NumeroDocumento,
                    pn.Nombres,
                    pn.ApellidoPaterno,
                    pn.ApellidoMaterno
                }
            ).FirstOrDefaultAsync();

            if (existente == null)
            {
                return null;
            }

            return new ConsultaDniResultadoDto
            {
                Exitoso = true,
                ClienteYaExiste = true,
                IdClienteExistente = existente.IdCliente,
                NumeroDocumento = existente.NumeroDocumento,
                Nombres = existente.Nombres,
                ApellidoPaterno = existente.ApellidoPaterno,
                ApellidoMaterno = existente.ApellidoMaterno,
                NombreCompleto =
                    $"{existente.ApellidoPaterno} {existente.ApellidoMaterno} {existente.Nombres}".Trim(),
                Mensaje = "Este cliente ya está registrado en el sistema."
            };
        }

        private async Task<ConsultaRucResultadoDto?> ObtenerClienteEmpresaExistenteAsync(
            int idTipoDocumento,
            string ruc
        )
        {
            var existente = await (
                from c in _context.Cliente
                join ce in _context.ClienteEmpresa
                    on c.IdCliente equals ce.IdCliente
                join u in _context.Ubigeo
                    on c.IdUbigeo equals u.IdUbigeo into ubicaciones
                from ub in ubicaciones.DefaultIfEmpty()
                where c.IdTipoDocumento == idTipoDocumento
                      && c.NumeroDocumento == ruc
                select new
                {
                    c.IdCliente,
                    c.NumeroDocumento,
                    c.Direccion,
                    ce.RazonSocial,
                    ce.NombreComercial,
                    Ubigeo = ub
                }
            ).FirstOrDefaultAsync();

            if (existente == null)
            {
                return null;
            }

            return new ConsultaRucResultadoDto
            {
                Exitoso = true,
                ClienteYaExiste = true,
                IdClienteExistente = existente.IdCliente,
                NumeroDocumento = existente.NumeroDocumento,
                RazonSocial = existente.RazonSocial,
                NombreComercial = existente.NombreComercial,
                Direccion = existente.Direccion,
                CodigoUbigeo = existente.Ubigeo?.CodigoUbigeo,
                IdUbigeo = existente.Ubigeo?.IdUbigeo,
                Ubicacion = existente.Ubigeo == null
                    ? null
                    : FormatearUbicacion(
                        existente.Ubigeo.CodigoUbigeo,
                        existente.Ubigeo.Departamento,
                        existente.Ubigeo.Provincia,
                        existente.Ubigeo.Distrito
                    ),
                Departamento = existente.Ubigeo?.Departamento,
                Provincia = existente.Ubigeo?.Provincia,
                Distrito = existente.Ubigeo?.Distrito,
                Mensaje = "Este cliente ya está registrado en el sistema."
            };
        }

        private async Task<Ubigeo?> ObtenerUbigeoPorCodigoAsync(string? codigoUbigeo)
        {
            if (string.IsNullOrWhiteSpace(codigoUbigeo))
            {
                return null;
            }

            return await _context.Ubigeo
                .FirstOrDefaultAsync(u => u.CodigoUbigeo == codigoUbigeo);
        }

        private async Task RegistrarConsultaExternaAsync(
            string nombreServicio,
            int? idUsuarioRegistro,
            int? idCliente,
            string numeroConsultado,
            bool exitoso,
            string? resultadoJson,
            string? mensajeError
        )
        {
            var idTipoServicio = await _context.TipoServicioExterno
                .Where(t => t.Nombre.ToLower() == nombreServicio.ToLower() && t.Estado)
                .Select(t => (int?)t.IdTipoServicioExterno)
                .FirstOrDefaultAsync();

            if (idTipoServicio == null)
            {
                return;
            }

            var consulta = new ConsultaExterna
            {
                IdTipoServicioExterno = idTipoServicio.Value,
                IdUsuarioRegistro = idUsuarioRegistro,
                IdCliente = idCliente,
                NumeroConsultado = numeroConsultado,
                Exitoso = exitoso,
                ResultadoJson = resultadoJson,
                MensajeError = mensajeError
            };

            _context.ConsultaExterna.Add(consulta);
            await _context.SaveChangesAsync();
        }

        private string ObtenerApiKey()
        {
            return _configuration["Decolecta:ApiKey"] ?? string.Empty;
        }

        private string ConstruirUrl(string endpoint)
        {
            var baseUrl = _configuration["Decolecta:BaseUrl"]
                ?? "https://api.decolecta.com/v1/";

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            return baseUrl + endpoint;
        }

        private static string NormalizarNumero(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            return new string(valor.Where(char.IsDigit).ToArray());
        }

        private static string FormatearUbicacion(
            string? codigoUbigeo,
            string departamento,
            string provincia,
            string distrito
        )
        {
            return $"{codigoUbigeo ?? "------"} - {departamento} / {provincia} / {distrito}";
        }

        private class DecolectaDniResponse
        {
            [JsonPropertyName("first_name")]
            public string? FirstName { get; set; }

            [JsonPropertyName("first_last_name")]
            public string? FirstLastName { get; set; }

            [JsonPropertyName("second_last_name")]
            public string? SecondLastName { get; set; }

            [JsonPropertyName("full_name")]
            public string? FullName { get; set; }

            [JsonPropertyName("document_number")]
            public string? DocumentNumber { get; set; }
        }

        private class DecolectaRucResponse
        {
            [JsonPropertyName("razon_social")]
            public string? RazonSocial { get; set; }

            [JsonPropertyName("numero_documento")]
            public string? NumeroDocumento { get; set; }

            [JsonPropertyName("estado")]
            public string? Estado { get; set; }

            [JsonPropertyName("condicion")]
            public string? Condicion { get; set; }

            [JsonPropertyName("direccion")]
            public string? Direccion { get; set; }

            [JsonPropertyName("ubigeo")]
            public string? Ubigeo { get; set; }

            [JsonPropertyName("distrito")]
            public string? Distrito { get; set; }

            [JsonPropertyName("provincia")]
            public string? Provincia { get; set; }

            [JsonPropertyName("departamento")]
            public string? Departamento { get; set; }
        }
    }
}