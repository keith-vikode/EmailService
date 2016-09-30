using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace EmailService.Client
{
    /// <summary>
    /// A request to send an email using a pre-defined template.
    /// </summary>
    /// <remarks>
    /// Templates are defined in the email service admin UI, and given a GUID. This GUID can
    /// be used to send copies of this template using arbitrary data that can be mapped into
    /// the template's markup.
    /// </remarks>
    public class TemplatedEmail : EmailParameters
    {
        public TemplatedEmail(Guid templateId, object data)
        {
            TemplateId = templateId;
            Data = data;
        }

        /// <summary>
        /// Gets the template ID.
        /// </summary>
        public Guid TemplateId { get; }

        /// <summary>
        /// Gets the template data to use.
        /// </summary>
        public object Data { get; }

        internal override HttpContent ToContent()
        {
            var values = ToValueDictionary();
            values.Add(nameof(TemplateId), TemplateId.ToString());
            if (Data != null)
            {
                values.Add(nameof(Data), JsonConvert.SerializeObject(Data));
            }

            return new FormUrlEncodedContent(values);
        }
    }
}
