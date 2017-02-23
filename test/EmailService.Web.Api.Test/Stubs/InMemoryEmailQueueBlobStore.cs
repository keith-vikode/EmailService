using EmailService.Core;
using EmailService.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Web.Api.Test.Stubs
{
    public class InMemoryEmailQueueBlobStore : IEmailQueueBlobStore
    {
        private static readonly Lazy<Dictionary<EmailQueueToken, EmailMessageParams>> _Blobs = new Lazy<Dictionary<EmailQueueToken, EmailMessageParams>>(() => new Dictionary<EmailQueueToken, EmailMessageParams>(), false);
        private static readonly Lazy<Dictionary<EmailQueueToken, EmailMessageParams>> _Poison = new Lazy<Dictionary<EmailQueueToken, EmailMessageParams>>(() => new Dictionary<EmailQueueToken, EmailMessageParams>(), false);

        public static Dictionary<EmailQueueToken, EmailMessageParams> Blobs => _Blobs.Value;

        public static Dictionary<EmailQueueToken, EmailMessageParams> Poison => _Poison.Value;

        public Task AddAsync(EmailQueueToken token, EmailMessageParams message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Blobs[token] = message;
            return Task.FromResult(0);
        }

        public Task<EmailMessageParams> GetAsync(EmailQueueToken token, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EmailMessageParams message;
            if (!Blobs.TryGetValue(token, out message))
                message = null;
            return Task.FromResult(message);
        }

        public Task MoveToPoisonStoreAsync(EmailQueueToken token, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EmailMessageParams message;
            if (Blobs.TryGetValue(token, out message))
            {
                Blobs.Remove(token);
                Poison[token] = message;
            }

            return Task.FromResult(0);
        }

        public Task RemoveAsync(EmailQueueToken token, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Blobs.Remove(token);
            return Task.FromResult(0);
        }
    }
}