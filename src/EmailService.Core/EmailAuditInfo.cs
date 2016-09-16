using EmailService.Core.Abstraction;
using System;
using System.Collections.Generic;

namespace EmailService.Core
{
    public enum RecipientType
    {
        To,
        CC,
        Bcc
    }

    public class RecipientInfo
    {
        public RecipientInfo(string address, RecipientType type = RecipientType.To)
        {
            Address = address;
            Type = type;
        }

        public RecipientType Type { get; }
        public string Address { get; }
    }

    public class SentEmailInfo
    {
        public IEnumerable<RecipientInfo> Recipients { get; set; } = new List<RecipientInfo>();
        public DateTime ProcessedUtc { get; set; }
        public string ApplicationName { get; set; }
        public string TemplateName { get; set; }
        public Guid? TemplateId { get; set; }
        public ITransportDefinition Transport { get; set; }
        public int DequeueCount { get; set; }
        public EmailContentLogLevel LogLevel { get; set; }
        public string Subject { get; set; }
    }
}
