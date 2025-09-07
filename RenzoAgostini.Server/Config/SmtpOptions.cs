using System.ComponentModel.DataAnnotations;
using MailKit.Security;

namespace RenzoAgostini.Server.Config
{
    public enum SecureSocketMode 
    { 
        Auto, 
        StartTls, 
        StartTlsWhenAvailable, 
        SslOnConnect, 
        None 
    }

    public sealed class SmtpOptions
    {
        [Required] public string Host { get; init; } = default!;
        [Range(1, 65535)] public int Port { get; init; } = 587;
        public string? Username { get; init; }
        public string? Password { get; init; }

        public SecureSocketMode SecureSocket { get; init; } = SecureSocketMode.StartTls;

        // Usa solo in dev
        public bool SkipCertificateValidation { get; init; } = false;

        [EmailAddress] public string? DefaultFromAddress { get; init; }
        public string? DefaultFromName { get; init; }

        [Range(1, 120)] public int TimeoutSeconds { get; init; } = 30;
        [Range(0, 10)] public int MaxRetries { get; init; } = 2;
        [Range(0, 60)] public int RetryBackoffSeconds { get; init; } = 2;
    }
}
