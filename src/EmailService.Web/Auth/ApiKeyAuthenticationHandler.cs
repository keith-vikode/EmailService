using EmailService.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace EmailService.Web.Auth
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private static readonly char[] AuthSplitChars = { ':' };

        private readonly ICryptoServices _crypto;
        private readonly IApplicationKeyStore _keyStore;

        public ApiKeyAuthenticationHandler(IApplicationKeyStore keyStore, ICryptoServices crypto)
        {
            _keyStore = keyStore;
            _crypto = crypto;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            byte[] apiKey;
            Guid applicationId;

            if (!TryGetApiKey(out applicationId, out apiKey))
            {
                return AuthenticateResult.Fail("Missing or malformed 'Authorization' header.");
            }
            
            if (!await TryVerifyApiKeyAsync(applicationId, apiKey))
            {
                return AuthenticateResult.Fail("Invalid API Key.");
            }

            // TODO: it would be good to add the application name etc in here as claims
            var identity = new ClaimsIdentity(Options.AuthenticationScheme, Options.ApplicationIdClaim, null);
            identity.AddClaim(new Claim(Options.ApplicationIdClaim, applicationId.ToString()));
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, null, Options.AuthenticationScheme);
            return AuthenticateResult.Success(ticket);
        }

        protected override Task<bool> HandleForbiddenAsync(ChallengeContext context)
        {
            return base.HandleForbiddenAsync(context);
        }

        public bool TryGetApiKey(out Guid applicationId, out byte[] key)
        {
            StringValues keyStrings;
            
            // we're expecting an auth header in the form of <GUID>:<base64 key>
            if (Context.Request.Headers.TryGetValue(Options.AuthorizationHeader, out keyStrings))
            {
                var keyString = keyStrings.FirstOrDefault();
                var parts = keyString.Split(AuthSplitChars);
                if (parts.Length == 2 && Guid.TryParse(parts[0], out applicationId))
                {
                    try
                    {
                        key = Convert.FromBase64String(parts[1]);
                        return true;
                    }
                    catch (FormatException)
                    {
                    }
                }
            }

            key = null;
            return false;
        }

        public async Task<bool> TryVerifyApiKeyAsync(Guid applicationId, byte[] apiKey)
        {
            // TODO: consider caching the keys for a short while
            var keys = await _keyStore.GetKeysAsync(applicationId);
            if (keys != null)
            {
                return
                    _crypto.VerifyApiKey(applicationId, apiKey, keys.Item1) ||
                    _crypto.VerifyApiKey(applicationId, apiKey, keys.Item2);
            }

            return false;
        }
    }
}
