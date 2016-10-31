using EmailService.Core;
using EmailService.Core.Abstraction;
using EmailService.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Storage.Azure
{
    public class StorageEmailLog : IEmailLogWriter, IEmailLogReader
    {
        private readonly CloudStorageAccount _account;
        private readonly Lazy<CloudTable> _sentMessagesTable;
        private readonly Lazy<CloudTable> _processLogTable;
        private readonly ILogger _logger;
        private bool _initialized;

        public StorageEmailLog(IOptions<AzureStorageOptions> options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StorageEmailLog>();

            // let this throw an exception if it fails, we'll get better information
            // from the core class than wrapping it in our own error
            _account = CloudStorageAccount.Parse(options.Value.ConnectionString);

            _sentMessagesTable = new Lazy<CloudTable>(() =>
            {
                var client = _account.CreateCloudTableClient();
                return client.GetTableReference(options.Value.AuditTableName);
            }, true);
            
            _processLogTable = new Lazy<CloudTable>(() =>
            {
                var client = _account.CreateCloudTableClient();
                return client.GetTableReference(options.Value.ProcessorLogTableName);
            }, true);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await _sentMessagesTable.Value.CreateIfNotExistsAsync(null, null, cancellationToken);
            await _processLogTable.Value.CreateIfNotExistsAsync(null, null, cancellationToken);
            _initialized = true;
        }
        
        public async Task<bool> TryLogSentMessageAsync(
            EmailQueueToken token,
            SentEmailInfo info,
            CancellationToken cancellationToken)
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

                await _sentMessagesTable.Value.ExecuteBatchAsync(batch);

                success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error logging email recipients for email {0}:\n{1}", token, ex);
            }

            return success;
        }
        
        public async Task<bool> TryLogProcessAttemptAsync(
            EmailQueueToken token,
            int retryCount,
            ProcessingStatus status,
            DateTime startUtc,
            DateTime endUtc,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            var success = false;

            try
            {
                if (!_initialized)
                {
                    await InitializeAsync(cancellationToken);
                }

                var entry = new TableProcessorLogEntry(token, retryCount, status, startUtc, endUtc, errorMessage);
                var op = TableOperation.Insert(entry);
                var result = await _processLogTable.Value.ExecuteAsync(op);
                success = result.HttpStatusCode < 300 && result.HttpStatusCode >= 200;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error writing processor log entry for token {0}:\n{1}", token, ex);
            }

            return success;
        }

        public async Task<IEnumerable<ISentEmailInfo>> GetSentMessagesAsync(
            Guid applicationId,
            DateTime rangeStart,
            DateTime rangeEnd)
        {
            var query = new TableQuery<TableEmailAuditLogEntry>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, applicationId.ToString()),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, rangeStart.Ticks.ToString()),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, rangeEnd.Ticks.ToString())
                        )));
            
            var results = new List<TableEmailAuditLogEntry>();

            var t = new TableContinuationToken();
            while (t != null)
            {
                var segment = await _sentMessagesTable.Value.ExecuteQuerySegmentedAsync(query, t);
                t = segment.ContinuationToken;

                results.AddRange(segment);
            }

            return results;

        }

        public async Task<IEnumerable<IProcessorLogEntry>> GetProcessingLogsAsync(EmailQueueToken token)
        {
            var query = new TableQuery<TableProcessorLogEntry>()
                .Where(TableQuery
                    .GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, token.RequestId.ToString()));

            var results = new List<TableProcessorLogEntry>();

            // we shouldn't ever have more than half a dozen entries
            // in any one partition, but just to be on the safe side,
            // we'll allow for segmented result sets
            var t = new TableContinuationToken();
            while (t != null)
            {
                var segment = await _processLogTable.Value.ExecuteQuerySegmentedAsync(query, t);
                t = segment.ContinuationToken;

                results.AddRange(segment);
            }

            return results;
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
    
    public class TableProcessorLogEntry : TableEntity, IProcessorLogEntry
    {
        public TableProcessorLogEntry()
        {
        }

        public TableProcessorLogEntry(
            EmailQueueToken token,
            int retryCount,
            ProcessingStatus status,
            DateTime startUtc,
            DateTime endUtc,
            string errorMessage)
            : base(token.RequestId.ToString(), retryCount.ToString())
        {
            ErrorMessage = errorMessage;
            ProcessStartedUtc = startUtc;
            ProcessFinishedUtc = endUtc;
            Status = status.ToString();
            RetryCount = retryCount;
        }

        public string ErrorMessage { get; set; }

        public DateTime ProcessFinishedUtc { get; set; }

        public DateTime ProcessStartedUtc { get; set; }

        public int RetryCount { get; set; }
        
        public string Status { get; set; }

        ProcessingStatus IProcessorLogEntry.Status
        {
            get
            {
                ProcessingStatus status;
                return Enum.TryParse<ProcessingStatus>(Status, out status) ? status : ProcessingStatus.None;
            }
        }
    }
}
