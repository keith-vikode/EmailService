using EmailService.Core.Abstraction;
using EmailService.Core.Services;
using EmailService.Core.Templating;
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
        private readonly IEmailQueueBlobStore _blobStore;
        private readonly IEmailTemplateStore _templateStore;
        private readonly IEmailTransportFactory _transportFactory;
        private readonly IEmailLogWriter _logWriter;
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
            IEmailLogWriter logWriter,
            ILoggerFactory loggerFactory)
        {
            _receiver = receiver;
            _blobStore = blobStore;
            _templateStore = templateStore;
            _transportFactory = transportFactory;
            _logWriter = logWriter;
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
            var batchSize = Math.Min(_receiver.MaxMessagesToRetrieve, Environment.ProcessorCount);

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogTrace("Checking for messages...");

                var messages = await _receiver.ReceiveAsync(batchSize, cancellationToken);
                if (messages.Any())
                {
                    _logger.LogInformation("Received {0} message(s)", messages.Count());

                    messages.AsParallel().ForAll(message =>
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            ProcessMessage(message, cancellationToken).Wait();
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
            _logger.LogTrace("Processing message {0} on try {1}", message.Token, message.DequeueCount);

            // load the args from the blob store; it's unlikely that they won't be found,
            // but to be safe we'll check for null (the args are in the blob store because
            // there's a chance that they'll exceed the 64KB limit on queue messages due
            // to including data and HTML)
            var args = await _blobStore.GetAsync(message.Token, cancellationToken);
            if (args != null)
            {
                try
                {
                    // do the actual sending and return information about the email that
                    // was sent so that we can log it; any failures will result in an
                    // exception that will be caught and handled in the catch block
                    var result = await TrySendEmailAsync(args);
                    result.DequeueCount = message.DequeueCount;

                    // after successful processing, the most important thing to do is
                    // to immediately remove this message from the queue; this will prevent
                    // retrieves if any of the subsequent calls fail (avoiding duplicate
                    // messages is more important than correct logging)
                    await _receiver.CompleteAsync(message, cancellationToken);
                    await _blobStore.RemoveAsync(message.Token, cancellationToken);

                    // now we can audit the event
                    await _logWriter.LogSuccessAsync(message.Token, result, cancellationToken);
                    _logger.LogInformation("Successfully sent email for message {0} on try {1}", message.Token, message.DequeueCount);
                }
                catch (Exception ex)
                {
                    if (message.DequeueCount >= MaxDequeue)
                    {
                        // failed, and reached the maximum retry count; move the message to
                        // the poison queue and move the blob data to the bin of failure
                        await _receiver.MoveToPoisonQueueAsync(message, cancellationToken);
                        await _blobStore.MoveToPoisonStoreAsync(message.Token, cancellationToken);
                        _logger.LogError("Failed to send email for message {0} after {1} tries, giving up\n{2}", message.Token, message.DequeueCount, ex);
                    }
                    else
                    {
                        // failed, but we'll let it retry to see if that fixes it
                        _logger.LogWarning("Failed to send email for message {0} after {1} tries\n{2}", message.Token, message.DequeueCount, ex);
                    }
                }
            }
            else
            {
                // if we couldn't find a matching blob, then raise an error and remove
                // the message from the queue
                _logger.LogError("Could not find message params for {0}", message.Token);
                await _receiver.CompleteAsync(message, cancellationToken);
            }
        }

        public async Task<SentEmailInfo> TrySendEmailAsync(EmailMessageParams args)
        {
            EmailTemplate email;

            var templateInfo = await _templateStore.GetTemplateAsync(args);
            if (templateInfo.ApplicationName == null)
            {
                throw new InvalidOperationException($"The application ID `{args.ApplicationId}` does not match any records in the database");
            }

            if (templateInfo.Template == null)
            {
                if (args.TemplateId.HasValue)
                {
                    throw new InvalidOperationException($"Could not find a template matching {args.TemplateId}");
                }
                else
                {
                    throw new InvalidOperationException("No subject and body were supplied, and no template ID was provided");
                }
            }

            // note that Data is a dictionary; if it has no values, we can assume that it's empty
            // and thus skip the transformation
            if (args.Data?.Count > 0)
            {
                email = await Transformer.TransformTemplateAsync(templateInfo.Template, args.Data, args.GetCulture());
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

            // we can pre-fill these log fields
            var log = new SentEmailInfo
            {
                ApplicationName = templateInfo.ApplicationName,
                TemplateName = templateInfo.Template.Name,
                TemplateId = args.TemplateId,
                Subject = senderParams.Subject,
                LogLevel = args.LogLevel,
                Recipients = senderParams.GetRecipients()
            };

            bool success = false;
            while (!success && templateInfo.TransportQueue.Any())
            {
                var transportInfo = templateInfo.TransportQueue.Dequeue();
                var transport = _transportFactory.CreateTransport(transportInfo);

                if (await transport.SendAsync(senderParams))
                {
                    // we can fill this information in now that we know it
                    log.ProcessedUtc = DateTime.UtcNow;
                    log.Transport = transportInfo;
                    success = true;
                }
            }

            if (!success)
            {
                throw new Exception("Could not send email with any of the configured transports");
            }

            return log;
        }
    }
}
