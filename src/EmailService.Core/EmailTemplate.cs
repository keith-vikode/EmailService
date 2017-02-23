using Newtonsoft.Json;
using System;
using System.Globalization;

namespace EmailService.Core
{
    /// <summary>
    /// Models an email/template.
    /// </summary>
    /// <remarks>
    /// We assume that the template is made of two strings - the subject and
    /// body. Both of these should be formatted in whatever templating language
    /// we are currently using (Razor, Mustache, JimMail, etc.). This object
    /// is also used to carry the transformed email text once it has passed
    /// through <see cref="ITemplateTransformer"/>.
    /// </remarks>
    public class EmailTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailTemplate"/> object.
        /// </summary>
        /// <param name="subject">Subject text</param>
        /// <param name="body">Body text</param>
        /// <param name="culture">Optional culture name</param>
        /// <param name="name">Name of the template, if any</param>
        public EmailTemplate(string subject, string body, CultureInfo culture = null, string name = null)
        {
            Subject = subject;
            Body = body;
            Culture = culture ?? CultureInfo.InvariantCulture;
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// Gets the subject text format.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Gets the body text format.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Gets the culture of this template.
        /// </summary>
        [JsonIgnore]
        public CultureInfo Culture { get; }

        public string Language => Culture?.Name;

        /// <summary>
        /// Gets the name of this template, if any.
        /// </summary>
        public string Name { get; }
    }
}
