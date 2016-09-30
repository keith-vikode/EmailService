using System;
using System.Net.Http;

namespace EmailService.Client
{
    /// <summary>
    /// A non-templated email where you specify the subject and body text,
    /// optionally including a data template.
    /// </summary>
    public class BasicEmail : EmailParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicEmail"/> class.
        /// </summary>
        /// <param name="body">Body text (may include {{placeholders}} if desired)</param>
        /// <param name="subject">Subject text (may include {{placeholders}} if desired)</param>
        /// <param name="data">Data to use within the body/subject</param>
        public BasicEmail(string body, string subject = null, object data = null)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentNullException(nameof(body), "Body text cannot be empty for basic email requests");
            }

            Body = body;
            Subject = subject;
            Data = data;
        }

        /// <summary>
        /// Gets the subject text (can include placeholders).
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Gets the body text (can include placeholders).
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Gets the data to use when transforming this email.
        /// </summary>
        public object Data { get; }

        internal override HttpContent ToContent()
        {
            var values = ToValueDictionary();
            values.Add(nameof(Subject), Subject);
            values.Add(nameof(Body), Body);
            return new FormUrlEncodedContent(values);
        }
    }
}
