using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Provides methods to write to the email processing logs.
    /// </summary>
    public interface IEmailLogWriter
    {
        Task<bool> TryLogProcessAttemptAsync(
            EmailQueueToken token,
            int retryCount,
            ProcessingStatus status,
            DateTime startUtc,
            DateTime endUtc,
            string errorMessage,
            CancellationToken cancellationToken);

        Task<bool> TryLogSentMessageAsync(
            EmailQueueToken token,
            SentEmailInfo info,
            CancellationToken cancellationToken);
    }
}
