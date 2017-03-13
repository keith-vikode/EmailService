using EmailService.Core.Abstraction;
using System.Collections.Generic;

namespace EmailService.Core
{
    public partial class QueueProcessor<TMessage>
        where TMessage : IEmailQueueMessage
    {
        private class EmailTemplateInfo
        {
            public string ApplicationName { get; set; }

            public string SenderName { get; set; }

            public string SenderAddress { get; set; }

            public EmailTemplate Template { get; set; }

            public Queue<ITransportDefinition> TransportQueue { get; set; }
        }
    }
}
