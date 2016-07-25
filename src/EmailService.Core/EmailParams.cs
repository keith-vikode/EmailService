using System;
using System.Collections.Generic;

namespace EmailService.Core
{
    /// <summary>
    /// Parameters required to send an email.
    /// </summary>
    public class EmailParams
    {
        public EmailParams()
        {
        }

        public EmailParams(
            string to,
            string template,
            object data = null,
            string culture = null,
            string senderEmail = null,
            string senderName = null,
            string cc = null)
        {
            To = to?.Split(',');
            CC = cc?.Split(',');
            Template = template;
            Data = data;
        }

        public IList<string> To { get; set; } = new List<string>();

        public IList<string> CC { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the email address of person or thing sending this
        /// email. If not provided, a default will be used.
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Gets or sets the name of person or thing sending this email. If
        /// not provided, a default will be used.
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the template ID to use for this email.
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Gets or sets the culture to use for sending this email.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the model data for this email. This can either be
        /// a structured object, or an anonymous object acting as a data
        /// dictionary.
        /// </summary>
        public object Data { get; set; }
    }
}
