using EmailService.Core;
using EmailService.Core.Abstraction;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace EmailService.Storage.Azure
{
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
