using System;
using System.Threading.Tasks;

namespace EmailService.Core.Templating
{
    /// <summary>
    /// Defines the service for transforming email templates.
    /// </summary>
    /// <remarks>
    /// We only currently have one implementation of <see cref="ITemplateTransformer"/>, but it's
    /// left open for unit testing and for future extension.
    /// </remarks>
    public interface ITemplateTransformer
    {
        /// <summary>
        /// Transforms an email template using the given data.
        /// </summary>
        /// <param name="template">Template to transform</param>
        /// <param name="data">Data to use as a model</param>
        /// <param name="formatter">The format provider (e.g. culture).</param>
        /// <returns>The transformed email template</returns>
        Task<EmailTemplate> TransformTemplateAsync(EmailTemplate template, object data, IFormatProvider formatter);

        /// <summary>
        /// Transforms text using the given data.
        /// </summary>
        /// <param name="template">Text to transform</param>
        /// <param name="data">Data to use as a model</param>
        /// <param name="formatter">The format provider (e.g. culture).</param>
        /// <returns>The transformed text</returns>
        Task<string> TransformTextAsync(string template, object data, IFormatProvider formatter);
    }
}
