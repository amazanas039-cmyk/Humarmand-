using System.Threading.Tasks;

namespace SkillBridge.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}
