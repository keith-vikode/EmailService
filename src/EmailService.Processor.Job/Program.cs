using EmailService.Core;
using EmailService.Core.Entities;
using EmailService.Storage.Azure;
using EmailService.Transports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EmailService.Web.ProcessorJob
{
    public class Program
    {
        private static IConfiguration Configuration;

        public static void Main(string[] args)
        {
            // allow configuration from either environment variables
            // or the command line args
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(LogLevel.Debug);

            var storageOptions = Options.Create(new AzureStorageOptions
            {
                ConnectionString = Configuration.GetConnectionString("Storage")
            });
            var receiver = new StorageEmailQueue(storageOptions);
            var blobStore = new AzureBlobStore(storageOptions);
            
            var builder = new DbContextOptionsBuilder<EmailServiceContext>();
            builder.UseSqlServer(Configuration.GetConnectionString("SqlServer"));
            var context = new EmailServiceContext(builder.Options);

            var sender = new EmailSender(context, EmailTransportFactory.Instance);

            var processor = new QueueProcessor<AzureEmailQueueMessage>(sender, receiver, blobStore, loggerFactory);

            var token = new CancellationTokenSource();

            Console.WriteLine("Press [enter] to exit");
            var task = processor.RunAsync(token.Token);
            
            Console.ReadLine();
            token.Cancel();
        }
    }
}
