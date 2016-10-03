using System;

namespace EmailService.Core.Abstraction
{
    public interface IProcessorLogEntry
    {
        int RetryCount { get; }

        ProcessingStatus Status { get; }

        DateTime ProcessStartedUtc { get; }

        DateTime ProcessFinishedUtc { get; }

        string ErrorMessage { get; }
    }
}