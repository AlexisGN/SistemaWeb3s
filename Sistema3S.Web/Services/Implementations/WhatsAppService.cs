using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public WhatsAppService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory
        )
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task EnviarDocumentoPdfAsync(
            string telefonoDestino,
            string rutaPdf,
            string nombreArchivo,
            string mensaje
        )
        {
            var version = _configuration["WhatsApp:GraphApiVersion"] ?? "v20.0";
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];
            var accessToken = _configuration["WhatsApp:AccessToken"];

            if (string.IsNullOrWhiteSpace(phoneNumberId))
            {
                throw new InvalidOperationException("No se configuró WhatsApp:PhoneNumberId.");
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException("No se configuró WhatsApp:AccessToken.");
            }

            if (string.IsNullOrWhiteSpace(telefonoDestino))
            {
                throw new InvalidOperationException("El teléfono destino no es válido.");
            }

            if (string.IsNullOrWhiteSpace(rutaPdf) || !File.Exists(rutaPdf))
            {
                throw new InvalidOperationException("No se encontró el PDF de la cotización para enviar por WhatsApp.");
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(25);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var mediaId = await SubirPdfAMetaAsync(
                    client,
                    version,
                    phoneNumberId,
                    rutaPdf,
                    nombreArchivo
                );

                await EnviarDocumentoAsync(
                    client,
                    version,
                    phoneNumberId,
                    telefonoDestino,
                    mediaId,
                    nombreArchivo,
                    mensaje
                );
            }
            catch (TaskCanceledException)
            {
                throw new InvalidOperationException(
                    "WhatsApp Cloud API no respondió a tiempo. Verifica token, PhoneNumberId, conexión y número destinatario permitido."
                );
            }
        }

        private static async Task<string> SubirPdfAMetaAsync(
            HttpClient client,
            string version,
            string phoneNumberId,
            string rutaPdf,
            string nombreArchivo
        )
        {
            var url = $"https://graph.facebook.com/{version}/{phoneNumberId}/media";

            await using var stream = File.OpenRead(rutaPdf);

            using var form = new MultipartFormDataContent();

            form.Add(new StringContent("whatsapp"), "messaging_product");
            form.Add(new StringContent("application/pdf"), "type");

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            form.Add(fileContent, "file", nombreArchivo);

            using var response = await client.PostAsync(url, form);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"No se pudo subir el PDF a WhatsApp Cloud API. Respuesta Meta: {responseBody}"
                );
            }

            using var json = JsonDocument.Parse(responseBody);

            if (!json.RootElement.TryGetProperty("id", out var idProperty))
            {
                throw new InvalidOperationException(
                    $"Meta no devolvió el identificador del archivo PDF. Respuesta: {responseBody}"
                );
            }

            var mediaId = idProperty.GetString();

            if (string.IsNullOrWhiteSpace(mediaId))
            {
                throw new InvalidOperationException("Meta devolvió un media_id vacío.");
            }

            return mediaId;
        }

        private static async Task EnviarDocumentoAsync(
            HttpClient client,
            string version,
            string phoneNumberId,
            string telefonoDestino,
            string mediaId,
            string nombreArchivo,
            string mensaje
        )
        {
            var url = $"https://graph.facebook.com/{version}/{phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = telefonoDestino,
                type = "document",
                document = new
                {
                    id = mediaId,
                    filename = nombreArchivo,
                    caption = mensaje
                }
            };

            var json = JsonSerializer.Serialize(payload);

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            using var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"No se pudo enviar el PDF por WhatsApp Cloud API. Respuesta Meta: {responseBody}"
                );
            }
        }
    }
}