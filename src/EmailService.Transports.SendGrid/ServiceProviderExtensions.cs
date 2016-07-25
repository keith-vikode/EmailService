using EmailService.Core.Services;
using EmailService.Transports.SendGrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService
{
    public static class ServiceProviderExtensions
    {
        public static void AddSendGrid(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SendGridOptions>(options => configuration.Bind(options));
            services.AddScoped<IEmailTransport, SendGridEmailTransport>();
        }
    }
}
