using System;

namespace EmailService.Core
{
    public interface ISentEmailInfo
    {
        Guid ApplicationId { get; }

        DateTime ProcessedTime { get; }

        string ApplicationName { get; }

        int DequeueCount { get; }

        EmailContentLogLevel LogLevel { get; }

        DateTime ReceivedTime { get; }

        string RecipientAddress { get; }

        string RecipientType { get; }

        Guid RequestId { get; }

        string Subject { get; }

        Guid? TemplateId { get; }

        string TemplateName { get; }

        Guid TransportId { get; }

        string TransportName { get; }

        string TransportType { get; }
    }
}
