using EmailService.Core;
using EmailService.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Storage.Azure
{
    public class StorageEmailLog : IEmailLogWriter
    {
        private readonly CloudStorageAccount _account;
        private readonly Lazy<CloudTable> _table;
        private readonly ILogger _logger;
        private bool _initialized;

        public StorageEmailLog(IOptions<AzureStorageOptions> options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StorageEmailLog>();

            // let this throw an exception if it fails, we'll get better information
            // from the core class than wrapping it in our own error
            _account = CloudStorageAccount.Parse(options.Value.ConnectionString);

            _table = new Lazy<CloudTable>(() =>
            {
                var client = _account.CreateCloudTableClient();
                return client.GetTableReference(options.Value.AuditTableName);
            }, true);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await _table.Value.CreateIfNotExistsAsync(null, null, cancellationToken);
            _initialized = true;
        }
        
        public async Task<bool> TryLogSuccessAsync(EmailQueueToken token, SentEmailInfo info, CancellationToken cancellationToken)
        {
            var success = false;

            try
            {
                if (!_initialized)
                {
                    await InitializeAsync(cancellationToken);
                }

                var batch = new TableBatchOperation();
                foreach (var entry in BuildEntries(token, info))
                {
                    batch.Insert(entry);
                }

                await _table.Value.ExecuteBatchAsync(batch);

                success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error logging email recipients for email {0}:\n{1}", token, ex);
            }

            return success;
        }

        private IEnumerable<TableEmailAuditLogEntry> BuildEntries(EmailQueueToken token, SentEmailInfo info)
        {
            foreach (var recipient in info.Recipients)
            {
                yield return new TableEmailAuditLogEntry(token.ApplicationId, info.ProcessedUtc)
                {
                    ApplicationName = info.ApplicationName,
                    DequeueCount = info.DequeueCount,
                    LogLevel = info.LogLevel,
                    ReceivedTime = token.TimeStamp,
                    RequestId = token.RequestId,
                    TemplateId = info.TemplateId,
                    TemplateName = info.TemplateName,
                    TransportId = info.Transport.Id,
                    TransportName = info.Transport.Name,
                    TransportType = info.Transport.Type.ToString(),
                    Subject = info.Subject,
                    RecipientAddress = recipient.Address,
                    RecipientType = recipient.Type.ToString()
                };
            }
        }
    }

    public class TableEmailAuditLogEntry : TableEntity
    {
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

        //public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        //{
        //    var d = base.WriteEntity(operationContext);
        //    d.Add(nameof(TransportType), new EntityProperty(TransportType.ToString()));
        //    d.Add(nameof(RecipientType), new EntityProperty(RecipientType.ToString()));
        //    return d;
        //}

        //public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        //{
        //    base.ReadEntity(properties, operationContext);
        //    TransportType tt;
        //    RecipientType rt;

        //    if (Enum.TryParse(properties[nameof(TransportType)].StringValue, out tt))
        //    {
        //        TransportType = tt;
        //    }

        //    if (Enum.TryParse(properties[nameof(RecipientType)].StringValue, out rt))
        //    {
        //        RecipientType = rt;
        //    }
        //}
    }
}
