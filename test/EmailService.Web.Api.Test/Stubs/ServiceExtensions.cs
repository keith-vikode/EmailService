using EmailService.Core.Services;
using EmailService.Web.Api.Test.Stubs;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static void AddInMemoryStorageServices(this IServiceCollection services)
        {
            services.AddSingleton<IEmailQueueSender, InMemoryEmailQueue>();
            services.AddSingleton<IEmailQueueBlobStore, InMemoryEmailQueueBlobStore>();
            services.AddSingleton<IEmailLogWriter, InMemoryEmailLog>();
            services.AddSingleton<IEmailLogReader, InMemoryEmailLog>();
        }
    }
}
