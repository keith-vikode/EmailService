using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Sends requests for emails to the queue.
    /// </summary>
    public interface IEmailQueueSender
    {
        Task SendAsync(EmailQueueToken token, CancellationToken cancellationToken);
    }
}
