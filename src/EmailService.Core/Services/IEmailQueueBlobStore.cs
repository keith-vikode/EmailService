using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Storage service for email messages (that may be too large for a queue).
    /// </summary>
    public interface IEmailQueueBlobStore
    {
        Task AddAsync(EmailQueueToken token, EmailMessageParams message, CancellationToken cancellationToken);

        Task<EmailMessageParams> GetAsync(EmailQueueToken token, CancellationToken cancellationToken);

        Task RemoveAsync(EmailQueueToken token, CancellationToken cancellationToken);

        Task MoveToPoisonStoreAsync(EmailQueueToken token, CancellationToken cancellationToken);
    }
}
