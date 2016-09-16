using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Provides methods to write to the email processing logs.
    /// </summary>
    public interface IEmailLogWriter
    {
        Task LogSuccessAsync(
            EmailQueueToken token,
            SentEmailInfo info,
            CancellationToken cancellationToken);
    }
}
