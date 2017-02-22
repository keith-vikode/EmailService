using EmailService.Core;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace EmailService.Storage.Azure
{
    public class TableEmailAuditLogEntry : TableEntity, ISentEmailInfo
    {
        public TableEmailAuditLogEntry()
        {
        }

        public TableEmailAuditLogEntry(Guid applicationId, DateTime processedTime)
        {
            PartitionKey = applicationId.ToString();
            RowKey = $"{processedTime.Ticks}-{Guid.NewGuid():N}";
            ProcessedTime = processedTime;
        }

        public Guid ApplicationId => Guid.Parse(PartitionKey);

        public DateTime ProcessedTime { get; set; }

        public string ApplicationName { get; set; }

        public int DequeueCount { get; set; }

        public EmailContentLogLevel LogLevel { get; set; }

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
