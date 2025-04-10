using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace ERMS.Services
{
    public class NullEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For testing, no email is sent.
            return Task.CompletedTask;
        }
    }
}
