namespace EmailService.Core
{
    public enum ProcessingStatus
    {
        None = 0,
        Pending = 1,
        FailedRequeued = 2,
        FailedAbandoned = 3,
        Succeeded = 4
    }
}
