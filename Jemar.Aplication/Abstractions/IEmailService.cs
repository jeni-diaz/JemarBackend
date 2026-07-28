using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions
{
    public interface IEmailService
    {
        // inlineImages: imágenes a incrustar en el HTML por Content-ID (cid).
        // La clave es el cid al que apunta el <img src="cid:clave"> y el valor
        // son los bytes de la imagen (PNG). Si es null, se envía solo el HTML.
        Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            IReadOnlyDictionary<string, byte[]>? inlineImages = null);
    }
}
