using EmailService.Core.Services;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using EmailService.Core;
using Microsoft.Extensions.Logging;

namespace EmailService.Storage.Azure
{
    public class StorageEmailQueue : IEmailQueueSender, IEmailQueueReceiver<AzureEmailQueueMessage>
    {
        private const int MaxDequeue = 32;

        private readonly CloudStorageAccount _account;
        private readonly ILogger _logger;

        private bool _initialized;

        private Lazy<CloudQueue> _queue;
        private Lazy<CloudQueue> _poisonQueue;

        public StorageEmailQueue(IOptions<AzureStorageOptions> options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StorageEmailQueue>();

            // let this throw an exception if it fails, we'll get better information
            // from the core class than wrapping it in our own error
            _account = CloudStorageAccount.Parse(options.Value.ConnectionString);

            // instantiate these queues lazily as required
            _queue = SetupQueue(_account, options.Value.PendingQueueName);
            _poisonQueue = SetupQueue(_account, options.Value.PendingPoisonQueueName);
        }

        public int MaxMessagesToRetrieve => MaxDequeue;

        public async Task CompleteAsync(AzureEmailQueueMessage message, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Completing message {0}", message.Token);

            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            await _queue.Value.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await _queue.Value.CreateIfNotExistsAsync(null, null, cancellationToken);
            await _poisonQueue.Value.CreateIfNotExistsAsync(null, null, cancellationToken);
            _initialized = true;
        }
        
        public async Task MoveToPoisonQueueAsync(AzureEmailQueueMessage message, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Moving message {0} to poison queue", message.Token);

            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }
            
            var cloudQueueMessage = CloudQueueMessage.CreateCloudQueueMessageFromByteArray(message.Token.EncodeBytes());
            await _queue.Value.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            await _poisonQueue.Value.AddMessageAsync(cloudQueueMessage, null, null, null, null, cancellationToken);
        }

        public async Task<IEnumerable<AzureEmailQueueMessage>> ReceiveAsync(int number, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Receiving {0} message(s)", number);

            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            if (number > MaxMessagesToRetrieve)
            {
                throw new ArgumentOutOfRangeException(nameof(number), $"The {GetType().Name} implementation of `IEmailQueueReceiver<TMessage>` can only dequeue up to {MaxMessagesToRetrieve} message(s) at a time");
            }

            if (number < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(number), $"The parameter `number` of `IEmailQueueReceiver` must be a positive integer greater than or equal to one, as you can't retrieve zero messages or less");
            }

            var tokens = new List<AzureEmailQueueMessage>();

            foreach (var message in await _queue.Value.GetMessagesAsync(number, null, null, null, cancellationToken))
            {
                tokens.Add(new AzureEmailQueueMessage
                {
                    Token = EmailQueueToken.DecodeBytes(message.AsBytes),
                    MessageId = message.Id,
                    PopReceipt = message.PopReceipt,
                    DequeueCount = message.DequeueCount
                });
            }

            return tokens;
        }
        
        public async Task SendAsync(EmailQueueToken token, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Sending new queue message {0}", token);

            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }

            var message = CloudQueueMessage.CreateCloudQueueMessageFromByteArray(token.EncodeBytes());
            await _queue.Value.AddMessageAsync(message, null, null, null, null, cancellationToken);
        }

        private static Lazy<CloudQueue> SetupQueue(CloudStorageAccount account, string queueName)
        {
            return new Lazy<CloudQueue>(() =>
            {
                var client = account.CreateCloudQueueClient();
                return client.GetQueueReference(queueName);
            }, true);
        }
    }
}
