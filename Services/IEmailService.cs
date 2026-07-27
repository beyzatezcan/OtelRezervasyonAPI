using System.Threading.Tasks;

namespace otelrezervation.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
