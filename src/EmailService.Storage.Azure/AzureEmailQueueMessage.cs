using EmailService.Core;
using System;

namespace EmailService.Storage.Azure
{
    public class AzureEmailQueueMessage : IEmailQueueMessage
    {
        public Guid Token { get; set; }

        public string MessageId { get; set; }

        public string PopReceipt { get; set; }

        public int DequeueCount { get; set; }
    }
}
