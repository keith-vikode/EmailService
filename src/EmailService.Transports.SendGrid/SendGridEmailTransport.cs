using EmailService.Core;
using EmailService.Core.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EmailService.Transports.SendGrid
{
    /// <summary>
    /// Lightweight class which sends emails via the SendGrid HTTP API.
    /// </summary>
    public class SendGridEmailTransport : IEmailTransport, IDisposable
    {
        private const string EndpointUrl = "https://api.sendgrid.com";

        private readonly string _sender;
        private readonly string _senderName;
        private readonly string _redirectTo;
        private readonly string _apiKey;
        private readonly string _template;

        private readonly HttpClient _httpClient;

        private bool _disposed;

        public SendGridEmailTransport(IOptions<SendGridOptions> options)
        {
            _sender = options.Value.SenderAddress;
            _senderName = options.Value.SenderName;
            _redirectTo = options.Value.RedirectTo;
            _apiKey = options.Value.ApiKey;
            _template = options.Value.Template;

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

        public async Task SendAsync(SenderParams args)
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
            data.Add(new KeyValuePair<string, string>("from", args.SenderEmail ?? _sender));
            data.Add(new KeyValuePair<string, string>("fromname", args.SenderName ?? _senderName));
            data.Add(new KeyValuePair<string, string>("subject", args.Subject));
            data.Add(new KeyValuePair<string, string>("html", args.Body));

            // the 'redirectTo' property allows us to redirect all emails to a given address,
            // which is very useful for development or testing
            if (string.IsNullOrWhiteSpace(_redirectTo))
            {
                // sendgrid allows multiple addresses by putting a [] after the to/cc args
                foreach (var to in args.To)
                {
                    data.Add(new KeyValuePair<string, string>("to[]", to));
                }

                foreach (var cc in args.CC)
                {
                    data.Add(new KeyValuePair<string, string>("cc[]", cc));
                }
            }
            else
            {
                data.Add(new KeyValuePair<string, string>("to", _redirectTo));
            }

            if (!string.IsNullOrWhiteSpace(args.SenderEmail))
            {
                data.Add(new KeyValuePair<string, string>("replyto", args.SenderEmail));
            }

            if (!string.IsNullOrWhiteSpace(_template))
            {
                string smtpApiJson = GetSmtpApiJson();
                data.Add(new KeyValuePair<string, string>("x-smtpapi", smtpApiJson));
            }

            // build the HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/mail.send.json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new FormUrlEncodedContent(data);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
        }

        private void CheckConfig()
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new Exception("Invalid email config: 'ApiKey' is missing or empty");
            }

            if (string.IsNullOrWhiteSpace(_sender))
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
                            template_id = _template
                        }
                    }
                }
            });
        }
    }
}
