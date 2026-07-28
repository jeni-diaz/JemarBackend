using Azure;
using Azure.Communication.Email;
using Jemar.Aplication.Abstractions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Infrastructure.Services
{
    // Envío de correo vía Azure Communication Services (API HTTP). Se usa en
    // Azure porque el App Service en plan Free bloquea el SMTP saliente; la API
    // HTTP de ACS no se ve afectada por ese bloqueo. En local se sigue usando
    // EmailService (SMTP), según la config (ver Program.cs).
    public class AcsEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public AcsEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            IReadOnlyDictionary<string, byte[]>? inlineImages = null)
        {
            var connectionString = _config["Communication:ConnectionString"]
                ?? throw new InvalidOperationException("La configuración 'Communication:ConnectionString' no existe.");
            var senderAddress = _config["Communication:SenderAddress"]
                ?? throw new InvalidOperationException("La configuración 'Communication:SenderAddress' no existe.");

            var client = new EmailClient(connectionString);

            // Correos de notificación (no-reply): el remitente DoNotReply del
            // dominio comunica que no se responden y así no se llena una casilla.
            var message = new EmailMessage(
                senderAddress: senderAddress,
                recipientAddress: toEmail,
                content: new EmailContent(subject) { Html = htmlBody });

            if (inlineImages != null)
            {
                foreach (var image in inlineImages)
                {
                    // ContentId hace que el adjunto sea inline y referenciable
                    // desde el HTML con <img src="cid:clave">.
                    var attachment = new EmailAttachment(
                        $"{image.Key}.png", "image/png", BinaryData.FromBytes(image.Value))
                    {
                        ContentId = image.Key
                    };
                    message.Attachments.Add(attachment);
                }
            }

            await client.SendAsync(WaitUntil.Completed, message);
        }
    }
}
