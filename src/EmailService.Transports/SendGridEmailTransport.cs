using EmailService.Core;
using EmailService.Core.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EmailService.Transports
{
    /// <summary>
    /// Lightweight class which sends emails via the SendGrid HTTP API.
    /// </summary>
    public class SendGridEmailTransport : IEmailTransport, IDisposable
    {
        private const string EndpointUrl = "https://api.sendgrid.com";
        
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
            // ensure that everything is correctly configured
            CheckConfig();

            /* The SendGrid API takes a form-encoded set of data; .NET handles this using a
             * list of KeyValue pairs which are translated into Key=Value&Key=Value.
             * The reason for NOT using a dictionary is that HTTP allows duplicate keys, and
             * SendGrid uses this for multiple To/CC addresses - a dictionary would break
             * on duplicate values whereas a list of KVPs has no problem.
             */
            var data = new List<KeyValuePair<string, string>>();
            data.Add(new KeyValuePair<string, string>("from", args.SenderAddress ?? _options.SenderAddress));
            data.Add(new KeyValuePair<string, string>("fromname", args.SenderName ?? _options.SenderName));
            data.Add(new KeyValuePair<string, string>("subject", args.Subject));
            data.Add(new KeyValuePair<string, string>("html", args.Body));
            
            // sendgrid allows multiple addresses by putting a [] after the to/cc args
            foreach (var to in args.To)
            {
                data.Add(new KeyValuePair<string, string>("to[]", to));
            }

            foreach (var cc in args.CC)
            {
                data.Add(new KeyValuePair<string, string>("cc[]", cc));
            }

            foreach (var bcc in args.Bcc)
            {
                data.Add(new KeyValuePair<string, string>("bcc[]", bcc));
            }

            if (!string.IsNullOrWhiteSpace(args.SenderAddress))
            {
                data.Add(new KeyValuePair<string, string>("replyto", args.SenderAddress));
            }

            // SendGrid allows us to define a template for messages to provide a common
            // header/footer; if provided, use it
            if (!string.IsNullOrWhiteSpace(_options.Template))
            {
                string smtpApiJson = GetSmtpApiJson();
                data.Add(new KeyValuePair<string, string>("x-smtpapi", smtpApiJson));
            }

            // build the HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/mail.send.json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Content = new FormUrlEncodedContent(data);
            var response = await _httpClient.SendAsync(request);

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

        private string GetSmtpApiJson()
        {
            // see https://sendgrid.com/docs/API_Reference/Web_API_v3/Transactional_Templates/smtpapi.html
            return JsonConvert.SerializeObject(new
            {
                filters = new
                {
                    templates = new
                    {
                        settings = new
                        {
                            enable = 1,
                            template_id = _options.Template
                        }
                    }
                }
            });
        }
    }
}
