using System.Threading.Tasks;

namespace Jemar.Aplication.Abstractions
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
