using EmailService.Core;
using EmailService.Core.Abstraction;
using System;

namespace EmailService.Web.Api.Test.Stubs
{
    public class BasicEmailQueueMessage : IEmailQueueMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();

        public EmailQueueToken Token { get; set; }

        public int DequeueCount { get; set; }
    }
}
