namespace EmailService.Client
{
    /// <summary>
    /// Enumerates the status codes for email requests.
    /// </summary>
    public enum RequestStatus
    {
        InvalidToken = 0,
        Pending = 1,
        FailedPendingRetry = 2,
        FailedPermanently = 3,
        Succeeded = 4
    }
}
