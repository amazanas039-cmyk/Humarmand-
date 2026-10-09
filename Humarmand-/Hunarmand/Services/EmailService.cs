using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hunarmand.Services
{
    /// <summary>
    /// Stub email service. In production, integrate with SendGrid, Mailgun, or SMTP.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            // TODO: Integrate with real email provider (SendGrid/Mailgun)
            _logger.LogInformation("[EMAIL STUB] To: {To} | Subject: {Subject} | Body: {Body}", to, subject, body);
            return Task.CompletedTask;
        }
    }
}
