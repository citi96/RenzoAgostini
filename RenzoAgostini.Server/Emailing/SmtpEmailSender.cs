using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using RenzoAgostini.Server.Config;
using RenzoAgostini.Server.Emailing.Interfaces;
using RenzoAgostini.Server.Emailing.Models;

namespace RenzoAgostini.Server.Emailing
{
    public class SmtpEmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<SmtpEmailSender> logger) : ICustomEmailSender
    {
        private readonly SmtpOptions _smtpOptions = smtpOptions.Value;

        public async Task<EmailResult> SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            if (message.From is null)
            {
                if (string.IsNullOrWhiteSpace(_smtpOptions.DefaultFromAddress))
                    return EmailResult.Fail("From mancante e DefaultFromAddress non configurato.");

                message = message with { From = new EmailAddress(_smtpOptions.DefaultFromAddress!, _smtpOptions.DefaultFromName) };
            }
            if (message.To.Count == 0)
                return EmailResult.Fail("Almeno un destinatario è richiesto.");

            var mime = BuildMime(message);
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    using var client = new SmtpClient();

                    if (_smtpOptions.SkipCertificateValidation)
                        client.ServerCertificateValidationCallback = (_, _, _, _) => true;

                    client.Timeout = _smtpOptions.TimeoutSeconds * 1000;

                    await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, ParseSecure(_smtpOptions.SecureSocket), ct);

                    if (!string.IsNullOrWhiteSpace(_smtpOptions.Username))
                        await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password, ct);

                    await client.SendAsync(mime, ct);
                    await client.DisconnectAsync(true, ct);

                    return EmailResult.Ok();
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "SMTP send tentativo {Attempt}", attempt + 1);
                    if (attempt >= _smtpOptions.MaxRetries) 
                        return EmailResult.Fail(ex.Message);

                    await Task.Delay(TimeSpan.FromSeconds(_smtpOptions.RetryBackoffSeconds * Math.Max(1, attempt + 1)), ct);
                }
            }
        }

        private static MimeMessage BuildMime(EmailMessage m)
        {
            var msg = new MimeMessage();

            if (m.From is not null) msg.From.Add(new MailboxAddress(m.From.Name, m.From.Address));
            foreach (var a in m.To) msg.To.Add(new MailboxAddress(a.Name, a.Address));
            foreach (var a in m.Cc) msg.Cc.Add(new MailboxAddress(a.Name, a.Address));
            foreach (var a in m.Bcc) msg.Bcc.Add(new MailboxAddress(a.Name, a.Address));
            if (m.ReplyTo is not null) msg.ReplyTo.Add(new MailboxAddress(m.ReplyTo.Name, m.ReplyTo.Address));

            msg.Subject = m.Subject ?? "";

            var body = new BodyBuilder
            {
                TextBody = m.TextBody,
                HtmlBody = m.HtmlBody
            };

            foreach (var att in m.Attachments)
                body.Attachments.Add(att.FileName, att.Content, ContentType.Parse(att.ContentType));

            msg.Body = body.ToMessageBody();
            return msg;
        }

        private static SecureSocketOptions ParseSecure(SecureSocketMode secureSocketMode) => 
            secureSocketMode switch
            {
                SecureSocketMode.Auto => SecureSocketOptions.Auto,
                SecureSocketMode.StartTls => SecureSocketOptions.StartTls,
                SecureSocketMode.StartTlsWhenAvailable => SecureSocketOptions.StartTlsWhenAvailable,
                SecureSocketMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
                _ => SecureSocketOptions.None
            };
    }
}
