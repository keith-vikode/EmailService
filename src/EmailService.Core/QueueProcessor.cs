using EmailService.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core
{
    public class QueueProcessor<TMessage> where TMessage : IEmailQueueMessage
    {
        private readonly ILogger _logger;
        private readonly IEmailQueueReceiver<TMessage> _receiver;
        private readonly IEmailMessageBlobStore _blobStore;
        private readonly EmailSender _sender;

        private const int MaxDequeue = 5;
        private const int MinInterval = 1;
        private const byte MaxInterval = 60;
        private const byte Exponent = 2;
        private int _interval = MinInterval;

        public QueueProcessor(
            EmailSender sender,
            IEmailQueueReceiver<TMessage> receiver,
            IEmailMessageBlobStore blobStore,
            ILoggerFactory loggerFactory)
        {
            _sender = sender;
            _receiver = receiver;
            _blobStore = blobStore;
            _logger = loggerFactory.CreateLogger<QueueProcessor<TMessage>>();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogTrace("Checking for messages...");

                var messages = await _receiver.ReceiveAsync(cancellationToken);
                if (messages.Any())
                {
                    _logger.LogInformation("Received {0} message(s)", messages.Count());

                    // process messages
                    messages.AsParallel().ForAll(async message =>
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await ProcessMessage(message, cancellationToken);
                            _logger.LogInformation("Message {0} completed", message.Token);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error processing message {0}:\n{1}", message.Token, ex.ToString());
                        }
                    });

                    // reset the interval once we receive a message
                    _interval = MinInterval;
                }
                else
                {
                    _interval = Math.Max(MaxInterval, _interval * Exponent);
                }
            }
        }

        public async Task ProcessMessage(TMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing message {0} on try {1}", message, message.DequeueCount);

            var args = await _blobStore.GetAsync(message.Token, cancellationToken);
            if (args != null)
            {
                // actually try to transform and send the email
                var success = await _sender.SendEmailAsync(args);

                if (success)
                {
                    _logger.LogInformation("Successfully sent email for message {0} on try {1}", message.Token, message.DequeueCount);
                }
                else if (message.DequeueCount >= MaxDequeue)
                {
                    _logger.LogError("Failed to send email for message {0} after {1} tries, giving up", message.Token, message.DequeueCount);
                }
                else
                {
                    _logger.LogWarning("Failed to send email for message {0} after {1} tries", message.Token, message.DequeueCount);
                }

                // if the message was successfully processed, or has reached or exceeded
                // the maximum number of retries, then remove it from the queue and blob
                // store; we'll deal with logging shortly
                if (success || message.DequeueCount >= MaxDequeue)
                {
                    await _blobStore.RemoveAsync(message.Token, cancellationToken);
                    await _receiver.CompleteAsync(message, cancellationToken);
                }
            }
            else
            {
                // if we couldn't find a matching blob, then raise an error and remove
                // the message from the queue
                _logger.LogError("Could not find message params for {0}", message);
                await _receiver.CompleteAsync(message, cancellationToken);
            }
        }
    }
}
