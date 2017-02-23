using EmailService.Core.Abstraction;
using System;
using EmailService.Core;

namespace EmailService.Web.Api.Test.Stubs
{
    public class BasicProcessorLogEntry : IProcessorLogEntry
    {
        public EmailQueueToken Token { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime ProcessFinishedUtc { get; set; }

        public DateTime ProcessStartedUtc { get; set; }

        public int RetryCount { get; set; }

        public ProcessingStatus Status { get; set; }
    }
}
