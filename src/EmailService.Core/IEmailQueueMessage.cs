using System;

namespace EmailService.Core
{
    public interface IEmailQueueMessage
    {
        Guid Token { get; }

        int DequeueCount { get; }
    }
}
