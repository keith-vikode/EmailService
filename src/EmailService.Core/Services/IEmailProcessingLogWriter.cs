using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Provides methods to write to the email processing logs.
    /// </summary>
    public interface IEmailProcessingLogWriter
    {
        Task AddToPendingLogAsync(object message);

        Task RemoveFromPendingLogAsync(object token);

        Task AddToSuccessLogAsync(object message);

        Task AddToFailureLogAsync(object message);
    }
}
