using EmailService.Core;
using EmailService.Core.Entities;
using EmailService.Core.Services;
using EmailService.Storage.Azure;
using EmailService.Transports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Web.ProcessorJob
{
    public class Program
    {
        private static readonly CancellationTokenSource Cancellation = new CancellationTokenSource();

        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory().AddConsole(LogLevel.Trace);
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<Program>();

        private static IConfiguration Configuration;

        public static void Main(string[] args)
        {
            Configuration = GetConfig(args);

            IEmailQueueReceiver<AzureEmailQueueMessage> receiver;
            IEmailQueueBlobStore blobStore;
            IEmailTemplateStore templateStore;
            IMemoryCache cache;
            IEmailLogWriter logWriter;
            IEmailTransportFactory transportFactory = EmailTransportFactory.Instance;

            Logger.LogInformation("Loading dependencies...");
            SetupDependencies(out receiver, out blobStore, out templateStore, out cache, out logWriter);

            Logger.LogInformation("Intializing queue processor...");
            var processor = new QueueProcessor<AzureEmailQueueMessage>(
                receiver,
                blobStore,
                templateStore,
                transportFactory,
                logWriter,
                LoggerFactory);

            Logger.LogInformation("Listening for messages", ConsoleColor.Cyan);

            var task = processor.RunAsync(Cancellation.Token);
            task.ContinueWith(t =>
            {
                var ex = t.Exception;
                if (ex != null)
                {
                    Logger.LogCritical("Error listening to messages:\n{0}", ex);
                }

                Cancellation.Cancel();
            }, TaskContinuationOptions.OnlyOnFaulted);

            Console.ReadLine();
            Logger.LogInformation("Stopping service...");

            Cancellation.Cancel();
        }

        private static void SetupDependencies(
            out IEmailQueueReceiver<AzureEmailQueueMessage> receiver,
            out IEmailQueueBlobStore blobStore,
            out IEmailTemplateStore templateStore,
            out IMemoryCache cache,
            out IEmailLogWriter logWriter)
        {
            var storageOptions = Options.Create(new AzureStorageOptions
            {
                ConnectionString = Configuration.GetConnectionString("Storage")
            });
            receiver = new StorageEmailQueue(storageOptions, LoggerFactory);
            blobStore = new AzureEmailQueueBlobStore(storageOptions, LoggerFactory);
            logWriter = new StorageEmailLog(storageOptions, LoggerFactory);

            var cacheOptions = Options.Create(new MemoryCacheOptions
            {
                CompactOnMemoryPressure = true,
                ExpirationScanFrequency = TimeSpan.FromMinutes(1)
            });
            cache = new MemoryCache(cacheOptions);

            var builder = new DbContextOptionsBuilder<EmailServiceContext>();
            builder.UseSqlServer(Configuration.GetConnectionString("SqlServer"));
            builder.UseMemoryCache(cache);
            templateStore = new DbTemplateStore(builder.Options);
        }

        private static IConfiguration GetConfig(string[] args)
        {
            // allow configuration from either environment variables
            // or the command line args
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
        }
    }
}
