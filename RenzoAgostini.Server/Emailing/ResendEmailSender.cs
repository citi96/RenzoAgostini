using RenzoAgostini.Server.Emailing.Interfaces;
using RenzoAgostini.Server.Emailing.Models;
using Resend;

namespace RenzoAgostini.Server.Emailing
{
    public class ResendEmailSender(IResend resend, IConfiguration config, ILogger<ResendEmailSender> logger) : ICustomEmailSender
    {
        public async Task<EmailResult> SendAsync(RenzoAgostini.Server.Emailing.Models.EmailMessage message, CancellationToken ct = default)
        {
            var resendMessage = new Resend.EmailMessage();

            // Handle From
            if (message.From is null)
            {
                var defaultFrom = config["Email:From"];
                if (string.IsNullOrWhiteSpace(defaultFrom))
                {
                    return EmailResult.Fail("From address is missing and no default is configured.");
                }
                resendMessage.From = defaultFrom;
            }
            else
            {
                resendMessage.From = $"{message.From.Name} <{message.From.Address}>";
            }

            // Handle Recipients
            if (message.To.Count == 0)
                return EmailResult.Fail("At least one recipient is required.");

            foreach (var to in message.To) resendMessage.To.Add(to.Address);
            foreach (var cc in message.Cc) resendMessage.Cc.Add(cc.Address);
            foreach (var bcc in message.Bcc) resendMessage.Bcc.Add(bcc.Address);

            if (message.ReplyTo is not null)
                resendMessage.ReplyTo.Add(message.ReplyTo.Address);

            resendMessage.Subject = message.Subject ?? "";

            // Handle Body
            if (!string.IsNullOrWhiteSpace(message.HtmlBody))
                resendMessage.HtmlBody = message.HtmlBody;

            if (!string.IsNullOrWhiteSpace(message.TextBody))
                resendMessage.TextBody = message.TextBody;

            // Handle Attachments
            foreach (var attachment in message.Attachments)
            {
                resendMessage.Attachments.Add(new Resend.EmailAttachment
                {
                    Filename = attachment.FileName,
                    Content = attachment.Content
                });
            }

            try
            {
                var response = await resend.EmailSendAsync(resendMessage, ct);

                if (response.Success)
                {
                    logger.LogInformation("Email sent successfully via Resend.");
                    return EmailResult.Ok();
                }

                return EmailResult.Fail("Unknown error sending email.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending email via Resend");
                return EmailResult.Fail(ex.Message);
            }
        }
    }
}
