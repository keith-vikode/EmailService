using System;

namespace EmailService.Core.Abstraction
{
    public interface IEmailRecipientLogEntry
    {
        Guid ApplicationId { get; }

        Guid RequestId { get; }

        Guid ReceivedTime { get; }

        Guid ProcessedTime { get; }

        Guid? TemplateId { get; }

        Guid TransportId { get; }

        string ApplicationName { get; }

        string TemplateName { get; }

        string TransportType { get; }

        string TransportName { get; }

        int DequeueCount { get; }

        EmailContentLogLevel LogLevel { get; }

        string Recipient { get; }

        string RecipientType { get; }

        string Subject { get; }

        string ErrorMessage { get; }
    }
}
