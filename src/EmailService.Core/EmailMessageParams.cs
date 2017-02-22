using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EmailService.Core
{
    public class EmailMessageParams
    {
        public Guid ApplicationId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
        public IList<string> To { get; set; } = new List<string>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> Bcc { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> CC { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> ReplyTo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SenderAddress { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SenderName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Subject { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BodyEncoded { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? TemplateId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Culture { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EmailContentLogLevel LogLevel { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public static string EncodeBody(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        public static string DecodeBody(string encoded)
        {
            if (string.IsNullOrWhiteSpace(encoded))
            {
                return string.Empty;
            }

            var bytes = Convert.FromBase64String(encoded);
            return Encoding.UTF8.GetString(bytes);
        }

        public static EmailMessageParams FromJson(string json)
        {
            return JsonConvert.DeserializeObject<EmailMessageParams>(json);
        }

        public static string ToJson(EmailMessageParams message, Formatting formatting = Formatting.Indented)
        {
            return JsonConvert.SerializeObject(message, formatting);
        }

        public string GetBody()
        {
            return DecodeBody(BodyEncoded);
        }

        public CultureInfo GetCulture()
        {
            if (string.IsNullOrEmpty(Culture))
            {
                return CultureInfo.InvariantCulture;
            }

            return new CultureInfo(Culture);
        }
    }
}
