using System.Globalization;
using System.Threading.Tasks;

namespace EmailService.Core.Services
{
    /// <summary>
    /// Describes an interface for loading email templates.
    /// </summary>
    public interface ITemplateStore
    {
        /// <summary>
        /// Loads an email template by name.
        /// </summary>
        /// <param name="name">Name of the template to load</param>
        /// <param name="culture">Culture/language to load. Note that this will fall back to
        /// a more generic or default culture if no matching culture exists.</param>
        /// <returns>The template object (un-transformed)</returns>
        Task<EmailTemplate> LoadTemplateAsync(string name, CultureInfo culture);
    }
}
