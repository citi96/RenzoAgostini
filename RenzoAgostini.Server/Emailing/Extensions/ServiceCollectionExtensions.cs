using RenzoAgostini.Server.Emailing.Interfaces;
using Resend;

namespace RenzoAgostini.Server.Emailing.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailing(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<ResendClientOptions>()
                .Configure(o => o.ApiToken = config["Resend:ApiToken"]!);

            services.AddHttpClient<ResendClient>();
            services.AddTransient<IResend, ResendClient>();

            services.AddTransient<ICustomEmailSender, ResendEmailSender>();

            return services;
        }
    }
}
