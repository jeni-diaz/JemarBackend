using Jemar.Aplication.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Jemar.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            IReadOnlyDictionary<string, byte[]>? inlineImages = null)
        {
            var host = _config["Email:Host"]
                ?? throw new InvalidOperationException("La configuración 'Email:Host' no existe.");
            var fromAddress = _config["Email:FromAddress"]
                ?? throw new InvalidOperationException("La configuración 'Email:FromAddress' no existe.");
            var fromName = _config["Email:FromName"] ?? "Jemar Envíos";
            var username = _config["Email:Username"] ?? fromAddress;
            var password = _config["Email:Password"] ?? string.Empty;

            var port = int.TryParse(_config["Email:Port"], out var parsedPort) ? parsedPort : 587;
            var enableSsl = !bool.TryParse(_config["Email:EnableSsl"], out var ssl) || ssl;

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject
            };
            message.To.Add(new MailAddress(toEmail));

            if (inlineImages != null && inlineImages.Count > 0)
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(
                    htmlBody, null, MediaTypeNames.Text.Html);

                foreach (var image in inlineImages)
                {
                    var resource = new LinkedResource(new MemoryStream(image.Value), "image/png")
                    {
                        ContentId = image.Key,
                        TransferEncoding = TransferEncoding.Base64
                    };
                    htmlView.LinkedResources.Add(resource);
                }

                message.AlternateViews.Add(htmlView);
            }
            else
            {
                message.Body = htmlBody;
                message.IsBodyHtml = true;
            }

#pragma warning disable SYSLIB0014
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(username, password)
            };

            await client.SendMailAsync(message);
#pragma warning restore SYSLIB0014
        }
    }
}
