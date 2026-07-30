using Azure;
using Azure.Communication.Email;
using Jemar.Aplication.Abstractions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Infrastructure.Services
{
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

            var message = new EmailMessage(
                senderAddress: senderAddress,
                recipientAddress: toEmail,
                content: new EmailContent(subject) { Html = htmlBody });

            if (inlineImages != null)
            {
                foreach (var image in inlineImages)
                {
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
