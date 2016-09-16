using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    public interface IEmailTemplateStore
    {
        Task<EmailTemplateInfo> GetTemplateAsync(EmailMessageParams args);
    }
}
