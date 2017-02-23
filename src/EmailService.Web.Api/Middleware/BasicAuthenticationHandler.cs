using EmailService.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EmailService.Web.Api.Middleware
{
    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private const string Basic = nameof(Basic);

        private readonly ICryptoServices _crypto;
        private readonly IApplicationKeyStore _keyStore;

        public BasicAuthenticationHandler(ICryptoServices crypto, IApplicationKeyStore keyStore)
        {
            _crypto = crypto;
            _keyStore = keyStore;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorizationHeader = Request.Headers[HeaderNames.Authorization];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return AuthenticateResult.Fail("No authorization header.");
            }

            if (!authorizationHeader.StartsWith($"{Basic} ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Success(ticket: null);
            }

            string encodedCredentials = encodedCredentials = authorizationHeader.Substring(Basic.Length).Trim();

            if (string.IsNullOrEmpty(encodedCredentials))
            {
                const string noCredentialsMessage = "No credentials";
                Logger.LogInformation(noCredentialsMessage);
                return AuthenticateResult.Fail(noCredentialsMessage);
            }

            try
            {
                string decodedCredentials = string.Empty;
                try
                {
                    decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to decode credentials : {encodedCredentials}", ex);
                }

                var delimiterIndex = decodedCredentials.IndexOf(':');
                if (delimiterIndex == -1)
                {
                    const string missingDelimiterMessage = "Invalid credentials, missing delimiter.";
                    Logger.LogInformation(missingDelimiterMessage);
                    return AuthenticateResult.Fail(missingDelimiterMessage);
                }

                var username = decodedCredentials.Substring(0, delimiterIndex);
                var password = decodedCredentials.Substring(delimiterIndex + 1);
                
                AuthenticationTicket ticket = await ValidateCredentialsAsync(username, password);

                if (ticket != null)
                {
                    Logger.LogInformation($"Credentials validated for {username}");
                    return AuthenticateResult.Success(ticket);
                }
                else
                {
                    Logger.LogInformation($"Credential validation failed for {username}");
                    return AuthenticateResult.Fail("Invalid credentials.");
                }
            }
            catch (Exception ex)
            {
                //var authenticationFailedContext = new AuthenticationFailedContext(Context, Options)
                //{
                //    Exception = ex
                //};

                //await Options.Events.AuthenticationFailed(authenticationFailedContext);
                //if (authenticationFailedContext.HandledResponse)
                //{
                //    return AuthenticateResult.Success(authenticationFailedContext.Ticket);
                //}
                //if (authenticationFailedContext.Skipped)
                //{
                //    return AuthenticateResult.Success(ticket: null);
                //}

                Logger.LogError(ex.ToString());
                throw;
            }
        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            var authResult = await HandleAuthenticateOnceAsync();

            Context.Response.StatusCode = 401;

            var headerValue = Basic;
            if (!string.IsNullOrWhiteSpace(Options.Realm))
            {
                headerValue += $" realm=\"{Options.Realm}\"";
            };

            Context.Response.Headers[HeaderNames.WWWAuthenticate] = headerValue;

            return false;
        }

        protected override Task<bool> HandleForbiddenAsync(ChallengeContext context)
        {
            Response.StatusCode = 403;
            return Task.FromResult(true);
        }

        protected override Task HandleSignOutAsync(SignOutContext context)
        {
            throw new NotSupportedException();
        }

        protected override Task HandleSignInAsync(SignInContext context)
        {
            throw new NotSupportedException();
        }

        private async Task<AuthenticationTicket> ValidateCredentialsAsync(string username, string password)
        {
            Guid applicationId;
            if (Guid.TryParse(username, out applicationId))
            {
                var keys = await _keyStore.GetKeysAsync(applicationId);
                if (keys != null)
                {
                    // we can validate against either the primary or secondary key
                    if (_crypto.VerifyApiKey(applicationId, password, keys.PrimaryApiKey) ||
                        _crypto.VerifyApiKey(applicationId, password, keys.SecondaryApiKey))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, keys.ApplicationName),
                            new Claim(ClaimTypes.NameIdentifier, keys.ApplicationId.ToString())
                        };

                        var identity = new ClaimsIdentity(claims, Basic);
                        var principal = new ClaimsPrincipal(identity);
                        return new AuthenticationTicket(principal, null, Basic);
                    }
                }
            }

            return null;
        }
    }
}
