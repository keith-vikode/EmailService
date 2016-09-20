using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

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

    public class AnonymousObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var c = new ExpandoObjectConverter();
            return c.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var jo = JToken.FromObject(value);
                jo.WriteTo(writer, new JsonConverter[] { });
            }
            else
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
        }
    }
}
