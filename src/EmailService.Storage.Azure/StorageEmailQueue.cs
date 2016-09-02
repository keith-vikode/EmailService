using EmailService.Core.Services;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace EmailService.Storage.Azure
{
    public class StorageEmailQueue : IEmailQueueSender, IEmailQueueReceiver<AzureEmailQueueMessage>
    {
        private readonly CloudStorageAccount _account;

        private bool _initialized;

        private Lazy<CloudQueue> _queue;

        public StorageEmailQueue(IOptions<AzureStorageOptions> options)
        {
            // let this throw an exception if it fails, we'll get better information
            //. from the core class than wrapping it in our own error
            _account = CloudStorageAccount.Parse(options.Value.ConnectionString);

            _queue = new Lazy<CloudQueue>(() =>
            {
                var client = _account.CreateCloudQueueClient();
                return client.GetQueueReference(options.Value.QueueName);
            }, false);
        }

        public async Task CompleteAsync(AzureEmailQueueMessage message, CancellationToken cancellationToken)
        {        
            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            await _queue.Value.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await _queue.Value.CreateIfNotExistsAsync(null, null, cancellationToken);
            _initialized = true;
        }

        public async Task<IEnumerable<AzureEmailQueueMessage>> ReceiveAsync(CancellationToken cancellationToken)
        {
            const int DequeueCount = 32;
            
            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var tokens = new List<AzureEmailQueueMessage>();

            foreach (var message in await _queue.Value.GetMessagesAsync(DequeueCount, null, null, null, cancellationToken))
            {
                Guid g;
                if (Guid.TryParse(message.AsString, out g))
                {
                    tokens.Add(new AzureEmailQueueMessage
                    {
                        Token = g,
                        MessageId = message.Id,
                        PopReceipt = message.PopReceipt,
                        DequeueCount = message.DequeueCount
                    });
                }
            }

            return tokens;
        }
        
        public async Task SendAsync(Guid token, CancellationToken cancellationToken)
        {
            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var message = new CloudQueueMessage(token.ToString());
            await _queue.Value.AddMessageAsync(message, null, null, null, null, cancellationToken);
        }
    }
}
