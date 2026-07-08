using System.Net;
using System.Net.Mail;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarConAdjuntoAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            string? rutaArchivoAdjunto
        )
        {
            var host = _configuration["Smtp:Host"];
            var portText = _configuration["Smtp:Port"];
            var user = _configuration["Smtp:User"];
            var password = _configuration["Smtp:Password"];
            var from = _configuration["Smtp:From"];
            var fromName = _configuration["Smtp:FromName"] ?? "Empresa 3S";

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portText) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(from))
            {
                throw new InvalidOperationException(
                    "La configuración SMTP no está completa. Revisa appsettings.json."
                );
            }

            if (!int.TryParse(portText, out var port))
            {
                throw new InvalidOperationException("El puerto SMTP no es válido.");
            }

            using var message = new MailMessage
            {
                From = new MailAddress(from, fromName),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = false
            };

            message.To.Add(destinatario);

            if (!string.IsNullOrWhiteSpace(rutaArchivoAdjunto) &&
                File.Exists(rutaArchivoAdjunto))
            {
                message.Attachments.Add(new Attachment(rutaArchivoAdjunto));
            }

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}