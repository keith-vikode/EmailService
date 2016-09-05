using EmailService.Core.Abstraction;
using EmailService.Core.Services;
using EmailService.Core.Templating;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core
{
    public class QueueProcessor<TMessage> where TMessage : IEmailQueueMessage
    {
        private readonly ILogger _logger;
        private readonly IEmailQueueReceiver<TMessage> _receiver;
        private readonly IEmailQueueBlobStore _blobStore;
        private readonly IEmailTemplateStore _templateStore;
        private readonly IEmailTransportFactory _transportFactory;
        private ITemplateTransformer _templateTransformer;

        private const int MaxDequeue = 5;
        private const int MinInterval = 32;
        private const int MaxInterval = 8192;
        private const byte Exponent = 2;
        private int _interval = MinInterval;

        public QueueProcessor(
            IEmailQueueReceiver<TMessage> receiver,
            IEmailQueueBlobStore blobStore,
            IEmailTemplateStore templateStore,
            IEmailTransportFactory transportFactory,
            ILoggerFactory loggerFactory)
        {
            _receiver = receiver;
            _blobStore = blobStore;
            _templateStore = templateStore;
            _transportFactory = transportFactory;
            _logger = loggerFactory.CreateLogger<QueueProcessor<TMessage>>();
        }

        /// <summary>
        /// Gets or sets the <see cref="ITemplateTransformer"/> to use when rendering emails.
        /// </summary>
        /// <remarks>
        /// This property allows you to plug in a custom transformer, or test against a mock.
        /// </remarks>
        public ITemplateTransformer Transformer
        {
            get
            {
                if (_templateTransformer == null)
                {
                    _templateTransformer = MustacheTemplateTransformer.Instance;
                }

                return _templateTransformer;
            }

            set
            {
                _templateTransformer = value;
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogTrace("Checking for messages...");
                
                var messages = await _receiver.ReceiveAsync(_receiver.MaxMessagesToRetrieve, cancellationToken);
                if (messages.Any())
                {
                    _logger.LogInformation("Received {0} message(s)", messages.Count());

                    messages.AsParallel().ForAll(async message =>
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await ProcessMessage(message, cancellationToken);
                            _logger.LogTrace("Message {0} completed", message.Token);
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
                    _interval = Math.Min(MaxInterval, _interval * Exponent);
                }

                _logger.LogTrace("Waiting for {0}ms", _interval);
                await Task.Delay(_interval);
            }
        }

        public async Task ProcessMessage(TMessage message, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Processing message {0} on try {1}", message, message.DequeueCount);

            var args = await _blobStore.GetAsync(message.Token, cancellationToken);
            if (args != null)
            {
                // actually try to transform and send the email
                var result = await TrySendEmailAsync(args);

                if (result.Succeeded)
                {
                    // successfully processed; remove the message and delete the blob data
                    await _receiver.CompleteAsync(message, cancellationToken);
                    await _blobStore.RemoveAsync(message.Token, cancellationToken);
                    _logger.LogInformation("Successfully sent email for message {0} on try {1}", message.Token, message.DequeueCount);
                }
                else if (message.DequeueCount >= MaxDequeue)
                {
                    // failed, and reached the maximum retry count; move the message to
                    // the poison queue and move the blob data to the bin of failure
                    await _receiver.MoveToPoisonQueueAsync(message, cancellationToken);
                    await _blobStore.MoveToPoisonStoreAsync(message.Token, cancellationToken);
                    _logger.LogError("Failed to send email for message {0} after {1} tries, giving up\n{2}", message.Token, message.DequeueCount, result.ErrorMessage);
                }
                else
                {
                    // failed, but we'll let it retry to see if that fixes it
                    _logger.LogWarning("Failed to send email for message {0} after {1} tries\n{2}", message.Token, message.DequeueCount, result.ErrorMessage);
                    if (result.Exception != null)
                    {
                        _logger.LogError(result.Exception.ToString());
                    }
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

        public async Task<EmailSenderResult> TrySendEmailAsync(EmailMessageParams args)
        {
            EmailTemplate email;

            var templateInfo = await _templateStore.GetTemplateAsync(args);
            if (templateInfo.ApplicationName == null)
            {
                return EmailSenderResult.Error("Invalid application ID", EmailSenderResult.ErrorCodes.TemplateNotFound);
            }

            if (args.Data != null)
            {
                try
                {
                    email = await Transformer.TransformTemplateAsync(templateInfo.Template, args.Data);
                }
                catch (Exception ex)
                {
                    return EmailSenderResult.Fault(ex);
                }
            }
            else
            {
                email = templateInfo.Template;
            }

            var senderParams = new SenderParams
            {
                To = args.To,
                CC = args.CC,
                Bcc = args.Bcc,
                Subject = email.Subject,
                Body = email.Body,
                SenderName = templateInfo.SenderName,
                SenderAddress = templateInfo.SenderAddress
            };

            bool success = false;
            while (!success && templateInfo.TransportQueue.Any())
            {
                var transportInfo = templateInfo.TransportQueue.Dequeue();

                try
                {
                    var transport = _transportFactory.CreateTransport(transportInfo);
                    if (await transport.SendAsync(senderParams))
                    {
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    return EmailSenderResult.Fault(ex);
                }
            }

            if (success)
            {
                return EmailSenderResult.Success;
            }

            return EmailSenderResult.Error("Error sending email", EmailSenderResult.ErrorCodes.Unhandled);
        }
    }
}
