using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Describes an interface for sending emails.
    /// </summary>
    public interface IEmailTransport
    {
        /// <summary>
        /// Sends an email.
        /// </summary>
        /// <param name="args">Email data to send</param>
        /// <returns>An awaitable task.</returns>
        Task SendAsync(SenderParams args);
    }
}
