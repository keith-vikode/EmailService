using EmailService.Core;
using EmailService.Core.Abstraction;

namespace EmailService.Storage.Azure
{
    public class AzureEmailQueueMessage : IEmailQueueMessage
    {
        public EmailQueueToken Token { get; set; }

        public string MessageId { get; set; }

        public string PopReceipt { get; set; }

        public int DequeueCount { get; set; }
    }
}
