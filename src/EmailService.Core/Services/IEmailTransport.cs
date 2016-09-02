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
        /// <returns>True if the message was sent successfully.</returns>
        Task<bool> SendAsync(SenderParams args);
    }
}
