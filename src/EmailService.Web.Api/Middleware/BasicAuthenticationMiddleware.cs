using EmailService.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace EmailService.Web.Api.Middleware
{
    public class BasicAuthenticationMiddleware : AuthenticationMiddleware<BasicAuthenticationOptions>
    {
        private IApplicationKeyStore _keyStore;
        private ICryptoServices _crypto;
        
        public BasicAuthenticationMiddleware(
            IApplicationKeyStore keyStore,
            ICryptoServices crypto,
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            IOptions<BasicAuthenticationOptions> options)
            : base(next, options, loggerFactory, encoder)
        {
            _crypto = crypto;
            _keyStore = keyStore;
        }
        
        protected override AuthenticationHandler<BasicAuthenticationOptions> CreateHandler()
        {
            return new BasicAuthenticationHandler(_crypto, _keyStore);
        }
    }
}
