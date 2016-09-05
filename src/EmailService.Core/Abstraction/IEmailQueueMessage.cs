namespace EmailService.Core.Abstraction
{
    public interface IEmailQueueMessage
    {
        EmailQueueToken Token { get; }

        int DequeueCount { get; }
    }
}
