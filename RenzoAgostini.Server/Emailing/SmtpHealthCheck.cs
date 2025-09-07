using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RenzoAgostini.Server.Config;
using RenzoAgostini.Server.Emailing.Interfaces;

namespace RenzoAgostini.Server.Emailing
{
    public class SmtpHealthCheck(IOptions<SmtpOptions> smtpOptions, ILogger<SmtpEmailSender> logger) : IHealthCheck
    {
        private readonly SmtpOptions _smtpOptions = smtpOptions.Value;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = new SmtpClient();
                if (_smtpOptions.SkipCertificateValidation) 
                    client.ServerCertificateValidationCallback = (_, _, _, _) => true;

                await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, ParseSecure(_smtpOptions.SecureSocket), cancellationToken);
                if (!string.IsNullOrEmpty(_smtpOptions.Username)) 
                    await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password, cancellationToken);

                await client.DisconnectAsync(true, cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex) 
            { 
                return HealthCheckResult.Unhealthy(ex.Message); 
            }
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