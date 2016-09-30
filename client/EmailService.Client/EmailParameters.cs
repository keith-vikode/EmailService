using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Http;

namespace EmailService.Client
{
    /// <summary>
    /// Base class for the parameters required to send an email.
    /// </summary>
    public abstract class EmailParameters : IValidatableObject
    {
        /// <summary>
        /// Gets or sets a list of recipient email addresses (required).
        /// </summary>
        public IList<string> To { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a list of carbon copy email addresses (optional).
        /// </summary>
        public IList<string> CC { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a list of blind carbon copy email addresses (optional).
        /// </summary>
        public IList<string> Bcc { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the display name of the sender (optional).
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the sender (optional).
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Gets or sets the culture to use when formatting the email.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        /// <summary>
        /// Creates an <see cref="HttpContent"/> object to send to the API.
        /// </summary>
        /// <returns>An <see cref="HttpContent"/> object.</returns>
        internal abstract HttpContent ToContent();

        /// <summary>
        /// Creates a dictionary of the base data in this request.
        /// </summary>
        /// <returns>A serialized dictionary.</returns>
        protected Dictionary<string, string> ToValueDictionary()
        {
            var join = new Func<IEnumerable<string>, string>(s => string.Join(",", s));

            var data = new Dictionary<string, string>();

            if (To?.Count > 0)
                data.Add(nameof(To), join(To));

            if (CC?.Count > 0)
                data.Add(nameof(CC), join(CC));

            if (Bcc?.Count > 0)
                data.Add(nameof(Bcc), join(Bcc));

            if (!string.IsNullOrEmpty(SenderName))
                data.Add(nameof(SenderName), SenderName);

            if (!string.IsNullOrEmpty(SenderAddress))
                data.Add(nameof(SenderAddress), SenderAddress);

            if (Culture != null)
                data.Add(nameof(Culture), Culture.Name);

            return data;
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}
