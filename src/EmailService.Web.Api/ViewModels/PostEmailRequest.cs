using EmailService.Core;
using EmailService.Web.Api.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EmailService.Web.Api.ViewModels
{
    /// <summary>
    /// Parameters required to send an email.
    /// </summary>
    public class PostEmailRequest : IValidatableObject
    {
        /// <summary>
        /// Message recipient email addresses.
        /// </summary>
        [Required]
        [ModelBinder(BinderType = typeof(CommaSeparatedModelBinder))]
        public IList<string> To { get; set; } = new List<string>();

        /// <summary>
        /// Message CC email addresses.
        /// </summary>
        [ModelBinder(BinderType = typeof(CommaSeparatedModelBinder))]
        public IList<string> CC { get; set; } = new List<string>();

        /// <summary>
        /// Message BCC email addresses.
        /// </summary>
        [ModelBinder(BinderType = typeof(CommaSeparatedModelBinder))]
        public IList<string> Bcc { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the email address of person or thing sending this
        /// email. If not provided, a default will be used.
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Gets or sets the name of person or thing sending this email. If
        /// not provided, a default will be used.
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the template ID to use for this email.
        /// </summary>
        public Guid? Template { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the culture to use for sending this email.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// JSON-encoded data for binding to the template.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the desired log level for this message.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EmailContentLogLevel LogLevel { get; set; } = EmailContentLogLevel.All;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!(To?.Any() ?? false))
            {
                yield return new ValidationResult("At least one recipient must be supplied", new string[] { nameof(To) });
            }

            IEnumerable<string> invalid;
            if (!ValidationUtils.AreAllValidEmailAddresses(To, out invalid))
            {
                yield return new ValidationResult("Not all 'To' email addresses are valid: " + string.Join(", ", invalid), new string[] { nameof(To) });
            }
            if (!ValidationUtils.AreAllValidEmailAddresses(CC, out invalid))
            {
                yield return new ValidationResult("Not all 'CC' email addresses are valid: " + string.Join(", ", invalid), new string[] { nameof(To) });
            }
            if (!ValidationUtils.AreAllValidEmailAddresses(To, out invalid))
            {
                yield return new ValidationResult("Not all 'Bcc' email addresses are valid: " + string.Join(", ", invalid), new string[] { nameof(To) });
            }

            if (!Template.HasValue && string.IsNullOrWhiteSpace(Body))
            {
                yield return new ValidationResult("If no template was supplied, then body text is required", new string[] { nameof(Body) });
            }
        }
    }
}
