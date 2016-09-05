using EmailService.Core.Abstraction;
using System;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    public interface IEmailAuditLog<T> where T : IEmailRecipientLogEntry
    {
        Task LogEmailSentAsync(T entry);
    }
}
