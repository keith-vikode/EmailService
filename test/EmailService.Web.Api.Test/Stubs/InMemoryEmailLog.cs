using EmailService.Core;
using EmailService.Core.Abstraction;
using EmailService.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmailService.Web.Api.Test.Stubs
{
    public class InMemoryEmailLog : IEmailLogWriter, IEmailLogReader
    {
        private static readonly Lazy<List<BasicProcessorLogEntry>> _ProcessingLog = new Lazy<List<BasicProcessorLogEntry>>(() => new List<BasicProcessorLogEntry>(), false);

        private static readonly Lazy<List<BasicSentEmailInfo>> _SentLog = new Lazy<List<BasicSentEmailInfo>>(() => new List<BasicSentEmailInfo>(), false);

        public static List<BasicProcessorLogEntry> ProcessingLog => _ProcessingLog.Value;

        public static List<BasicSentEmailInfo> SentLog => _SentLog.Value;

        public Task<IEnumerable<IProcessorLogEntry>> GetProcessingLogsAsync(EmailQueueToken token)
        {
            var entries = ProcessingLog.Where(l => l.Token == token);
            return Task.FromResult(entries.AsEnumerable<IProcessorLogEntry>());
        }

        public Task<IEnumerable<ISentEmailInfo>> GetSentMessagesAsync(Guid applicationId, DateTime rangeStart, DateTime rangeEnd)
        {
            var entries = SentLog.Where(l => l.ApplicationId == applicationId && l.ProcessedTime >= rangeStart && l.ProcessedTime < rangeEnd);
            return Task.FromResult(entries.AsEnumerable<ISentEmailInfo>());
        }

        public Task<bool> TryLogProcessAttemptAsync(EmailQueueToken token, int retryCount, ProcessingStatus status, DateTime startUtc, DateTime endUtc, string errorMessage, CancellationToken cancellationToken)
        {
            ProcessingLog.Add(new BasicProcessorLogEntry
            {
                Token = token,
                RetryCount = retryCount,
                Status = status,
                ProcessStartedUtc = startUtc,
                ProcessFinishedUtc = endUtc,
                ErrorMessage = errorMessage
            });
            return Task.FromResult(true);
        }

        public Task<bool> TryLogSentMessageAsync(EmailQueueToken token, SentEmailInfo info, CancellationToken cancellationToken)
        {
            SentLog.AddRange(info.Recipients.Select(r => new BasicSentEmailInfo
            {
                ApplicationId = token.ApplicationId,
                RequestId = token.RequestId,
                ReceivedTime = token.TimeStamp,
                ApplicationName = info.ApplicationName,
                DequeueCount = info.DequeueCount,
                TemplateId = info.TemplateId,
                TemplateName = info.TemplateName,
                TransportId = info.Transport.Id,
                TransportType = info.Transport.Type.ToString(),
                TransportName = info.Transport.Name,
                LogLevel = info.LogLevel,
                ProcessedTime = info.ProcessedUtc,
                Subject = info.Subject,
                RecipientAddress = r.Address,
                RecipientType = r.Type.ToString()
            }));
            return Task.FromResult(true);
        }
    }
}