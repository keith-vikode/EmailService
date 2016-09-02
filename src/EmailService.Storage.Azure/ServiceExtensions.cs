using EmailService.Core.Services;
using EmailService.Storage.Azure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EmailService
{
    public static class ServiceExtensions
    {
        public static void AddAzureStorageServices(this IServiceCollection services, Action<AzureStorageOptions> options)
        {
            services.Configure(options);
            services.AddSingleton<IEmailQueueSender, StorageEmailQueue>();
            services.AddSingleton<IEmailMessageBlobStore, AzureBlobStore>();
        }
    }
}
