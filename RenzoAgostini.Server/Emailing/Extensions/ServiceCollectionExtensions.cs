using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RenzoAgostini.Server.Config;
using RenzoAgostini.Server.Emailing.Interfaces;

namespace RenzoAgostini.Server.Emailing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailing(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<SmtpOptions>()
            .Bind(config.GetSection("Email:Smtp"))
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host), "Host richiesto")
            .Validate(
                o => o.DefaultBcc.All(address =>
                    string.IsNullOrWhiteSpace(address) || new EmailAddressAttribute().IsValid(address)),
                "DefaultBcc contiene indirizzi non validi");

        services.AddSingleton<ICustomEmailSender, SmtpEmailSender>();
        services.AddHealthChecks().AddCheck<SmtpHealthCheck>("smtp", HealthStatus.Degraded);
        return services;
    }
}
