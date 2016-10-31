using EmailService.Core.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Provides methods to query the email processing logs.
    /// </summary>
    public interface IEmailLogReader
    {
        Task<IEnumerable<IProcessorLogEntry>> GetProcessingLogsAsync(EmailQueueToken token);

        Task<IEnumerable<ISentEmailInfo>> GetSentMessagesAsync(Guid applicationId, DateTime rangeStart, DateTime rangeEnd);
    }
}
