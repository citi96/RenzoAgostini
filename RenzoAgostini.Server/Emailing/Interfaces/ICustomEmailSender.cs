using System.Net.Mail;
using RenzoAgostini.Server.Emailing.Models;

namespace RenzoAgostini.Server.Emailing.Interfaces
{
    public interface ICustomEmailSender
    {
        Task<EmailResult> SendAsync(EmailMessage message, CancellationToken ct = default);
    }
}
