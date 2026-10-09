using System.Threading.Tasks;

namespace Hunarmand.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}
