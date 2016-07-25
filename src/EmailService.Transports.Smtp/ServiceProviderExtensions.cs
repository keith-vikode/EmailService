using EmailService.Core.Services;
using EmailService.Transports.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Adds SMTP email sending services.
        /// </summary>
        /// <param name="services">Services collection to extend</param>
        /// <param name="configuration">Configuration sub-section to bind to</param>
        public static void AddSmtp(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpOptions>(options => configuration.Bind(options));
            services.AddScoped<IEmailTransport, SmtpEmailTransport>();
        }
    }
}
