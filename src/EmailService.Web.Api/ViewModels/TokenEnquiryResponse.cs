using EmailService.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace EmailService.Web.Api.ViewModels
{
    public class TokenEnquiryResponse
    {
        public DateTime Submitted { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public DateTime? LastProcessed { get; set; }

        public int RetryCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string ErrorMessage { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;
    }
}
