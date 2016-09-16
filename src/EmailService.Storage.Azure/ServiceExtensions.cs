using EmailService.Core.Services;
using EmailService.Storage.Azure;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static void AddAzureStorageServices(this IServiceCollection services, Action<AzureStorageOptions> options)
        {
            services.Configure(options);
            services.AddSingleton<IEmailQueueSender, StorageEmailQueue>();
            services.AddSingleton<IEmailQueueBlobStore, AzureEmailQueueBlobStore>();
        }
    }
}
