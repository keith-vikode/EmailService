using EmailService.Core;
using System;

namespace EmailService.Web.Api.Test.Stubs
{
    public class BasicSentEmailInfo : ISentEmailInfo
    {
        public Guid ApplicationId { get; set; }

        public string ApplicationName { get; set; }

        public int DequeueCount { get; set; }

        public EmailContentLogLevel LogLevel { get; set; }

        public DateTime ProcessedTime { get; set; }

        public DateTime ReceivedTime { get; set; }

        public string RecipientAddress { get; set; }

        public string RecipientType { get; set; }

        public Guid RequestId { get; set; }

        public string Subject { get; set; }

        public Guid? TemplateId { get; set; }

        public string TemplateName { get; set; }

        public Guid TransportId { get; set; }

        public string TransportName { get; set; }

        public string TransportType { get; set; }
    }
}
