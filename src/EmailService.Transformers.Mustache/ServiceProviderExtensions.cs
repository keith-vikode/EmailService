using EmailService.Core.Services;
using EmailService.Transformers.Mustache;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService
{
    public static class ServiceProviderExtensions
    {
        public static void AddMustacheTemplateTransformer(this IServiceCollection services)
        {
            services.AddSingleton<ITemplateTransformer, MustacheTemplateTransformer>();
        }
    }
}
