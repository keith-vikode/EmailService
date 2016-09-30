using EmailService.Core;
using EmailService.Core.Abstraction;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.Transports
{
    /// <summary>
    /// Lightweight class which sends emails via the SendGrid HTTP API.
    /// </summary>
    public class SendGridEmailTransport : IEmailTransport, IDisposable
    {
        private const string EndpointUrl = "https://api.sendgrid.com";
        private const string SendApi = "/v3/mail/send";

        private readonly SendGridOptions _options;
        private readonly HttpClient _httpClient;

        private bool _disposed;

        public SendGridEmailTransport(SendGridOptions options)
        {
            _options = options;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(EndpointUrl);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient.Dispose();
                _disposed = true;
            }
        }

        public async Task<bool> SendAsync(SenderParams args)
        {
            const string Bearer = nameof(Bearer);
            const string TextHtml = "text/html";
            const string ApplicationJson = "application/json";

            // ensure that everything is correctly configured
            CheckConfig();

            var json = JsonConvert.SerializeObject(new
            {
                personalizations = new object[]
                {
                    new
                    {
                        to = args.To?.Select(e => new { email = e }),
                        cc = args.CC?.Select(e => new { email = e }),
                        bcc = args.Bcc?.Select(e => new { email = e }),
                        subject = args.Subject
                    }
                },
                from = new
                {
                    email = args.SenderAddress ?? _options.SenderAddress,
                    name = args.SenderName ?? _options.SenderName
                },
                content = new object[]
                {
                    new
                    {
                        type = TextHtml,
                        value = args.Body
                    }
                }
            }, Formatting.Indented);

            // build the HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, SendApi);
            request.Headers.Authorization = new AuthenticationHeaderValue(Bearer, _options.ApiKey);
            request.Content = new StringContent(json, Encoding.UTF8, ApplicationJson);

            // get the response
            var response = await _httpClient.SendAsync(request);

            // check the response
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
            }

            return response.IsSuccessStatusCode;
        }

        private void CheckConfig()
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new Exception("Invalid email config: 'ApiKey' is missing or empty");
            }

            if (string.IsNullOrWhiteSpace(_options.SenderAddress))
            {
                throw new Exception("Invalid email config: 'Sender' is missing or empty");
            }
        }
    }
}
