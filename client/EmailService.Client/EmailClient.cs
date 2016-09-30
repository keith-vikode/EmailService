using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EmailService.Client
{
    /// <summary>
    /// Provides access to the email API. This class is thread-safe.
    /// </summary>
    public sealed class EmailClient : IDisposable
    {
        private readonly EmailClientOptions _options;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly ValidationContext _validationContext;

        private bool _disposed;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailClient"/> class.
        /// </summary>
        /// <param name="options">Configuration settings.</param>
        /// <param name="loggerFactory">Logging factory for diagnostic services</param>
        public EmailClient(IOptions<EmailClientOptions> options, ILoggerFactory loggerFactory)
        {
            _options = options.Value;
            _httpClient = CreateClient();
            _logger = loggerFactory.CreateLogger<EmailClient>();
            _validationContext = new ValidationContext(this);
        }
        
        /// <summary>
        /// Sends a new email request to the queue.
        /// </summary>
        /// <param name="args">Email request parameters</param>
        /// <returns>The token for the request.</returns>
        public async Task<RequestToken> SendAsync(EmailParameters args)
        {
            ValidateArgs(args);

            // send the request as form encoded pairs
            _logger.LogInformation("Sending request to {0}{1}", _options.ServerUrl, _options.MessagesApi);

            HttpContent content = args.ToContent();
            var response = await _httpClient.PostAsync(_options.MessagesApi, content);

            // throw an exception if we received anything other than 202
            response.EnsureSuccessStatusCode();

            // return the token value
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<RawEmailResponse>(json);
            return new RequestToken(obj.Token);
        }

        /// <summary>
        /// Lists all available templates for the current application.
        /// </summary>
        /// <returns>A dictionary of template ID to template name.</returns>
        public async Task<Dictionary<Guid, string>> ListTemplatesAsync()
        {
            var response = await _httpClient.GetAsync(_options.TemplatesApi);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Dictionary<Guid, string>>(json);
        }
    
        /// <summary>
        /// Disposes of this client.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
            else
            {
                throw new ObjectDisposedException(nameof(_httpClient), "The inner HTTP client has already been disposed");
            }
        }
        
        private HttpClient CreateClient()
        {
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(_options.ApplicationId, _options.ApiKey),
                ClientCertificateOptions = ClientCertificateOption.Automatic
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(_options.ServerUrl)
            };

            return client;
        }

        private void ValidateArgs(IValidatableObject args)
        {
            var results = args.Validate(_validationContext);
            if (results.Any())
            {
                throw new Exception("Arguments are not valid");
            }
        }

        private class RawEmailResponse
        {
            public string Token { get; set; }
        }
    }
}
