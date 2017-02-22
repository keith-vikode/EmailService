using EmailService.Core.Abstraction;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Provides access to the email pending queue for receiving messages.
    /// </summary>
    /// <typeparam name="TMessage">Type of message to receive, where the message
    /// implements <see cref="IEmailQueueMessage"/> to provide the required payload.
    /// </typeparam>
    /// <remarks>The generic parameter is designed to be used for messages of the specific
    /// queue technology used, e.g. Azure Storage Queues or ServiceBus.</remarks>
    public interface IEmailQueueReceiver<TMessage>
        where TMessage : IEmailQueueMessage
    {
        int MaxMessagesToRetrieve { get; }

        Task<IEnumerable<TMessage>> ReceiveAsync(int number, CancellationToken cancellationToken);

        Task CompleteAsync(TMessage message, CancellationToken cancellationToken);

        Task MoveToPoisonQueueAsync(TMessage message, CancellationToken cancellationToken);
    }
}
