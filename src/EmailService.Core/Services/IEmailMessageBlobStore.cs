using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Storage service for email messages (that may be too large for a queue).
    /// </summary>
    public interface IEmailMessageBlobStore
    {
        Task AddAsync(Guid token, EmailMessageParams message, CancellationToken cancellationToken);

        Task<EmailMessageParams> GetAsync(Guid token, CancellationToken cancellationToken);

        Task RemoveAsync(Guid token, CancellationToken cancellationToken);

        Task ArchiveAsync(Guid token, CancellationToken cancellationToken);
    }
}
