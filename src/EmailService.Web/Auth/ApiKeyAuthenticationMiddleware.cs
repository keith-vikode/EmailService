using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EmailService.Core.Services;

namespace EmailService.Web.Auth
{
    public class ApiKeyAuthenticationMiddleware : AuthenticationMiddleware<ApiKeyAuthenticationOptions>
    {
        private IApplicationKeyStore _keyStore;
        private ICryptoServices _crypto;

        public ApiKeyAuthenticationMiddleware(
            IApplicationKeyStore keyStore,
            ICryptoServices crypto,
            RequestDelegate next,
            IOptions<ApiKeyAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder)
            : base(next, options, loggerFactory, encoder)
        {
            _keyStore = keyStore;
            _crypto = crypto;
        }

        protected override AuthenticationHandler<ApiKeyAuthenticationOptions> CreateHandler()
        {
            return new ApiKeyAuthenticationHandler(_keyStore, _crypto);
        }
    }
}
