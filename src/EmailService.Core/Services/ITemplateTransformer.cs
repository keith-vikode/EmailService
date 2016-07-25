using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Defines the service for transforming email templates.
    /// </summary>
    public interface ITemplateTransformer
    {
        /// <summary>
        /// Transforms an email template using the given data.
        /// </summary>
        /// <param name="template">Template to transform</param>
        /// <param name="data">Data to use as a model</param>
        /// <returns>The transformed email template</returns>
        Task<EmailTemplate> TransformTemplateAsync(EmailTemplate template, object data);
    }
}
