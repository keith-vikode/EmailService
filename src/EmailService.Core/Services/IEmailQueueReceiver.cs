using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    public interface IEmailQueueReceiver<TMessage> where TMessage : IEmailQueueMessage
    {
        Task<IEnumerable<TMessage>> ReceiveAsync(CancellationToken cancellationToken);

        Task CompleteAsync(TMessage message, CancellationToken cancellationToken);
    }
}
